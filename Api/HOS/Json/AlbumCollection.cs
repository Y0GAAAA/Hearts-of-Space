namespace Api.HOS.Json
{

    public class AlbumCollection
    {
        public Album[] content { get; set; }
        public int totalElements { get; set; }
    }

    public class Album
#if _HOSLIB
    : IDataRow
#endif
    {
        public int id { get; set; }
        public string attribution { get; set; }
        public string title { get; set; }
        public int duration { get; set; }
#if _HOSLIB
        public object[] GetRow() => new object[] { id, title, attribution, duration };
#endif
    }
}
