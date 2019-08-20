# DCCS.FileWatcherService
DCCS.FileWatcherService provide a simple configureable file watcher service which calls a function with a short deplay to handle multiple file changes in a short timer period.

## Installation

TBD

## Examples

Include in startup:
```csharp
// TBD
```

Configuration file section:
```json
{
    "FileWatcherService": 
    {
        "Watches":
        [ 
            {
                "Directory": "C:\\Import",
                "SearchPattern": "*.txt",
                "SearchRegExPattern": ".*",
                "DelayInMS": 500 
            }
        ]        
    }
}
```

## Contributing
All contributors are welcome to improve this project. The best way would be to start a bug or feature request and discuss the way you want find a solution with us.
After this process, please create a fork of the repository and open a pull request with your changes. Of course, if your changes are small, feel free to immediately start your pull request without previous discussion. 
