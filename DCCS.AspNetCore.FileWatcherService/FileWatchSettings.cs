using System;
using System.Collections.Generic;
using System.Text;

namespace DCCS.AspNetCore.FileWatcherService
{
    public class FileWatchSetting
    {
        public FileWatchSetting()
        {

        }

        public FileWatchSetting(string name, string directory)
        {
            Name = name;
            Directory = directory;
        }

        public string Directory { get; set; }
        public string Name { get; set; }
        public string SearchPattern { get; set; }
        public string SearchRegExPattern { get; set; }
        public int? DelayInMS { get; set; }
        public string CallbackUrl { get; set; }
        public string CallbackMethod { get; set; } = "GET";
        public bool NotifiyDelete { get; set; } = true;
        public bool NotifiyChange { get; set; } = true;
        public bool NotifiyNew { get; set; } = true;
    }
}
