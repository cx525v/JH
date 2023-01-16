using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models
{
    public static class TweetResponse
    {
        public static int TweentTotalCount { get; set; }

        public static IDictionary<string, int> HashTags { get; set; } = new Dictionary<string, int>();
    }
}
