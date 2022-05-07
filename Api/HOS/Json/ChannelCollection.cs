namespace Api.HOS.Json
{
    public class Channel
#if _HOSLIB
    : IDataRow
#endif
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public ChannelProgram[] programs { get; set; }
#if _HOSLIB
        public object[] GetRow() => new object[] { id, name, description };
#endif
    }
    public class ChannelProgram
#if _HOSLIB
    : IDataRow
#endif
    {
        public int id { get; set; }
        public string title { get; set; }
        public string shortDescription { get; set; }
#if _HOSLIB
        public object[] GetRow() => new object[] { id, title, shortDescription };
#endif
    }
}
