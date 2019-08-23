# DCCS.FileWatcherService [![Build status](https://ci.appveyor.com/api/projects/status/lfdrbppktadocb0t?svg=true)](https://ci.appveyor.com/project/mgeramb/dccs-aspnetcore-filewatcherservice) [![NuGet Badge](https://buildstats.info/nuget/DCCS.AspNetCore.FileWatcherService)](https://www.nuget.org/packages/DCCS.AspNetCore.FileWatcherService/)
DCCS.FileWatcherService provide a simple configureable file watcher service which fires a notification with a short deplay to handle multiple file changes in a short time period. The notification handler can be specified as delegate in the startup of the your project or as http url in the configuration.

## Installation

Install [DCCS.AspNetCore.FileWatcherService](https://www.nuget.org/packages/DCCS.AspNetCore.FileWatcherService/) with NuGet:

    Install-Package DCCS.AspNetCore.FileWatcherService

Or via the .NET Core command line interface:

    dotnet add package DCCS.AspNetCore.FileWatcherService

Either commands, from Package Manager Console or .NET Core CLI, will download and install DCCS.AspNetCore.FileWatcherService and all required dependencies.


## Usage

Include in startup:
```csharp
public void ConfigureServices(IServiceCollection services)
{
     services.AddDccsBuildingBlockFileWatcherService(Configuration)
     // Optional call AddFileWatch to add an none configureable fileWatcher    
    .AddFileWatch(new FileWatch(new FileWatchSetting { Name = "Word change", Directory = @"C:\Docs", CallbackUrl = "http://localhost/ImportWord" })); 
     // Add NotificationHandler is optional and can be used to add C# delegates as callback
    .AddNotificationHandler("Excel Change", (o, a) => { /* called for each change */ }) 
}
```

A watch configruation section can be placed in the "FileWatcherService" node or as array under "Watches"
All watch configuration options:
```json
{
    "Name": "<Name of the watcher>", // Required for array elements. Default under the root node: "#"
    "Directory": "C:\\ImportText", // Required. Directory which will be observed
    "SearchPattern": "*.txt", // Optional. Simple file name pattern with * and ? wildcards
    "SearchRegExPattern": ".*", // Optional. Regular expression search pattern
    "CallbackUrl": "http://localhost/MakeTextImport", // Optional. URL which will be called for a change
    "CallbackMethod": "POST", // Optional, Default: GET, Values: POST|PUT|DELETE|GET Note: No filename information is provided for method GET
    "NotifiyDelete": false, // Optional, Default: true
    "NotifiyChange": false, // Optional, Default: true
    "NotifiyNew": true, // Optional, Default: true
    "DelayInMS": <Number> // Optional, Default: 500, Number of milliseconds for the delay for the notification, so that multiple changes will be reported in one notification
}
```

## Example
Configuration file section:
```json
{
    "FileWatcherService": 
    {
        "Watches":
        [ 
            {
                "Name": "Text Import",
                "Directory": "C:\\ImportText",
                "SearchPattern": "*.txt",
                "SearchRegExPattern": ".*",
                "CallbackUrl": "http://localhost/MakeTextImport",
                "CallbackMethod": "POST", 
                "NotifiyDelete": false,
                "NotifiyChange": false,
                "NotifiyNew": true,
            },
            {
                "Name": "Excel Change",
                "Directory": "C:\\ExcelFiles",
                "SearchPattern": "*.xlsx",
                "SearchRegExPattern": ".*",
                "DelayInMS": 600
            }
        ],        
        "DefaultDelayInMS": 500 
    }
}
```

A full working sample is included in the source and can be found [here](https://github.com/DCCS-IT-Business-Solutions/DCCS.AspNetCore.FileWatcherService/tree/master/DCCS.AspNetCore.FileWatcherService.Sample).

## Contributing
All contributors are welcome to improve this project. The best way would be to start a bug or feature request and discuss the way you want find a solution with us.
After this process, please create a fork of the repository and open a pull request with your changes. Of course, if your changes are small, feel free to immediately start your pull request without previous discussion. 
