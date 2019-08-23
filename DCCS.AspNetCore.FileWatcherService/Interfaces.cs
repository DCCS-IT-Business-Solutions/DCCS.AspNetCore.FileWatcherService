using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace DCCS.AspNetCore.FileWatcherService
{
    public interface IFileWatcherService
    {
        void AddFileWatch(IFileWatch watch);
        void RemoveFileWatch(string watcherName);
        IFileWatcherService AddNotificationHandler(EventHandler<FileWatcherEventArgs> callback);
        IFileWatcherService AddNotificationHandler(string watcherName, EventHandler<FileWatcherEventArgs> callback);
        IFileWatcherService RemoveNotificationHandler(EventHandler<FileWatcherEventArgs> callback);
        IFileWatcherService RemoveNotificationHandler(string watcherName, EventHandler<FileWatcherEventArgs> callback);
    }

    public interface IFileWatch : IDisposable
    {
        string Name { get; }
        EventHandler<FileWatcherEventArgs> Changed { get; set; }
        bool Started { get; }

        void StartWatching();
        void StopWatching();
    }

    [DataContract]
    public class FileWatcherEventArgs : EventArgs
    {
        [DataMember]
        public string[] NewFiles { get; internal set; }
        [DataMember]
        public string[] ChangedFiles { get; internal set; }
        [DataMember]
        public string[] DeletedFiles { get; internal set; }
    }

}
