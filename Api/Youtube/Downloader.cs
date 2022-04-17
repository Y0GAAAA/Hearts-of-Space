using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode.Videos;

namespace Api.Youtube
{
    public class Downloader
    {

        private readonly VideoClient downloader = new VideoClient(new());

#nullable enable
        public async Task<string?> GetAudioStreamUrlAsync(VideoId videoId, CancellationToken token = default)
#nullable disable
        {
            var bestQualityStream = (await downloader.Streams.GetManifestAsync(videoId, token)).GetAudioOnlyStreams()
                                                                                               .Where(si => si.Container.Name == "webm")
                                                                                               .OrderBy(si => si.Bitrate)
                                                                                               .FirstOrDefault();

            return bestQualityStream?.Url;
        }

    }
}
