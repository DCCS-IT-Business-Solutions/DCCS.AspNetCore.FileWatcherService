using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace DCCS.AspNetCore.FileWatcherService
{
    public static class StartupExtension
    {
        public static Watch[] _watches;
        public static IFileWatcherService AddDccsBuildingBlockFileWatcherService(this IServiceCollection services,
            IConfiguration configuration, string configurationSectionName = FileWatcherService.DefaultConfigSectionName)
        {
            var fileWatcherService = new FileWatcherService(configuration, configurationSectionName);
            services.AddTransient<IHostedService, FileWatcherService>(e => fileWatcherService);
            services.AddSingleton<IFileWatcherService>(e => fileWatcherService);
            return fileWatcherService;
        }




    }
}
