using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CommentedNews_Functions
{
    public class Scrape
    {
        [FunctionName("Scrape")]
        public void Run([TimerTrigger("0 0/15 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Scraping at: {DateTime.Now}");
        }
    }
}
