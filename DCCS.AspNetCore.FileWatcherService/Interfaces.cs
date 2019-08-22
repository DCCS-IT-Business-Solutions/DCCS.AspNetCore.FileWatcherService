using System;
using System.Collections.Generic;
using System.Text;

namespace DCCS.AspNetCore.FileWatcherService
{
    public interface IFileWatcherService
    {
        void AddWatch(IWatch watch);
        void RemoveWatch(string watcherName);
        IFileWatcherService AddNotificationHandler(string watcherName, EventHandler<FileWatcherEventArgs> callback);
        IFileWatcherService RemoveNotificationHandler(string watcherName, EventHandler<FileWatcherEventArgs> callback);
    }

    public interface IWatch
    {
        string Name { get; }
        EventHandler<FileWatcherEventArgs> Changed { get; set; }
        void StartWatching();
        void StopWatching();
    }

    public class FileWatcherEventArgs : EventArgs
    {
        public string[] NewFiles { get; internal set; }
        public string[] ChangedFiles { get; internal set; }
        public string[] DeletedFiles { get; internal set; }
    }

}
