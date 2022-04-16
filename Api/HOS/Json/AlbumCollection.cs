namespace Api.HOS.Json
{

    public class AlbumCollection
    {
        public Album[] content { get; set; }
        public int totalElements { get; set; }
    }

    public class Album
    {
        public int id { get; set; }
        public string attribution { get; set; }
        public string title { get; set; }
        public int duration { get; set; }
    }
}
