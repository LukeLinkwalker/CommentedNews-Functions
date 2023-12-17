using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using CommentedNews_Functions.Entities;

namespace CommentedNews_Functions
{
    public class ArticlesFunc
    {
        private readonly ArticleContext _context;

        public ArticlesFunc(ArticleContext context)
        {
            _context = context;
        }

        [FunctionName("Articles_all")]
        public async Task<IActionResult> AllArticles(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "articles")] HttpRequest req,
            ILogger log)
        {
            List<Article> articles = _context.Article.ToList<Article>();
            string json = JsonConvert.SerializeObject(articles);
            return new OkObjectResult(json);
        }

        [FunctionName("Articles_day")]
        public async Task<IActionResult> ArticlesFromDay(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "articles/{day:int}")] HttpRequest req,
            ILogger log,
            int day)
        {
            List<Article> articles = _context.Article.Where(article => article.ThreadTimestamp.Day == day).ToList();
            string json = JsonConvert.SerializeObject(articles);
            return new OkObjectResult(json);
        }
    }
}
