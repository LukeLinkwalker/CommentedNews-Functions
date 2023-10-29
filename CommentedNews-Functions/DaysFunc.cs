using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CommentedNews_Functions.Entities;
using System.Collections.Generic;
using System.Linq;

namespace CommentedNews_Functions
{
    public class DaysFunc
    {
        private readonly ArticleContext _context;

        public DaysFunc(ArticleContext context)
        {
            _context = context;
        }

        [FunctionName("days")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            List<Article> articles = _context.Article.ToList<Article>(); 
            List<DateTime> days = new List<DateTime>();

            if(articles.Count > 0) 
            { 
                articles = articles.OrderByDescending(article => article.ThreadTimestamp).ToList();
                articles = articles.DistinctBy(article => article.ThreadTimestamp.Day).ToList();
                articles = articles.Take(7).ToList();
                days = articles.Select(article => article.ThreadTimestamp).ToList();
            }

            string json = JsonConvert.SerializeObject(days);
            return new OkObjectResult(json);
        }
    }
}
