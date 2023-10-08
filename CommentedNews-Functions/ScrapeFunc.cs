using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommentedNews_Functions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
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

            foreach (JToken thread in threads)
            {
                if ((string)thread["kind"] == "t3")
                {
                    JToken threadData = thread["data"];
                    string domain = (string)threadData["domain"];

                    if (IsDomainNews(domain) == true)
                    {
                        Article article = new Article();
                        article.ArticleTitle = (string)threadData["title"];

                        if (threadData["url_overridden_by_dest"] != null)
                        {
                            article.ArticleUrl = (string)threadData["url_overridden_by_dest"];
                        }
                        else
                        {
                            article.ArticleUrl = (string)threadData["url"];
                        }

                        article.ArticleThumbnail = (string)threadData["thumbnail"];
                        article.ThreadComments = (int)threadData["num_comments"];
                        article.ThreadUrl = String.Format("{0}{1}", "http://www.reddit.com", threadData["permalink"]);
                        article.ThreadTimestamp = Utils.GetTime((long)threadData["created_utc"], 2);

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
