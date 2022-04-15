using System;
using System.Data;

namespace Global.Table
{
    public static class TableBase
    {
        private static readonly DataTable _albumTableBase = GetAlbumTableBase();
        private static readonly DataTable _albumTrackTableBase = GetAlbumTrackTableBase();
        private static readonly DataTable _channelTableBase = GetChannelTableBase();
        private static readonly DataTable _channelProgramTableBase = GetChannelProgramTableBase();
        private static readonly DataTable _channelProgramTrackTableBase = GetChannelProgramTrackTableBase();

        public static DataTable AlbumTableBase => _albumTableBase.Clone();
        public static DataTable AlbumTrackTableBase => _albumTrackTableBase.Clone();
        public static DataTable ChannelTableBase => _channelTableBase.Clone();
        public static DataTable ChannelProgramTableBase => _channelProgramTableBase.Clone();
        public static DataTable ChannelProgramTrackTableBase => _channelProgramTrackTableBase.Clone();

        private static DataTable GetAlbumTableBase()
        {
            return new HandyDataTable().AddColumn<Int32>("id")
                                       .AddColumn<String>("title")
                                       .AddColumn<String>("attribution")
                                       .AddColumn<Int32>("duration");
        }
        private static DataTable GetAlbumTrackTableBase()
        {
            return new HandyDataTable().AddColumn<Int32>("id")
                                       .AddColumn<String>("title")
                                       .AddColumn<String>("artists")
                                       .AddColumn<Int32>("duration");
        }
        private static DataTable GetChannelTableBase()
        {
            return new HandyDataTable().AddColumn<Int32>("id")
                                       .AddColumn<String>("name")
                                       .AddColumn<String>("description");
        }
        private static DataTable GetChannelProgramTableBase()
        {
            return new HandyDataTable().AddColumn<Int32>("id")
                                       .AddColumn<String>("title")
                                       .AddColumn<String>("description");
        }
        private static DataTable GetChannelProgramTrackTableBase()
        {
            return new HandyDataTable().AddColumn<Int32>("id")
                                       .AddColumn<String>("title")
                                       .AddColumn<String>("album")
                                       .AddColumn<String>("artists")
                                       .AddColumn<Int32>("duration");
        }

    }
}
