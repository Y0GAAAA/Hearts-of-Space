using Global.Table;
using Global.Util;
using System;
using System.Collections;
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

        public object[] GetRow() => new object[] { id, title, attribution, duration };
    }
}
