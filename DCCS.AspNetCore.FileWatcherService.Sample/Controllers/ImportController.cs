using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DCCS.AspNetCore.FileWatcherService.Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        ILogger _logger;
        public ImportController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
        }
        // GET api/values
        [HttpGet("startTextImport")]
        public void StartTextImport()
        {
            _logger.LogInformation("Text import triggered");
        }

    }
}
