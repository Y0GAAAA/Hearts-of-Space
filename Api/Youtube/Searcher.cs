using Api.HOS.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Common;
using YoutubeExplode.Search;

namespace Api.Youtube
{
    public class Searcher
    {
        private readonly SearchClient client = new SearchClient(new());

        public async Task<VideoSearchResult> SearchSongAsync(Track track, CancellationToken token = default)
        {
            string query = track.title + ' ' + track.artists.First().name;
            return await SearchSongAsync(query, track.duration, token);
        }
        public async Task<VideoSearchResult> SearchSongAsync(string query, int duration, CancellationToken token = default)
        {
            var videos = (await client.GetVideosAsync(query, token)
                                     .CollectAsync(6))
                                     .OrderBy(v =>
                                     {
                                         var d = v.Duration ?? TimeSpan.MaxValue;
                                         return Math.Abs((int) d.TotalSeconds - duration);
                                     });

            if (!videos.Any())
                return null;

            try { return videos.First(v => v.Author.Title.EndsWith(" - Topic")); }
            catch
            {
                Debug.WriteLine("no auto-generated video found, falling back to first search result");
                return videos.First();
            }
        }
    }
}
