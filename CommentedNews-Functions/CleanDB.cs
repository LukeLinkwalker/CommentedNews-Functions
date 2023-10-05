using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CommentedNews_Functions
{
    public class CleanDB
    {
        [FunctionName("CleanDB")]
        public void Run([TimerTrigger("0 0 0 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Cleaning db at: {DateTime.Now}");
        }
    }
}
