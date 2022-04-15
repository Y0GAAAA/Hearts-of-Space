using Api.HOS;
using Api.HOS.Json;
using Client.Audio;
using Client.UI;
using Global.Table;
using Global.Util;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Gui;

using Attribute = Terminal.Gui.Attribute;

#pragma warning disable CS1998

namespace Client
{
    public class Tui
    {

        #region Enums

        private enum Category : int
        {
            Albums = 0,
            Channels = 1,
        }

        public enum Step
        {
            ConsultingAlbums,
            ConsultingAlbumTracks,

            ConsultingChannels,
            ConsultingChannelPrograms,
            ConsultingChannelProgramTracks,
        }

        #endregion

        private static readonly String[] CATEGORIES = new String[] { "Albums", "Channels" };

        /* - CACHED CATEGORIES - */
        private readonly AsyncLazy<DataTable> ALBUM_TABLE = new AsyncLazy<DataTable>(() => Task.Run(async () =>
        {
            DataTable table = TableBase.AlbumTableBase;
            Album[] albums = await CachedHOSApi.GetAlbumsAsync();

            foreach (Album album in albums)
            {
                table.Rows.Add(album.id, album.title, album.attribution, album.duration);
            }

            return table;
        }));
        private readonly AsyncLazy<DataTable> CHANNEL_TABLE = new AsyncLazy<DataTable>(() => Task.Run(async () =>
        {
            DataTable table = TableBase.ChannelTableBase;
            Channel[] channels = await CachedHOSApi.GetChannelsAsync();

            foreach (Channel channel in channels)
            {
                table.Rows.Add(channel.id, channel.name, channel.description);
            }

            return table;
        }));

        /* - AUDIO - */
        private readonly NaiveYoutubeAudioPlayer audioPlayer = new NaiveYoutubeAudioPlayer();

        /* - "PREVIOUS" BUTTON - */
        private readonly LimitedStack<StepData> stepHistory = new LimitedStack<StepData>(16);

        private Step CurrentStep => stepHistory.Peek()?.Step ?? throw new NotImplementedException("unreachable");

        private async Task<Track[]> GetTracksAsync(int id)
        {
            return CurrentStep switch
            {
                Step.ConsultingAlbumTracks => (await CachedHOSApi.GetAlbumTrackById(id)).ToArray(),
                Step.ConsultingChannelProgramTracks => (await CachedHOSApi.GetProgramTrackById(id)).ToArray(),
                Step.ConsultingAlbums => await CachedHOSApi.GetAlbumTracksAsync(id),
                Step.ConsultingChannelPrograms => (await CachedHOSApi.GetProgramTracksAsync(id)).SelectMany(x => x.tracks).ToArray(),
                _ => null,
            };
        }

        #region UI GLOBALS

        private readonly ListView categorySelectionView = new ListView(CATEGORIES) { Y = 8, Width = 8, Height = CATEGORIES.Length, TabIndex = 1 };
        private readonly TableView mainTableView = new TableView() { X = 22, Y = 2, Width = Dim.Percent(80), Height = Dim.Fill(1), TabIndex = 0 };
        private readonly ListView runningTasksView = new ListView(UITasks.tasks) { X = 0, Y = 20, Width = 20, Height = Dim.Percent(50), CanFocus = false, TabStop = false };
        private readonly Label timestampLabel = new Label() { Y = 1, Width = Dim.Fill(), TextAlignment = TextAlignment.Right, CanFocus = false, TabStop = false };
        private readonly Label phraseLabel = new Label("slow music for fast times") { X = 4, Y = 0, CanFocus = false };

        #endregion

        public void Run()
        {
            Application.Init();

            mainTableView.KeyDown += (ke) => MainTableView_KeyDown(ke, mainTableView.SelectedRow);
            mainTableView.SelectedCellChanged += MainTableView_SelectedCellChanged;

            categorySelectionView.SelectedItemChanged += CategorySelectionView_SelectedItemChanged;

            // main window
            Window window = new Window("hearts of space");
            MenuBarItem[] keyBinds = new MenuBarItem[]
            {
                new MenuBarItem("Keybinds", new MenuItem[]
                {
                    new MenuItem("Play", "P", null),
                    new MenuItem("More", "M", null),
                    new MenuItem("Add to queue", "Q", null),
                    new MenuItem("Next song", "N", null),
                    new MenuItem("Navigate back", "Esc", null),
                    new MenuItem("Play/Pause", "Space", null),

                }),
            };

            // controls
            window.Add(phraseLabel);
            window.Add(new MenuBar(keyBinds) { Width = 7, X = 0, Y = 2, CanFocus = false, });
            window.Add(categorySelectionView);
            window.Add(mainTableView);
            window.Add(timestampLabel);
            window.Add(runningTasksView);

            window.ColorScheme = new ColorScheme
            {
                Normal = Attribute.Make(Color.White, Color.Black),
                Focus = Attribute.Make(Color.Magenta, Color.Black),
                HotNormal = Attribute.Make(Color.White, Color.Black),
                HotFocus = Attribute.Make(Color.Magenta, Color.Black),
                Disabled = Attribute.Make(Color.White, Color.Black),
            };

            window.KeyDown += (ke) =>
            {
                if (ke.KeyEvent.Key == Key.Space)
                {
                    audioPlayer.PlayPause();
                }

                if (ke.KeyEvent.Key == Key.Esc)
                {
                    HandleBack();
                }
            };

            Application.MainLoop.AddTimeout(
                TimeSpan.FromMilliseconds(200),
                (_) =>
                {
                    if (!audioPlayer.HasMedia || audioPlayer.MediaEndReached) { timestampLabel.Text = string.Empty; }
                    else { timestampLabel.Text = audioPlayer.PlayingStatus + " | " + audioPlayer.TimeStatus; }
                    return true;
                }
            );

            Color[] colors = new Color[] { Color.White, Color.BrightBlue, Color.BrightCyan, Color.Gray };

            Application.MainLoop.AddTimeout(
                TimeSpan.FromSeconds(10),
                (_) =>
                {
                    Task.Run(async () =>
                    {
                        foreach (Color color in colors)
                        {
                            phraseLabel.ColorScheme = new ColorScheme()
                            {
                                Normal = Attribute.Make(color, Color.Black),
                            };
                            await Task.Delay(2000);
                        };
                    });
                    return true;
                }
            );

            Application.Top.Add(window);
            Application.Run();
        }

        #region EVENTS

        // artificially select the whole row instead of just one cell 
        private void MainTableView_SelectedCellChanged(TableView.SelectedCellChangedEventArgs obj)
        {
            int y = obj.NewRow;
            int columnCount = obj.Table.Columns.Count;
            TableView.TableSelection multiselectParameters = new TableView.TableSelection(new Point(0, y), new Rect(0, y, columnCount, 1));

            mainTableView.MultiSelectedRegions.Push(multiselectParameters);
        }

        private async void CategorySelectionView_SelectedItemChanged(ListViewItemEventArgs obj)
        {
            await (obj.Item switch
            {
                (int)Category.Albums => DisplayAlbums(),
                (int)Category.Channels => DisplayChannels(),
                _ => Task.CompletedTask,
            });
        }
        private async void MainTableView_KeyDown(View.KeyEventEventArgs obj, int selectedRow)
        {
            if (selectedRow == -1)
            {
                return;
            }

            DataRow row = mainTableView.Table.Rows[selectedRow];
            int keyCode = (int)obj.KeyEvent.Key;

            await (keyCode switch
            {
                109 or 77 => HandleMore(row),
                110 or 78 => HandleNext(),
                112 or 80 => HandlePlay(row),
                113 or 81 => HandleQueue(row),
                _ => Task.CompletedTask,
            });
        }

        #endregion

        public void SetCurrentStepData(Step step, int id = 0) => stepHistory.Add(new StepData(step, id));

        #region OPS HANDLERS
        private async Task HandleMore(DataRow row)
        {
            Int32 id = row.Field<Int32>(0);
            await (CurrentStep switch
            {
                Step.ConsultingAlbums => DisplayAlbumTracks(id),
                Step.ConsultingChannels => DisplayChannelPrograms(id),
                Step.ConsultingChannelPrograms => DisplayChannelProgramTracks(id),
                _ => Task.CompletedTask,
            });
        }
        private async Task HandlePlay(DataRow row)
        {
            // should be safe, all tables start with an id column
            Int32 id = row.Field<Int32>("id");
            Track[] tracks = await GetTracksAsync(id);

            if (tracks is null)
            {
                return;
            }

            Track toPlay = tracks[0];

            await audioPlayer.PlayTrackWithVisualFeedbackAsync(toPlay);

            if (tracks.Length > 1)
            {
                audioPlayer.AddToQueue(tracks[1..]);
            }
        }
        private async Task HandleQueue(DataRow row)
        {
            Int32 id = row.Field<Int32>("id");
            Track[] tracks = await GetTracksAsync(id);

            if (tracks is null)
            {
                return;
            }

            audioPlayer.AddToQueue(tracks);
        }
        private async void HandleBack()
        {
            if (stepHistory.Count < 2)
            {
                return;
            }

            (StepData _, StepData last) = (stepHistory.Pop(), stepHistory.Pop());

            (Step lastStep, Int32 id) = last;
            await (lastStep switch
            {
                Step.ConsultingAlbums => DisplayAlbums(),
                Step.ConsultingAlbumTracks => DisplayAlbumTracks(id),
                Step.ConsultingChannels => DisplayChannels(),
                Step.ConsultingChannelPrograms => DisplayChannelPrograms(id),
                Step.ConsultingChannelProgramTracks => DisplayChannelProgramTracks(id),
                _ => Task.CompletedTask,
            });
        }
        private async Task HandleNext()
        {
            bool hasNextTrack = await audioPlayer.PlayNextTrackInQueue();
            if (!hasNextTrack)
            {
                ErrorBox.Show("No tracks in queue");
            }
        }
        #endregion

        #region DISPLAY
        private async Task DisplayAlbums()
        {
            SetCurrentStepData(Step.ConsultingAlbums);
            DataTable albums = await ALBUM_TABLE.GetAsync()
                                          .WithUITask("getting albums");
            mainTableView.Table = albums;
        }
        private async Task DisplayAlbumTracks(int albumId)
        {
            SetCurrentStepData(Step.ConsultingAlbumTracks, albumId);
            Track[] albumTracks = await CachedHOSApi.GetAlbumTracksAsync(albumId)
                                          .WithUITask("getting tracks");
            mainTableView.Table = albumTracks.ToDataTable();
        }
        private async Task DisplayChannels()
        {
            SetCurrentStepData(Step.ConsultingChannels);
            DataTable channels = await CHANNEL_TABLE.GetAsync()
                                              .WithUITask("getting channels");
            mainTableView.Table = channels;
        }
        private async Task DisplayChannelPrograms(int channelId)
        {
            SetCurrentStepData(Step.ConsultingChannelPrograms, channelId);
            mainTableView.Table = (await CachedHOSApi.GetChannelProgramsAsync(channelId)).ToDataTable();
        }
        private async Task DisplayChannelProgramTracks(int programId)
        {
            SetCurrentStepData(Step.ConsultingChannelProgramTracks, programId);
            ProgramAlbum[] programTracks = await CachedHOSApi.GetProgramTracksAsync(programId)
                                            .WithUITask("getting tracks");
            mainTableView.Table = programTracks.ToDataTable();
        }
        #endregion
    }
}
