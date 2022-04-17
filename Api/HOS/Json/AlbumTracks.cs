using Global.Util;

namespace Api.HOS.Json
{
    public class AlbumTracks
    {
        public Track[] tracks { get; set; }
    }
    public class Track
    {
        public int id { get; set; }
        public string title { get; set; }
        public Artist[] artists { get; set; }
        public int duration { get; set; }

        public static implicit operator Track[](Track track)
        {
            return track.ToArray();
        }
        public override string ToString() => $"{title} - {artists[0].name}";
    }
    public class Artist
    {
        public string name { get; set; }
    }
}
