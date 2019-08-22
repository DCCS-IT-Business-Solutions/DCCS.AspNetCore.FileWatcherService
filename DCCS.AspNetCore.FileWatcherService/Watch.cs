using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;

namespace DCCS.AspNetCore.FileWatcherService
{
 
    public  class Watch : IWatch
    {
        private FileSystemWatcher _watcher;
        private volatile Timer _changeTimer;
     
        private readonly WatchSetting _setting;
        private volatile FileWatcherEventArgs _nextArgs;

        public Watch(WatchSetting setting)
        {
            this._setting = setting;
            if (string.IsNullOrEmpty(setting.Directory))
                throw new Exception($"{nameof(setting.Directory)} must not be empty or null");
            if (setting.DelayInMS < 0)
                throw new Exception($"{nameof(setting.DelayInMS)} must greater or equal 0");
        }

        public string Name => _setting.Name;
        public EventHandler<FileWatcherEventArgs> Changed { get; set; }

        protected virtual void OnChanged(FileWatcherEventArgs args)
        {
            Changed?.Invoke(this, args);
            if (!string.IsNullOrEmpty(_setting.CallbackUrl))
            {
                var request = WebRequest.Create(_setting.CallbackUrl);
                request.GetResponseAsync();
            }
        }

        public void StartWatching()
        {
            if (_watcher != null)
                throw new Exception("Watching already started");

            _watcher = new FileSystemWatcher();
            Directory.CreateDirectory(_setting.Directory);
            _watcher.Path = _setting.Directory;
            if (!string.IsNullOrEmpty(_setting.SearchPattern))
                _watcher.Filter = _setting.SearchPattern;
            else
                _watcher.Filter = "*.*";
            if (_setting.NotifiyChange)
                _watcher.Changed += OnChanged;
            if (_setting.NotifiyNew)
                _watcher.Created += OnChanged;
            if (_setting.NotifiyDelete)
                _watcher.Deleted += OnChanged;
            _watcher.EnableRaisingEvents = true;
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
                    if (_nextArgs.NewFiles.Contains(e.FullPath))
                        _nextArgs.NewFiles = _nextArgs.NewFiles.Where(n => n != e.FullPath).ToArray(); // Delete from new files
                    if (_nextArgs.ChangedFiles.Contains(e.FullPath))
                        _nextArgs.ChangedFiles = _nextArgs.ChangedFiles.Where(n => n != e.FullPath).ToArray(); // Delete from changed files

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

            if (args != null)
            {
                OnChanged(args);
            }
        }
    }
}

