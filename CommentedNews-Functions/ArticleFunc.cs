using CommentedNews_Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentedNews_Functions
{
    public class ArticleFunc
    {
        private readonly ArticleContext _context;

        public ArticleFunc(ArticleContext context)
        {
            _context = context;
        }

        [FunctionName("Article_Create")]
        public async Task<IActionResult> ArticleCreate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "article")] HttpRequest req,
            ILogger log)
        {
            var headerList = req.Headers.ToList();

            string ArticleUrl = headerList.SingleOrDefault(pair => pair.Key == "ArticleUrl").Value;
            string ArticleTitle = headerList.SingleOrDefault(pair => pair.Key == "ArticleTitle").Value;
            string ArticleThumbnail = headerList.SingleOrDefault(pair => pair.Key == "ArticleThumbnail").Value;
            string ThreadUrl = headerList.SingleOrDefault(pair => pair.Key == "ThreadUrl").Value;
            string ThreadComments = headerList.SingleOrDefault(pair => pair.Key == "ThreadComments").Value;
            string Timestamp = headerList.SingleOrDefault(pair => pair.Key == "Timestamp").Value;

            if(ArticleUrl.Length == 0 || ArticleTitle.Length == 0 || ArticleThumbnail.Length == 0 ||
                ThreadUrl.Length == 0 || ThreadComments.Length == 0 || Timestamp.Length == 0)
            {
                return new BadRequestResult();
            }

            Article article = new Article();
            article.ArticleUrl = ArticleUrl;
            article.ArticleTitle = ArticleTitle;
            article.ArticleThumbnail = ArticleThumbnail;
            article.ThreadUrl = ThreadUrl;
            article.ThreadComments = int.Parse(ThreadComments);
            article.ThreadTimestamp = DateTimeOffset.FromUnixTimeSeconds(long.Parse(Timestamp)).DateTime;

            _context.Article.Add(article);
            _context.SaveChanges();

            return new OkResult();
        }

        [FunctionName("Article_Read")]
        public async Task<IActionResult> ArticleRead(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "article/{id:int}")] HttpRequest req,
            ILogger log, int id)
        {
            Article article = _context.Article.Where(article => article.Id == id).SingleOrDefault();

            if(article != null)
            {
                string json = JsonConvert.SerializeObject(article);
                return new OkObjectResult(json);
            }
            else
            {
                return new NotFoundResult();
            }
        }

        [FunctionName("Article_Update")]
        public async Task<IActionResult> ArticleUpdate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "article/{id:int}")] HttpRequest req,
            ILogger log, int id)
        {
            var headerList = req.Headers.ToList();

            string ArticleUrl = headerList.SingleOrDefault(pair => pair.Key == "ArticleUrl").Value;
            string ArticleTitle = headerList.SingleOrDefault(pair => pair.Key == "ArticleTitle").Value;
            string ArticleThumbnail = headerList.SingleOrDefault(pair => pair.Key == "ArticleThumbnail").Value;
            string ThreadUrl = headerList.SingleOrDefault(pair => pair.Key == "ThreadUrl").Value;
            string ThreadComments = headerList.SingleOrDefault(pair => pair.Key == "ThreadComments").Value;
            string Timestamp = headerList.SingleOrDefault(pair => pair.Key == "Timestamp").Value;

            if (ArticleUrl.Length == 0 || ArticleTitle.Length == 0 || ArticleThumbnail.Length == 0 ||
                ThreadUrl.Length == 0 || ThreadComments.Length == 0 || Timestamp.Length == 0)
            {
                return new BadRequestResult();
            }

            Article article = _context.Article.Where(article => article.Id == id).SingleOrDefault();

            if(article != null)
            {
                article.ThreadComments = int.Parse(ThreadComments);
                _context.SaveChanges();
                return new OkResult();
            }
            else
            {
                return new NotFoundResult();
            }

        }

        [FunctionName("Article_Delete")]
        public async Task<IActionResult> ArticleDelete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "article/{id:int}")] HttpRequest req,
            ILogger log, int id)
        {
            Article article = _context.Article.Where(article => article.Id == id).SingleOrDefault();

            if (article != null)
            {
                _context.Remove(article);
                _context.SaveChanges();
                return new OkResult();
            }
            else
            {
                return new NotFoundResult();
            }
        }
    }
}
