using System;
using System.Collections.Generic;
using System.Linq;
using CommentedNews_Functions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CommentedNews_Functions
{
    public class CleanDatabaseFunc
    {
        private readonly ArticleContext _context;

        public CleanDatabaseFunc(ArticleContext context)
        {
            _context = context;
        }

        [FunctionName("CleanDB")] 
        public void Run([TimerTrigger("0 1 0 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Cleaning db at: {DateTime.Now}");
        
            try
            {
                List<Article> articles = _context.Article.ToList<Article>();
        
                DateTime today = Utils.GetDay();
        
                foreach (Article article in articles)
                {
                    if(today.Subtract(article.ThreadTimestamp).Days >= 7)
                    {
                        _context.Remove<Article>(article);
                    }
                }
        
                _context.SaveChanges();
            } catch (Exception ex)
            {
                log.LogInformation($"Cleaning db failed at: {DateTime.Now}");
                log.LogInformation(ex.ToString());
            }
        }
    }
}
