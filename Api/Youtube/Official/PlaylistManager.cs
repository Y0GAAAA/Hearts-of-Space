using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlaylistId = System.String;
using VideoId = System.String;

namespace Api.Youtube.Official
{
    public class PlaylistManager
    {
        private readonly List<Playlist> playlists;
        private readonly YouTubeService youtubeClient;

        public PlaylistManager(YouTubeService youTubeService)
        {
            PlaylistsResource.ListRequest playlistsReq = youTubeService.Playlists.List("snippet,id,contentDetails");
            playlistsReq.Mine = true;
            playlists = (List<Playlist>)playlistsReq.Execute().Items;
            youtubeClient = youTubeService;
        }

        public System.Boolean PlaylistExists(System.String title) => playlists.Any(p => p.Snippet.Title == title);
        public async Task<Playlist> CreatePlaylistAsync(System.String title)
        {
            Playlist playlist = new Playlist()
            {
                Snippet = new() { Title = title },
            };
            return await youtubeClient.Playlists.Insert(playlist, "snippet").ExecuteAsync()
                                                                            .ContinueWith(p =>
                                                                            {
                                                                                if (p.IsFaulted)
                                                                                    throw p.Exception.InnerException;

                                                                                playlists.Add(p.Result);
                                                                                return p.Result;
                                                                            });
        }
        public async Task AddVideoToPlaylistAsync(PlaylistId playlistId, VideoId videoId)
        {
            PlaylistItem playlistItem = new PlaylistItem()
            {
                Snippet = new()
                {
                    PlaylistId = playlistId,
                    ResourceId = new() { VideoId = videoId, Kind = "youtube#video", ETag = "youtube#video" }
                },
            };
            await youtubeClient.PlaylistItems.Insert(playlistItem, "id,snippet").ExecuteAsync();
        }
    }
}
