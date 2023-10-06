using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace CommentedNews_Functions
{
    public class Scrape
    {
        private readonly ArticleContext _context;

        public Scrape(ArticleContext context)
        {
            _context = context;
        }

        [FunctionName("Scrape")]
        public void Run([TimerTrigger("0 0/15 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Scraping at: {DateTime.Now}");

            //var article = new Article();
            //article.ArticleTitle = "Lorem Ipsum";
            //article.ArticleUrl = "www.dr.dk";
            //article.ArticleThumbnail = "google images";
            //article.ThreadUrl = "www.tv2.dk";
            //article.ThreadComments = 33;
            //article.ThreadTimestamp = DateTime.Now;
            
            //_context.Article.Add(article);
            //_context.SaveChanges();
        }
    }
}
