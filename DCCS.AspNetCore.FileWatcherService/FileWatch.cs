using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

namespace DCCS.AspNetCore.FileWatcherService
{
 
    public  class FileWatch : IFileWatch
    {
        private FileSystemWatcher _watcher;
        private volatile Timer _changeTimer;
     
        private readonly FileWatchSetting _setting;
        private volatile FileWatcherEventArgs _nextArgs;

        public FileWatch(FileWatchSetting setting)
        {
            this._setting = setting;
            if (string.IsNullOrEmpty(setting.Directory))
                throw new Exception($"{nameof(setting.Directory)} must not be empty or null");
            if (setting.DelayInMS < 0)
                throw new Exception($"{nameof(setting.DelayInMS)} must greater or equal 0");
        }

        public string Name => _setting.Name;
        public EventHandler<FileWatcherEventArgs> Changed { get; set; }

        public bool Started => _watcher != null;

        protected virtual void OnChanged(FileWatcherEventArgs args)
        {
            Changed?.Invoke(this, args);
            if (!string.IsNullOrEmpty(_setting.CallbackUrl))
            {
                var request = WebRequest.Create(_setting.CallbackUrl);
                if (!string.IsNullOrEmpty(_setting.CallbackMethod))
                    request.Method = _setting.CallbackMethod;
                if (!request.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
                {
                    request.ContentType = "application/json";
                    var dataContractJsonSerializer = new DataContractJsonSerializer(args.GetType(), new DataContractJsonSerializerSettings { SerializeReadOnlyTypes = true });
                    using (var requestStream = request.GetRequestStream())
                    {
                        dataContractJsonSerializer.WriteObject(requestStream, args);
                    }
                }
                request.GetResponseAsync();
            }
        }

        public void StartWatching()
        {
            if (Started)
                throw new Exception("Watching already started");
            try
            {
                _watcher = new FileSystemWatcher();
                Directory.CreateDirectory(_setting.Directory);
                _watcher.Path = _setting.Directory;
                if (!string.IsNullOrEmpty(_setting.SearchPattern))
                    _watcher.Filter = _setting.SearchPattern;
                else
                    _watcher.Filter = "*.*";
                if (_setting.NotifiyChange)
                {
                    _watcher.Changed += OnChanged;
                    _watcher.Renamed += OnChanged;
                }
                if (_setting.NotifiyNew)
                    _watcher.Created += OnChanged;
                if (_setting.NotifiyDelete)
                    _watcher.Deleted += OnChanged;
                _watcher.EnableRaisingEvents = true;
            }
            catch
            {
                StopWatching();
                throw;
            }
        }

        public void StopWatching()
        {
            var temp = _watcher;
            _watcher = null;
            if (temp != null)
            {
                temp.Changed -= OnChanged;
                temp.Dispose();
            }
            var tempTimer = _changeTimer;
            _changeTimer = null;
            if (tempTimer != null)
            {
                tempTimer.Dispose();
            }
        }

        void OnChanged(object source, FileSystemEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_setting.SearchRegExPattern))
            {
                if (!Regex.IsMatch(e.Name, _setting.SearchRegExPattern))
                    return;
            }
            lock (this)
            {
                if (_nextArgs == null)
                    _nextArgs = new FileWatcherEventArgs() { ChangedFiles = new string[0], DeletedFiles = new string[0], NewFiles = new string[0] };

                if ((e.ChangeType & WatcherChangeTypes.Created) == WatcherChangeTypes.Created || (e.ChangeType & WatcherChangeTypes.Renamed) == WatcherChangeTypes.Renamed)
                {
                    if (e is RenamedEventArgs renamedEventArgs)
                    {
                        if (_nextArgs.NewFiles.Contains(renamedEventArgs.OldFullPath))
                            _nextArgs.NewFiles = _nextArgs.NewFiles.Where(n => n != renamedEventArgs.OldFullPath).ToArray(); // Delete from new files
                        if (_nextArgs.ChangedFiles.Contains(renamedEventArgs.OldFullPath))
                            _nextArgs.ChangedFiles = _nextArgs.ChangedFiles.Where(n => n != renamedEventArgs.OldFullPath).ToArray(); // Delete from changed files

                        if (_setting.NotifiyDelete)
                            _nextArgs.DeletedFiles = _nextArgs.DeletedFiles.Concat(new string[] { renamedEventArgs.OldFullPath }).Distinct().ToArray();
                    }
                    if (_nextArgs.DeletedFiles.Contains(e.FullPath))
                        _nextArgs.DeletedFiles = _nextArgs.DeletedFiles.Where(n => n != e.FullPath).ToArray(); // Delete from deleted files

                    _nextArgs.NewFiles = _nextArgs.NewFiles.Concat(new string[] { e.FullPath }).Distinct().ToArray();
                }
                if ((e.ChangeType & WatcherChangeTypes.Changed) == WatcherChangeTypes.Changed)
                {
                    if (!_nextArgs.NewFiles.Contains(e.FullPath))
                        _nextArgs.ChangedFiles = _nextArgs.ChangedFiles.Concat(new string[] { e.FullPath }).Distinct().ToArray();
                }
                if ((e.ChangeType & WatcherChangeTypes.Deleted) == WatcherChangeTypes.Deleted)
                {
                    bool wasNew = false;
                    if (_nextArgs.NewFiles.Contains(e.FullPath))
                    {
                        _nextArgs.NewFiles = _nextArgs.NewFiles.Where(n => n != e.FullPath).ToArray(); // Delete from new files
                        wasNew = true;
                    }
                    if (_nextArgs.ChangedFiles.Contains(e.FullPath))
                        _nextArgs.ChangedFiles = _nextArgs.ChangedFiles.Where(n => n != e.FullPath).ToArray(); // Delete from changed files

                    if (!wasNew)
                        _nextArgs.DeletedFiles = _nextArgs.DeletedFiles.Concat(new string[] { e.FullPath }).Distinct().ToArray();
                }

                if (_setting.DelayInMS > 0 || _setting.DelayInMS == null)
                {
                    if (_changeTimer == null)
                    {
                        _changeTimer = new Timer();
                        _changeTimer.Interval = _setting.DelayInMS ?? 500;
                        _changeTimer.AutoReset = false;
                        _changeTimer.Elapsed += _changeTimer_Elapsed;
                        _changeTimer.Start();
                    }
                }
            }
            if (_setting.DelayInMS <= 0)
            {
                SendChanges();
            }
        }

        private void _changeTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (this)
            {
                _changeTimer = null;
            }
            SendChanges();
        }

        private void SendChanges()
        {
            FileWatcherEventArgs args;
            lock (this)
            {
                args = _nextArgs;
                _nextArgs = null;
            }

            if (args != null && (args.NewFiles.Length > 0 || args.ChangedFiles.Length > 0 || args.DeletedFiles.Length > 0))
            {
                OnChanged(args);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                StopWatching();
                Changed = null;
                disposedValue = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}

