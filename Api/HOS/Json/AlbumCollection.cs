using System;

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
        public String attribution { get; set; }
        public String title { get; set; }
        public int duration { get; set; }
    }
}
