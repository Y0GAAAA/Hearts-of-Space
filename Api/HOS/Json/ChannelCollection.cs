namespace Api.HOS.Json
{
    public class Channel
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public ChannelProgram[] programs { get; set; }
    }
    public class ChannelProgram
    {
        public int id { get; set; }
        public string title { get; set; }
        public string shortDescription { get; set; }
    }
}
