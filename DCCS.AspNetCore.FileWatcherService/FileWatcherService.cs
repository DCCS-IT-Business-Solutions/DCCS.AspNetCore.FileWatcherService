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

    public class FileWatcherService : IFileWatcherService, IHostedService, IDisposable
    {
        public const string DefaultConfigSectionName = "FileWatcherService";
        private const string RootWatchName = "#";
        private readonly Dictionary<string, IFileWatch> _watches = new Dictionary<string, IFileWatch>();

        public FileWatcherService(IConfiguration configuration, string configurationSectionName = DefaultConfigSectionName)
        {
            const string watchesNodeName = "Watches";
            var rootSection = !string.IsNullOrEmpty(configurationSectionName) ? configuration?.GetSection(configurationSectionName) : null;
            var watchSettings = new List<FileWatchSetting>();
            
            // Check for watch setting in configuration node
            var singleWatchSettings = rootSection?.Get<FileWatchSetting>();
            if (singleWatchSettings != null && !string.IsNullOrEmpty(singleWatchSettings.Directory))
            {
                if (string.IsNullOrEmpty(singleWatchSettings.Name))
                    singleWatchSettings.Name = RootWatchName;
                watchSettings.Add(singleWatchSettings);
            }

            // Check for an array of watch settings in the root node
            var settings = rootSection?.GetSection(watchesNodeName)?.Get<FileWatchSetting[]>();
            if (settings != null)
                watchSettings.AddRange(settings);

            // Handle default deleay      
            const string defaultDelayNodeName = "DefaultDelayInMS";
            var delaySection = configuration?.GetSection(configurationSectionName)?.GetSection(defaultDelayNodeName)?.Get<int>();
            if (delaySection != null)
            {
                foreach (var watchSetting in watchSettings)
                {
                    if (watchSetting.DelayInMS == null)
                        watchSetting.DelayInMS = delaySection.Value < 0 ? 0 : delaySection.Value;
                }
            }

            // Create watches
            if (settings != null)
            {
                for (int i = 0; i < settings.Length; i++)
                {
                    var setting = settings[i];
                    if (string.IsNullOrEmpty(setting.Name))
                        throw new Exception($"Configuration block {configurationSectionName}/{watchesNodeName}[{i}] has no or an empty {nameof(FileWatchSetting.Name)} property");
                    var watch = new FileWatch(setting);
                    AddFileWatch(watch);
                }
            }
            StartWatching();
        }

        private void StartWatching()
        {
            foreach (var watch in _watches.Values)
            {
                if (!watch.Started)
                    watch.StartWatching();
            }
        }

        private void StopWatching()
        {
            foreach (var watch in _watches.Values)
            {
                watch.StopWatching();
            }
        }

        public void AddFileWatch(IFileWatch watch)
        {
            if (_watches.ContainsKey(watch.Name))
                throw new Exception($"Watch with name '{watch.Name}' exist already");
            _watches.Add(watch.Name, watch);
        }

        public void RemoveFileWatch(string watcherName)
        {
            _watches.Remove(watcherName);
        }

        public IFileWatcherService AddNotificationHandler(string watcherName, EventHandler<FileWatcherEventArgs> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            if (string.IsNullOrEmpty(watcherName))
                watcherName = RootWatchName;
            if (!_watches.ContainsKey(watcherName))
                throw new Exception($"Watch with name '{watcherName}' does not exist");
            var watch = _watches[watcherName];
            watch.Changed += callback;
            return this;
        }

        public IFileWatcherService AddNotificationHandler(EventHandler<FileWatcherEventArgs> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            foreach (var watch in _watches.Values)
                watch.Changed += callback;
            return this;
        }

        public IFileWatcherService RemoveNotificationHandler(EventHandler<FileWatcherEventArgs> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            foreach (var watch in _watches.Values)
                watch.Changed -= callback;
            return this;
        }

        public IFileWatcherService RemoveNotificationHandler(string watcherName, EventHandler<FileWatcherEventArgs> callback)
        {
            if (callback == null)
                throw new ArgumentNullException(nameof(callback));
            if (string.IsNullOrEmpty(watcherName))
                watcherName = RootWatchName;
            if (!_watches.ContainsKey(watcherName))
                throw new Exception($"Watch with name '{watcherName}' does not exist");
            var watch = _watches[watcherName];
            watch.Changed -= callback;
            return this;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(StartWatching, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(StopWatching, cancellationToken);
        }

        public void Dispose()
        {
            foreach (var watch in _watches.Values)
                watch.Dispose();
            _watches.Clear();
        }
    }
}
