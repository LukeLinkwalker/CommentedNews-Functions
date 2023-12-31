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
    public class FetchFunc
    {
        private readonly ArticleContext _articleContext;
        private readonly MediaContext _mediaContext;
        private List<string> medier {  get; set; }

        public FetchFunc(ArticleContext articleContext, MediaContext mediaContext)
        {
            _articleContext = articleContext;
            _mediaContext = mediaContext;

            var media = _mediaContext.Media.ToList<Media>();
            medier = media.Select(m => m.Name).ToList();
        }

        [FunctionName("Fetch")]
        public async Task Run([TimerTrigger("0 0 0/2 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Fetching at: {DateTime.Now}");
            try
            {
                string json = await GetJSON(log);
                
                log.LogInformation($"Processing JSON input of length: {json.Length}");

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
            } catch (Exception ex)
            {
                log.LogInformation($"Error occured at: {DateTime.Now}");
                log.LogInformation(ex.ToString());
            }
        }

        /// <summary>
        /// Fetches new posts made on the /r/Denmark subreddit through the Reddit API.
        /// </summary>
        private async Task<string> GetJSON(ILogger log)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://www.reddit.com/r/denmark/new");
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")
                );
                client.DefaultRequestHeaders.Add("User-Agent", "News_Threads_Ranking");

                HttpResponseMessage response = client.GetAsync(".json?limit=100").Result;
                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    log.LogInformation($"Http request to reddit API failed. Response code: {response.StatusCode}");
                }
            } 
            catch (Exception ex)
            {
                log.LogInformation($"Error occured while getting data from Reddit at: {DateTime.Now}");
                log.LogInformation(ex.ToString());
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

            Thread[] threads = JsonConvert.DeserializeObject<Thread[]>(((JArray)dataObj["children"]).ToString());

            foreach(Thread thread in threads)
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
                        article.ThreadUrl = $"https://www.reddit.com{thread.data.permalink}";
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
