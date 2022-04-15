using Global.Table;
using System.Data;
using System.Linq;

namespace Api.HOS.Json
{
    public static class JsonArrayExtension
    {
        public static DataTable ToDataTable(this ProgramAlbum[] albums)
        {
            DataTable table = TableBase.ChannelProgramTrackTableBase;

            foreach (ProgramAlbum album in albums)
            {
                foreach (Track track in album.tracks)
                    table.Rows.Add(track.id, track.title, album.title, track.artists.First().name, track.duration);
            }

            return table;
        }
        public static DataTable ToDataTable(this Track[] tracks)
        {
            DataTable table = TableBase.AlbumTrackTableBase;
            foreach (Track track in tracks)
            {
                System.String artists = string.Join(", ", track.artists.Select(artist => artist.name));
                table.Rows.Add(track.id, track.title, artists, track.duration);
            }
            return table;
        }
        public static DataTable ToDataTable(this ChannelProgram[] programs)
        {
            DataTable table = TableBase.ChannelProgramTableBase;

            foreach (ChannelProgram program in programs)
                table.Rows.Add(program.id, program.title, program.shortDescription);

            return table;
        }
    }
}
