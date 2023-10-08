using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentedNews_Functions.Entities
{
    public class Article
    {
        public int Id { get; set; }
        public string ArticleUrl { get; set; }
        public string ArticleTitle { get; set; }
        public string ArticleThumbnail { get; set; }
        public string ThreadUrl { get; set; }
        public int ThreadComments { get; set; }
        public DateTime ThreadTimestamp { get; set; }
    }
}
