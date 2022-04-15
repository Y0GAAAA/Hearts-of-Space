namespace Api.HOS.Json
{
    public class ProgramTracks
    {
        public ProgramAlbum[] albums { get; set; }
    }
    public class ProgramAlbum
    {
        public System.String title { get; set; }
        public Track[] tracks { get; set; }
    }
}
