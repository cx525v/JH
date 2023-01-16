namespace SharedLibrary.Models
{
    public record TwitterRecord
    {
        public HashTagData Data { get; set; } = new HashTagData();
    }
    public record HashTagData
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;

        public Entity Entities { get; set; } = new Entity();
    }


    public record Entity
    {
        public List<HashTag> Hashtags { get; set; } = new List<HashTag>();
    } 

    public record HashTag
    {
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;

    }
}
