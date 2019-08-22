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
        public const string DefaultConfigSectionName = "FileWatcherService";
        private readonly Dictionary<string, IWatch> _watches = new Dictionary<string, IWatch>();
        private readonly IConfiguration _configuration;

        public FileWatcherService(IConfiguration configuration, string configurationSectionName = DefaultConfigSectionName)
        {
            _configuration = configuration;
            const string watchesNodeName = "Watches";

            var settings = _configuration.GetSection(configurationSectionName)?.GetSection(watchesNodeName)?.Get<WatchSetting[]>();
                  
            const string defaultDelayNodeName = "DefaultDelayInMS";
            var delaySection = _configuration.GetSection(configurationSectionName)?.GetSection(defaultDelayNodeName)?.Get<int>();
            if (delaySection != null)
            {
                foreach (var watchSetting in settings)
                {
                    watchSetting.DelayInMS = delaySection.Value < 0 ? 0 : delaySection.Value;
                }
            }
            if (settings != null)
            {
                for (int i = 0; i < settings.Length; i++)
                {
                    var setting = settings[i];
                    if (string.IsNullOrEmpty(setting.Name))
                        throw new Exception($"Configuration block {configurationSectionName}/{watchesNodeName}[{i}] has no or an empty {nameof(WatchSetting.Name)} property");
                    var watch = new Watch(setting);
                    AddWatch(watch);
                    watch.StartWatching();
                }
            }

        }

        public void AddWatch(IWatch watch)
        {
            if (_watches.ContainsKey(watch.Name))
                throw new Exception($"Watch with name '{watch.Name}' exist already");
            _watches.Add(watch.Name, watch);
        }

        public void RemoveWatch(string watcherName)
        {
            _watches.Remove(watcherName);
        }

        public IFileWatcherService AddNotificationHandler(string watcherName, EventHandler<FileWatcherEventArgs> callback)
        {
            if (!_watches.ContainsKey(watcherName))
                throw new Exception($"Watch with name '{watcherName}' does not exist");
            var watch = _watches[watcherName];
            watch.Changed += callback;
            return this;
        }

        public IFileWatcherService RemoveNotificationHandler(string watcherName, EventHandler<FileWatcherEventArgs> callback)
        {
            if (!_watches.ContainsKey(watcherName))
                throw new Exception($"Watch with name '{watcherName}' does not exist");
            var watch = _watches[watcherName];
            watch.Changed -= callback;
            return this;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_watches != null)
            {
                foreach (var watch in _watches.Values)
                    watch.StopWatching();
            }
            return Task.CompletedTask;
        }

    }
}
