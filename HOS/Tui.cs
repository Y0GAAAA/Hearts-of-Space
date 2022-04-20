﻿using Api.HOS;
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

        private static readonly string[] CATEGORIES = new string[] { "Albums", "Channels" };

        /* - CACHED CATEGORIES - */
        private readonly AsyncLazy<DataTable> ALBUM_TABLE = new AsyncLazy<DataTable>(() => Task.Run(async () =>
        {
            var table = TableBase.AlbumTableBase;
            var albums = await CachedHOSApi.GetAlbumsAsync();

            foreach (var album in albums)
            {
                table.Rows.Add(album.id, album.title, album.attribution, album.duration);
            }

            return table;
        }));
        private readonly AsyncLazy<DataTable> CHANNEL_TABLE = new AsyncLazy<DataTable>(() => Task.Run(async () =>
        {
            var table = TableBase.ChannelTableBase;
            var channels = await CachedHOSApi.GetChannelsAsync();

            foreach (var channel in channels)
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
        private const int LEFT_PANE_WIDTH = 32;

        private readonly ListView categorySelectionView = new ListView(CATEGORIES) { Y = 8, Width = 8, Height = CATEGORIES.Length, TabIndex = 1 };
        private readonly TableView mainTableView = new TableView() { X = LEFT_PANE_WIDTH + 2, Y = 2, Width = Dim.Percent(80), Height = Dim.Fill(1), TabIndex = 0 };
        private readonly ListView runningTasksView = new ListView(UITasks.tasks) { X = 0, Y = 20, Width = LEFT_PANE_WIDTH, Height = Dim.Sized(UITasks.MAX_TASK_COUNT), CanFocus = false, TabStop = false };
        private readonly Label timestampLabel = new Label() { Y = 1, Width = Dim.Fill(), TextAlignment = TextAlignment.Right, CanFocus = false, TabStop = false };
        private readonly Label phraseLabel = new Label("slow music for fast times") { X = 4, Y = 0, CanFocus = false };
        private readonly ListView queueView;

        // Initialize UI globals that need to refer to other components in the Tui constructor
        public Tui()
        {
            queueView = new ListView() { X = 0, Y = Pos.Bottom(runningTasksView) + 1, Width = LEFT_PANE_WIDTH, Height = Dim.Fill(), CanFocus = false, TabStop = false };
        }

        #endregion

        public void Run()
        {
            Application.Init();

            audioPlayer.QueueChanged += (_, q) =>
            {
                var stringizedQueue = q.Select(t => t.ToString())
                                       .ToList();
                queueView.SetSource(stringizedQueue);
            };

            mainTableView.KeyDown += (ke) => MainTableView_KeyDown(ke, mainTableView.SelectedRow);
            mainTableView.SelectedCellChanged += MainTableView_SelectedCellChanged;

            categorySelectionView.SelectedItemChanged += CategorySelectionView_SelectedItemChanged;

            // main window
            var window = new Window("hearts of space");
            var keyBinds = new MenuBarItem[]
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
            window.Add(queueView);
            window.Add(new Label("- Queue -") { X = LEFT_PANE_WIDTH / 2 - 5, Y = Pos.Bottom(runningTasksView), CanFocus = false, TabStop = false, ColorScheme = new ColorScheme() { Normal = Attribute.Make(Color.Magenta, Color.Black) } });

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

            var colors = new Color[] { Color.White, Color.BrightBlue, Color.BrightCyan, Color.Gray };

            Application.MainLoop.AddTimeout(
                TimeSpan.FromSeconds(10),
                (_) =>
                {
                    Task.Run(async () =>
                    {
                        foreach (var color in colors)
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
            var multiselectParameters = new TableView.TableSelection(new Point(0, y), new Rect(0, y, columnCount, 1));

            mainTableView.MultiSelectedRegions.Push(multiselectParameters);
        }

        private async void CategorySelectionView_SelectedItemChanged(ListViewItemEventArgs obj)
        {
            await (obj.Item switch
            {
                (int) Category.Albums => DisplayAlbums(),
                (int) Category.Channels => DisplayChannels(),
                _ => Task.CompletedTask,
            });
        }
        private async void MainTableView_KeyDown(View.KeyEventEventArgs obj, int selectedRow)
        {
            if (selectedRow == -1)
            {
                return;
            }

            var row = mainTableView.Table.Rows[selectedRow];
            int keyCode = (int) obj.KeyEvent.Key;

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
            int id = row.Field<int>(0);
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
            int id = row.Field<int>("id");
            var tracks = await GetTracksAsync(id);

            if (tracks is null)
            {
                return;
            }

            var toPlay = tracks[0];

            await audioPlayer.PlayTrackWithVisualFeedbackAsync(toPlay);

            if (tracks.Length > 1)
            {
                audioPlayer.AddToQueue(tracks[1..]);
            }
        }
        private async Task HandleQueue(DataRow row)
        {
            int id = row.Field<int>("id");
            var tracks = await GetTracksAsync(id);

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

            (var _, var last) = (stepHistory.Pop(), stepHistory.Pop());

            (var lastStep, int id) = last;
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
            var albums = await ALBUM_TABLE.GetAsync()
                                          .WithUITask("getting albums");
            mainTableView.Table = albums;
        }
        private async Task DisplayAlbumTracks(int albumId)
        {
            SetCurrentStepData(Step.ConsultingAlbumTracks, albumId);
            var albumTracks = await CachedHOSApi.GetAlbumTracksAsync(albumId)
                                          .WithUITask("getting tracks");
            mainTableView.Table = albumTracks.ToDataTable();
        }
        private async Task DisplayChannels()
        {
            SetCurrentStepData(Step.ConsultingChannels);
            var channels = await CHANNEL_TABLE.GetAsync()
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
            var programTracks = await CachedHOSApi.GetProgramTracksAsync(programId)
                                            .WithUITask("getting tracks");
            mainTableView.Table = programTracks.ToDataTable();
        }
        #endregion
    }
}