using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommentedNews_Functions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommentedNews_Functions
{
    public class ScrapeFunc
    {
        private readonly ArticleContext _articleContext;
        private readonly MediaContext _mediaContext;
        private List<string> medier {  get; set; }

        public ScrapeFunc(ArticleContext articleContext, MediaContext mediaContext)
        {
            _articleContext = articleContext;
            _mediaContext = mediaContext;

            var media = _mediaContext.Media.ToList<Media>();
            medier = media.Select(m => m.Name).ToList();
        }

        [FunctionName("Scrape")]
        public async Task Run([TimerTrigger("0 0/15 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Scraping at: {DateTime.Now}");

            string json = await GetJSON(log);
            List<Article> articles = ParseJSON(json);
            articles = articles.OrderByDescending(article => article.ThreadTimestamp).ToList();

            foreach(Article article in articles)
            {
                Article articleInDatabase = _articleContext.Article.SingleOrDefault<Article>(a => a.ArticleUrl == article.ArticleUrl);
                if(articleInDatabase == null)
                {
                    _articleContext.Article.Add(article);
                }
                else
                {
                    articleInDatabase.ThreadComments = article.ThreadComments;
                }
            }

            _articleContext.SaveChanges();
        }

        /// <summary>
        /// Fetches new posts made on the /r/Denmark subreddit through the Reddit API.
        /// </summary>
        private async Task<string> GetJSON(ILogger log)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://www.reddit.com/r/denmark/new");
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
            );
            client.DefaultRequestHeaders.Add("User-Agent", "News_Threads_Scraper");

            HttpResponseMessage response = client.GetAsync(".json?limit=100").Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                log.LogInformation("Http request to reddit API failed.");
            }

            return string.Empty;
        }

        /// <summary>
        /// Parses all threads in the JSON input and extracts the information required for an Article object when a thread contains a news article.
        /// </summary>
        private List<Article> ParseJSON(string json)
        {
            List<Article> articles = new List<Article>();

            var jsonData = JObject.Parse(json);
            var dataObj = jsonData["data"];
            JArray threads = (JArray)dataObj["children"];

            Thread[] t = JsonConvert.DeserializeObject<Thread[]>(threads.ToString());

            foreach(Thread thread in t)
            {
                if (thread.kind == "t3")
                {
                    if(IsDomainNews(thread.data.domain) == true)
                    {
                        Article article = new Article();
                        article.ArticleTitle = thread.data.title;

                        if(thread.data.url_overridden_by_dest != string.Empty)
                        {
                            article.ArticleUrl = thread.data.url_overridden_by_dest;
                        }
                        else
                        {
                            article.ArticleUrl = thread.data.url;
                        }

                        article.ArticleThumbnail = thread.data.thumbnail;
                        article.ThreadComments = thread.data.num_comments;
                        article.ThreadUrl = thread.data.permalink;
                        article.ThreadTimestamp = Utils.GetTime(thread.data.created_utc, 2);

                        articles.Add(article);
                    }
                }
            }

            return articles;
        }

        private bool IsDomainNews(string domain)
        {
            string pattern = @"\w+.dk";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            Match match = regex.Match(domain);

            if (medier.Any(medie => medie == match.Value))
            {
                return true;
            }

            return false;
        }
    }
}
