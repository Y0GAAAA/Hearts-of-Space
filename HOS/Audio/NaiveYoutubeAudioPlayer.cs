using Api.HOS.Json;
using Api.Youtube;
using Client.UI;
using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Client.Audio
{
    public class NaiveYoutubeAudioPlayer
    {
        private readonly LibVLC libVlc = new LibVLC(enableDebugLogs: false);
        private readonly Searcher youtubeSearcher = new Searcher();
        private readonly Downloader youtubeDownloader = new Downloader();

        private readonly Queue<Track> trackQueue = new Queue<Track>();
        private readonly MediaPlayer player;

        public event EventHandler<Track[]> QueueChanged = (_, _) => {};

        static NaiveYoutubeAudioPlayer()
        {
            Core.Initialize();
        }
        public NaiveYoutubeAudioPlayer()
        {
            player = new MediaPlayer(libVlc);
            player.EndReached += (s, e) =>
            {
                MediaEndReached = true;
                Task.Run(async () =>
                {
                    await PlayNextTrackInQueue();
                });
            };
        }
       
        public string TimeStatus
        {
            get
            {
                string toMMSS(long ms)
                {
                    return TimeSpan.FromMilliseconds(ms)
                                   .ToString("mm\\:ss");
                }
                (long length, long time) = (player.Length, player.Time);

                if (length == -1 || time == -1)
                {
                    return string.Empty;
                }

                return string.Concat(
                    toMMSS(time),
                    '/',
                    toMMSS(length)
                );
            }
        }
        public string PlayingStatus => player.IsPlaying ? "playing" : "paused";
        public bool HasMedia => player.Media is not null;
        public bool MediaEndReached { get; private set; } = true;

        public enum PlayTrackResult
        {
            Success,
            NoTrackFound,
            NetworkParsingFailed,
            FailedToPlay,
        }

        public void AddToQueue(Track[] track)
        {
            foreach (var t in track)
            {
                trackQueue.Enqueue(t);
                QueueChanged.Invoke(null, trackQueue.ToArray());
                Debug.WriteLine($"added track (id {t.id}) to queue; length of queue is {trackQueue.Count}");
            }
        }

        public async Task<PlayTrackResult> PlayTrackAsync(Track track)
        {
            var searchResult = await youtubeSearcher.SearchSongAsync(track)
                                                    .WithUITask("searching");

            if (searchResult is null)
            {
                return PlayTrackResult.NoTrackFound;
            }

            string streamUrl = await youtubeDownloader.GetAudioStreamUrlAsync(searchResult.Id)
                                                   .WithUITask("downloading");

            using var media = new Media(libVlc, new Uri(streamUrl));
            var parseResult = await media.Parse(MediaParseOptions.ParseNetwork)
                                         .WithUITask("parsing");

            if (parseResult is not MediaParsedStatus.Done)
            {
                return PlayTrackResult.NetworkParsingFailed;
            }

            bool playing = player.Play(media);

            MediaEndReached = !playing;

            return playing ? PlayTrackResult.Success : PlayTrackResult.FailedToPlay;
        }

        public async Task PlayTrackWithVisualFeedbackAsync(Track track)
        {
            var result = await PlayTrackAsync(track);

            string errorString = result switch
            {
                PlayTrackResult.NetworkParsingFailed => "Network parsing failed",
                PlayTrackResult.NoTrackFound => "No track found",
                PlayTrackResult.FailedToPlay => "Failed to play",
                _ => null,
            };

            if (errorString is not null)
            {
                ErrorBox.Show(errorString);
            }
        }

        public async Task<bool> PlayNextTrackInQueue()
        {
            if (trackQueue.Count > 0)
            {
                var nextTrack = trackQueue.Dequeue();
                QueueChanged.Invoke(null, trackQueue.ToArray());
                await PlayTrackWithVisualFeedbackAsync(nextTrack);
                return true;
            }
            return false;
        }

        public void PlayPause() => player.SetPause(player.IsPlaying);
    }
}
