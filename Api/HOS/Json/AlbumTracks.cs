using Global.Table;
using Global.Util;
using System.Data;
using System.Linq;

namespace Api.HOS.Json
{
    public class AlbumTracks
    {
        public Track[] tracks { get; set; }
    }
    public class Track : IDataRow
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

        public DataRow GetRow() => TableBase.ChannelProgramTrackTableBase.NewRow(id, title, artists.First().name, duration);
    }
    public class Artist
    {
        public string name { get; set; }
    }
}
