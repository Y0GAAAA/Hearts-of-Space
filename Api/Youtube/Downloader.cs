using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode.Videos;

namespace Api.Youtube
{
    public class Downloader
    {

        private readonly VideoClient downloader = new VideoClient(new());

        public async Task<System.String> GetAudioStreamUrlAsync(VideoId videoId)
        {
            YoutubeExplode.Videos.Streams.AudioOnlyStreamInfo bestQualityStream = (await downloader.Streams.GetManifestAsync(videoId)).GetAudioOnlyStreams()
                                                                                        .Where(si => si.Container.Name == "webm")
                                                                                        .OrderBy(si => si.Bitrate)
                                                                                        .First();
            return bestQualityStream.Url;
        }

    }
}
