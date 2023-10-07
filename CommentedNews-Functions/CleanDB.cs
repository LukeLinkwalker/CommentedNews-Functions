using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CommentedNews_Functions
{
    public class CleanDB
    {
        private readonly ArticleContext _context;

        public CleanDB(ArticleContext context)
        {
            _context = context;
        }

        [FunctionName("CleanDB")]
        public void Run([TimerTrigger("0 0 0 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Cleaning db at: {DateTime.Now}");
        
            List<Article> articles = _context.Article.ToList<Article>();
        
            DateTime today = Utils.GetDay();
        
            foreach (Article article in articles)
            {
                if(today.Subtract(article.ThreadTimestamp).Days > 7)
                {
                    _context.Remove<Article>(article);
                }
            }
        
            _context.SaveChanges();
        }
    }
}
