using Global.Table;
using Global.Util;
using System.Data;

namespace Api.HOS.Json
{
    public class Channel : IDataRow
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public ChannelProgram[] programs { get; set; }

        public object[] GetRow() => new object[]{ id, name, description };
    }
    public class ChannelProgram : IDataRow
    {
        public int id { get; set; }
        public string title { get; set; }
        public string shortDescription { get; set; }

        public object[] GetRow() => new object[]{ id, title, shortDescription};
    }
}
