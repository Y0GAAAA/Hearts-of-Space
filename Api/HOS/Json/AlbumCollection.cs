using Global.Table;
using Global.Util;
using System.Data;

namespace Api.HOS.Json
{

    public class AlbumCollection
    {
        public Album[] content { get; set; }
        public int totalElements { get; set; }
    }

    public class Album : IDataRow
    {
        public int id { get; set; }
        public string attribution { get; set; }
        public string title { get; set; }
        public int duration { get; set; }

        public DataRow GetRow() => TableBase.AlbumTableBase.NewRow(id, title, attribution, duration);
    }
}
