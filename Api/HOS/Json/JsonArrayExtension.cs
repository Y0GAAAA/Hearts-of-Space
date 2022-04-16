using Global.Table;
using System.Data;
using System.Linq;

namespace Api.HOS.Json
{
    public static class JsonArrayExtension
    {
        public static DataTable ToDataTable(this ProgramAlbum[] albums)
        {
            var table = TableBase.ChannelProgramTrackTableBase;

            foreach (var album in albums)
            {
                foreach (var track in album.tracks)
                    table.Rows.Add(track.id, track.title, album.title, track.artists.First().name, track.duration);
            }

            return table;
        }
        public static DataTable ToDataTable(this Track[] tracks)
        {
            var table = TableBase.AlbumTrackTableBase;
            foreach (var track in tracks)
            {
                string artists = string.Join(", ", track.artists.Select(artist => artist.name));
                table.Rows.Add(track.id, track.title, artists, track.duration);
            }
            return table;
        }
        public static DataTable ToDataTable(this ChannelProgram[] programs)
        {
            var table = TableBase.ChannelProgramTableBase;

            foreach (var program in programs)
                table.Rows.Add(program.id, program.title, program.shortDescription);

            return table;
        }
    }
}
