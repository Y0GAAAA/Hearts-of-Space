using Api.HOS.Json;
using Global.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Channel = Api.HOS.Json.Channel;

#pragma warning disable CS1998

namespace Api.HOS
{
    public class CachedHOSApi
    {

        private static readonly Dictionary<int, ChannelProgram[]> channelPrograms = new Dictionary<int, ChannelProgram[]>();

        private static readonly Dictionary<int, AsyncLazy<ProgramAlbum[]>> programTracks = new Dictionary<int, AsyncLazy<ProgramAlbum[]>>();
        private static readonly Dictionary<int, AsyncLazy<Track[]>> albumTracks = new Dictionary<int, AsyncLazy<Track[]>>();

        // ugly code, hard to clean without macros
        private static readonly AsyncLazy<Album[]> albumsLazy = new AsyncLazy<Album[]>(() => Task.Run(async () =>
        {
            return (await HOSApi.GetAlbumsAsync()).Select(album =>
            {
                albumTracks.TryAdd(album.id, new AsyncLazy<Track[]>(
                    () => HOSApi.GetAlbumTracksAsync(album.id)
                ));
                return album;
            }).ToArray();
        }));
        private static readonly AsyncLazy<Channel[]> channelsLazy = new AsyncLazy<Channel[]>(() => Task.Run(async () =>
        {
            return (await HOSApi.GetChannelsAsync()).Select(channel =>
            {
                channelPrograms.TryAdd(channel.id, channel.programs);
                foreach (var program in channel.programs)
                {
                    programTracks.TryAdd(program.id, new AsyncLazy<ProgramAlbum[]>(() => Task.Run(async () =>
                    {
                        return await HOSApi.GetProgramTracksAsync(program.id);
                    })));
                }

                return channel;
            }).ToArray();
        }));

        public static async Task<Track> GetAlbumTrackById(int id)
        {
            return albumTracks.Values.Where(lazy => lazy.ValueCreated)
                                     .SelectMany(lazy => lazy.GetAsync().BlockOn())
                                     .FirstOrDefault(track => track.id == id);
        }
        public static async Task<Track> GetProgramTrackById(int id)
        {
            return programTracks.Values.Where(lazy => lazy.ValueCreated)
                                     .SelectMany(lazy => lazy.GetAsync().BlockOn())
                                     .SelectMany(programAlbum => programAlbum.tracks)
                                     .FirstOrDefault(track => track.id == id);
        }

        public static async Task<Album[]> GetAlbumsAsync() => await albumsLazy.GetAsync();
        public static async Task<Channel[]> GetChannelsAsync() => await channelsLazy.GetAsync();
        public static async Task<Track[]> GetAlbumTracksAsync(int albumId)
        {
            try
            {
                return await albumTracks[albumId].GetAsync();
            }
            catch
            {
                if (!albumsLazy.ValueCreated)
                {
                    await albumsLazy.GetAsync();
                    return await GetAlbumTracksAsync(albumId);
                }
                throw;
            }
        }
        public static async Task<ProgramAlbum[]> GetProgramTracksAsync(int programId)
        {
            try
            {
                return await programTracks[programId].GetAsync();
            }
            catch
            {
                if (!channelsLazy.ValueCreated)
                {
                    await channelsLazy.GetAsync();
                    return await GetProgramTracksAsync(programId);
                }
                throw;
            }
        }

        // async function even though it's not needed; there to have an uniform api
        public static async Task<ChannelProgram[]> GetChannelProgramsAsync(int channelId) => channelPrograms[channelId];
    }
}
