using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace DCCS.AspNetCore.FileWatcherService
{
    public static class StartupExtension
    {
        public static Watch[] _watches;
        public static IFileWatcherService AddDccsBuildingBlockFileWatcherService(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<IFileWatcherService, FileWatcherService>();
            FileWatcherService fileWatcher = new FileWatcherService();
            fileWatcher.Initialize(configuration);
            return fileWatcher;
        }




    }
}
