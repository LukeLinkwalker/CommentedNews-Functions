using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace CommentedNews_Functions
{
    public class Scrape
    {
        private readonly string[] medier = new string[] {
            "tv2.dk",
            "dr.dk",
            "information.dk",
            "altinget.dk",
            "tjekdet.dk",
            "zetland.dk",
            "politiken.dk",
            "ekstrabladet.dk",
            "bt.dk"
        };

        private readonly ArticleContext _context;

        public Scrape(ArticleContext context)
        {
            _context = context;
        }

        [FunctionName("Scrape")]
        public async Task Run([TimerTrigger("0 0/15 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Scraping at: {DateTime.Now}");

            string json = await GetJSON(log);
            List<Article> articles = ParseJSON(json);
            articles = articles.OrderByDescending(article => article.ThreadTimestamp).ToList();

            DateTime today = Utils.GetDay();
            DateTime yesterday = today.AddDays(-1);

            foreach(Article article in articles)
            {
                if (article.ThreadTimestamp.Day == today.Day || article.ThreadTimestamp.Day == yesterday.Day)
                {
                    Article articleInDatabase = _context.Article.SingleOrDefault<Article>(a => a.ArticleUrl == article.ArticleUrl);

                    if(articleInDatabase == null)
                    {
                        _context.Article.Add(article);
                    }
                    else
                    {
                        articleInDatabase.ThreadComments = article.ThreadComments;
                    }
                }
            }

            _context.SaveChanges();
        }

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
                        article.ThreadUrl = String.Format("{0}{1}", "www.reddit.com", threadData["permalink"]);
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
