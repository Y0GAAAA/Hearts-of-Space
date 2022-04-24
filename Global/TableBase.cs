using System.Data;

namespace Global.Table
{
    public static class TableBase
    {
        private static readonly DataTable _albumTableBase = GetAlbumTableBase();
        private static readonly DataTable _trackTableBase = GetTrackTableBase();
        private static readonly DataTable _channelTableBase = GetChannelTableBase();
        private static readonly DataTable _channelProgramTableBase = GetChannelProgramTableBase();
        private static readonly DataTable _favoriteTableBase = GetFavoriteTableBase();

        public static DataTable AlbumTableBase => _albumTableBase.Clone();
        public static DataTable TrackTableBase => _trackTableBase.Clone();
        public static DataTable ChannelTableBase => _channelTableBase.Clone();
        public static DataTable ChannelProgramTableBase => _channelProgramTableBase.Clone();
        public static DataTable FavoriteTableBase => _favoriteTableBase.Clone();

        private static DataTable GetAlbumTableBase()
        {
            return new HandyDataTable().AddColumn<int>("id")
                                       .AddColumn<string>("title")
                                       .AddColumn<string>("attribution")
                                       .AddColumn<int>("duration");
        }
        private static DataTable GetTrackTableBase()
        {
            return new HandyDataTable().AddColumn<int>("id")
                                       .AddColumn<string>("title")
                                       .AddColumn<string>("artists")
                                       .AddColumn<int>("duration");
        }
        private static DataTable GetChannelTableBase()
        {
            return new HandyDataTable().AddColumn<int>("id")
                                       .AddColumn<string>("name")
                                       .AddColumn<string>("description");
        }
        private static DataTable GetChannelProgramTableBase()
        {
            return new HandyDataTable().AddColumn<int>("id")
                                       .AddColumn<string>("title")
                                       .AddColumn<string>("description");
        }
        private static DataTable GetFavoriteTableBase()
        {
            return new HandyDataTable().AddColumn<string>("Type");
        }
    }
}
