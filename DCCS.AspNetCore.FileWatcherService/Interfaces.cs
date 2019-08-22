using System;
using System.Collections.Generic;
using System.Text;

namespace DCCS.AspNetCore.FileWatcherService
{
    public interface IFileWatcherService
    {
        void AddWatch(IWatch watch);
        FileWatcherService AddHandler(string name, EventHandler<FileWatcherEventArgs> callback);
    }

    public interface IWatch
    {
        string Name { get; }
        EventHandler<FileWatcherEventArgs> Changed { get; set; }
    }
}
