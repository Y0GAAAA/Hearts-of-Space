using System;

namespace Api.HOS.Json
{
    public class Channel
    {
        public int id { get; set; }
        public String name { get; set; }
        public String description { get; set; }
        public ChannelProgram[] programs { get; set; }
    }
    public class ChannelProgram
    {
        public int id { get; set; }
        public String title { get; set; }
        public String shortDescription { get; set; }
    }
}
