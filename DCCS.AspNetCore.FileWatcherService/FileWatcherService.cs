using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DCCS.AspNetCore.FileWatcherService
{

    public class FileWatcherService : IFileWatcherService, IHostedService
    {
        private const string DefaultConfigSectionName = "FileWatcherService";
        private readonly Dictionary<string, IWatch> _watches = new Dictionary<string, IWatch>();
        private IConfiguration _configuration;

        public FileWatcherService(IConfiguration configuration)
        {
            _configuration = configuration;


        }

        public FileWatcherService Initialize(string configurationSectionName = DefaultConfigSectionName)
        {
            const string watchesNodeName = "Watches";

            var settings = _configuration.GetSection(configurationSectionName).GetSection(watchesNodeName).Get<WatchSetting[]>();

      
            const string delayNodeName = "DelayInMS";
            var delaySection = _configuration.GetSection(configurationSectionName).GetSection(delayNodeName).Get<int>();

            foreach (var watchSetting in settings)
            {
                watchSetting.DelayInMS = delaySection <= 0 ? 1 : delaySection;
            }



            for (int i = 0; i < settings.Length; i++)
            {
                var setting = settings[i];
                if (string.IsNullOrEmpty(setting.Name))
                    throw new Exception($"Configuration block {configurationSectionName}/{watchesNodeName}[{i}] has no or an empty {nameof(WatchSetting.Name)} property");
                var watch = new Watch(setting);
                AddWatch(watch);
                watch.StartWatching();
            }

            return this;
        }

        public void AddWatch(IWatch watch)
        {
            if (_watches.ContainsKey(watch.Name))
                throw new Exception($"Watch with name '{watch.Name}' exist already");
            _watches.Add(watch.Name, watch);
        }

        public FileWatcherService AddHandler(string name, EventHandler<FileWatcherEventArgs> callback)
        {
            if (!_watches.ContainsKey(name))
                throw new Exception($"Watch with name '{name}' does not exist");
            var watch = _watches[name];
            watch.Changed += callback;
            return this;
        }

        public FileWatcherService RemoveHandler(string name, EventHandler<FileWatcherEventArgs> callback)
        {
            if (!_watches.ContainsKey(name))
                throw new Exception($"Watch with name '{name}' does not exist");
            var watch = _watches[name];
            watch.Changed -= callback;
            return this;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine("Timed Background Service is starting.");

            DoWork();

            return Task.CompletedTask;
        }

        private void DoWork()
        {
            System.Diagnostics.Debug.WriteLine("Timed Background Service is working.");
            this.Initialize();

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine("Timed Background Service is stopping.");



            return Task.CompletedTask;
        }

    }
}
