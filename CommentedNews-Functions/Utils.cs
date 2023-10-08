using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentedNews_Functions
{
    public static class Utils
    {
        /// <summary>
        /// Returns a DateTime instance based on a UNIX timestamp whilst also setting appropriate UTC timezone.
        /// </summary>
        public static DateTime GetTime(long timestamp, double utc)
        {
            DateTime time = new DateTime(1970, 1, 1, 0, 0, 0);
            time = time.AddSeconds(timestamp);
            time = time.AddHours(utc);

            return time;
        }

        /// <summary>
        /// Returns a DateTime instance that tells the current time and date in Denmark.
        /// </summary>
        public static DateTime GetDay()
        {
            return TimeZoneInfo.ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
        }
    }
}
