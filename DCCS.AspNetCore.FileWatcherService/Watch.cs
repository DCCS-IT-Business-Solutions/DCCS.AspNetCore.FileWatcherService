using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DCCS.AspNetCore.FileWatcherService
{
 
    public  class Watch : IWatch
    {
        private FileSystemWatcher _watcher;
     
        private readonly WatchSetting _setting;

        public Watch(WatchSetting setting)
        {
            this._setting = setting;
            if (string.IsNullOrEmpty(setting.Directory))
                throw new Exception($"{nameof(setting.Directory)} must not be empty or null");
        }

        public string Name => _setting.Name;
        public EventHandler<FileWatcherEventArgs> Changed { get; set; }

        protected virtual void OnChanged(FileWatcherEventArgs args)
        {
            Changed?.Invoke(this, args);
        }

        public void StartWatching()
        {
            if (_watcher != null)
                throw new Exception("Watching already started");
            _watcher = new FileSystemWatcher();
            _watcher.Path = _setting.Directory;
            _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                                            | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            if (!string.IsNullOrEmpty(_setting.SearchPattern))
                _watcher.Filter = _setting.SearchPattern;
            else
                _watcher.Filter = "*.*";
            _watcher.Changed += OnChanged;
            _watcher.EnableRaisingEvents = true;
        }

        void OnChanged(object source, FileSystemEventArgs e)
        {
            var args = new FileWatcherEventArgs();
            OnChanged(args);

        }
        
    }

}

public class FileWatcherEventArgs : EventArgs
{
    public string [] NewFiles { get; internal set; }
    public string[] ChangedFiles { get; internal set; }
    public string[] DeletedFiles { get; internal set; }
}
