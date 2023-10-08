using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentedNews_Functions.Entities
{
    public class Listing
    {
        public string kind { get; set; }
        public int dist { get; set; }
        public Thread[] children { get; set; }
    }

    public class Thread
    {
        public string kind { get; set; }
        public ThreadData data { get; set; }
    }

    public class ThreadData
    {
        public string domain { get; set; } = string.Empty;
        public string title { get; set; } = string.Empty;
        public string url { get; set; } = string.Empty;
        public string url_overridden_by_dest { get; set; } = string.Empty;
        public string thumbnail { get; set; } = string.Empty;
        public int num_comments { get; set; }
        public string permalink { get; set; } = string.Empty;
        public long created_utc { get; set; }
    }
}
