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

namespace CommentedNews_Functions
{
    public class Articles
    {
        private readonly ArticleContext _context;
        private readonly IConfiguration _configuration;

        public Articles(ArticleContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [FunctionName("GetArticles")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            List<Article> articles = _context.Articles.ToList<Article>();
            return new OkObjectResult("Number of articles: " + articles.Count);
        }
    }
}
