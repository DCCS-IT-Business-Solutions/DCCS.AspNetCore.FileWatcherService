# DCCS.FileWatcherService [![Build status](https://ci.appveyor.com/api/projects/status/lfdrbppktadocb0t?svg=true)](https://ci.appveyor.com/project/mgeramb/dccs-aspnetcore-filewatcherservice) [![NuGet Badge](https://buildstats.info/nuget/DCCS.AspNetCore.FileWatcherService)](https://www.nuget.org/packages/DCCS.AspNetCore.FileWatcherService/)
DCCS.FileWatcherService provide a simple configureable file watcher service which calls a function with a short deplay to handle multiple file changes in a short timer period. The callback can be specified as delegate in the startup of the your webproject or as url in the configuration.

## Installation

TBD

## Examples

Include in startup:
```csharp
services.AddDccsBuildingBlockFileWatcherService()
.AddHandler("Excel Import", (o,a) => { /* called for each change */ } )
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
                "CallbackURL": "http://localhost/MakeTextImport"
            },
            {
                "Name": "Excel Import",
                "Directory": "C:\\ImportExcel",
                "SearchPattern": "*.xlsx",
                "SearchRegExPattern": ".*",
            }
        ],        
        "DelayInMS": 500 
    }
}
```

## Contributing
All contributors are welcome to improve this project. The best way would be to start a bug or feature request and discuss the way you want find a solution with us.
After this process, please create a fork of the repository and open a pull request with your changes. Of course, if your changes are small, feel free to immediately start your pull request without previous discussion. 
