namespace SharedLibrary.Models
{
    public class TweetResponse
    {
        public  int TweentTotalCount { get; set; }

        public  IDictionary<string, int> HashTags { get; set; } = new Dictionary<string, int>();
    }
}
