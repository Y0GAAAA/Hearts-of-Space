using Api.HOS.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Api.HOS
{
    public static class HOSApi
    {
        private static class WebJson
        {
            private static readonly HttpClient client = new HttpClient();
            private static readonly JsonSerializer json = new JsonSerializer()
            {
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
            };

            // accessing the deserializer concurrently is most certainly UB
            private static readonly Object DESERIALIZE_LOCK = new Object();

            private static T DeserializeStream<T>(Stream stream)
            {
                lock (DESERIALIZE_LOCK)
                {
                    using (StreamReader reader = new StreamReader(stream, leaveOpen: false))
                    using (JsonTextReader jsonReader = new JsonTextReader(reader))
                    {
                        return json.Deserialize<T>(jsonReader);
                    }
                }
            }
            public static async Task<T> DeserializeFromUrlAsync<T>(String url)
            {
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get,
                    url
                );
                HttpResponseMessage req = await client.SendAsync(request);
                Stream jsonStream = await req.Content.ReadAsStreamAsync();
                return DeserializeStream<T>(jsonStream);
            }
        }

        public static async Task<Album[]> GetAlbumsAsync()
        {
            int totalAlbums = -1;
            int processed = 0;

            List<Album> albumList = new List<Album>();

            for (int page = 0; totalAlbums == -1 || processed < totalAlbums; page++)
            {
                AlbumCollection albums = await WebJson.DeserializeFromUrlAsync<AlbumCollection>($"http://api.hos.com/api/v1/albums?page={page}&size=50");

                if (page == 0)
                    totalAlbums = albums.totalElements;

                albumList.AddRange(albums.content);
                processed += albums.content.Length;
            }

            return albumList.ToArray();
        }
        public static async Task<Track[]> GetAlbumTracksAsync(int albumId) => (await WebJson.DeserializeFromUrlAsync<AlbumTracks>($"http://api.hos.com/api/v1/albums/{albumId}")).tracks;
        public static async Task<Channel[]> GetChannelsAsync() => await WebJson.DeserializeFromUrlAsync<Channel[]>("http://api.hos.com/api/v1/channels/complete?hemisphere=northern");
        public static async Task<ProgramAlbum[]> GetProgramTracksAsync(int programId) => (await WebJson.DeserializeFromUrlAsync<ProgramTracks>($"http://api.hos.com/api/v1/programs/{programId}")).albums;
    }
}
