# DCCS.FileWatcherService [![Build status](https://ci.appveyor.com/api/projects/status/lfdrbppktadocb0t?svg=true)](https://ci.appveyor.com/project/mgeramb/dccs-aspnetcore-filewatcherservice) [![NuGet Badge](https://buildstats.info/nuget/DCCS.AspNetCore.FileWatcherService)](https://www.nuget.org/packages/DCCS.AspNetCore.FileWatcherService/)
DCCS.FileWatcherService provide a simple configureable file watcher service which calls a function with a short deplay to handle multiple file changes in a short timer period. The callback can be specified as delegate in the startup of the your webproject or as url in the configuration.

## Installation

Install [DCCS.AspNetCore.FileWatcherService](https://www.nuget.org/packages/DCCS.AspNetCore.FileWatcherService/) with NuGet:

    Install-Package DCCS.AspNetCore.FileWatcherService

Or via the .NET Core command line interface:

    dotnet add package DCCS.AspNetCore.FileWatcherService

Either commands, from Package Manager Console or .NET Core CLI, will download and install DCCS.AspNetCore.FileWatcherService and all required dependencies.


## Examples

Include in startup:
```csharp
services.AddDccsBuildingBlockFileWatcherService()
.AddNotificationHandler("Excel Change", (o,a) => { /* called for each change */ } )
```

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
