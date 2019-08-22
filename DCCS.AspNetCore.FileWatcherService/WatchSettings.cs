using System;
using System.Collections.Generic;
using System.Text;

namespace DCCS.AspNetCore.FileWatcherService
{
    public class WatchSetting
    {
        public string Directory { get; set; }
        public string Name { get; set; }
        public string SearchPattern { get; set; }
        public string SearchRegExPattern { get; set; }
        public int? DelayInMS { get; set; }
        public string CallbackUrl { get; set; }
        public bool NotifiyDelete { get; set; } = true;
        public bool NotifiyChange { get; set; } = true;
        public bool NotifiyNew { get; set; } = true;
    }
}
