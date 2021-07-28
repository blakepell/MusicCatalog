/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Dapper;
using Microsoft.Data.Sqlite;
using MusicCatalog.Common.Models;
using NAudio.Wave;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MusicCatalog.Common
{
    /// <summary>
    /// The audio manager that is responsible for playing music, updating information about what's
    /// being played as well as be manage the playlist stack of what should be played.
    /// </summary>
    public class AudioManager : DependencyObject, IDisposable
    {
        public static readonly DependencyProperty NowPlayingSongTitleProperty = DependencyProperty.Register(
            nameof(NowPlayingSongTitle), typeof(string), typeof(AudioManager), new PropertyMetadata("No Song Info"));

        public string NowPlayingSongTitle
        {
            get => (string)GetValue(NowPlayingSongTitleProperty);
            set => SetValue(NowPlayingSongTitleProperty, value);
        }

        public static readonly DependencyProperty NowPlayingArtistProperty = DependencyProperty.Register(
            nameof(NowPlayingArtist), typeof(string), typeof(AudioManager), new PropertyMetadata("No Artist Info"));

        public string NowPlayingArtist
        {
            get => (string)GetValue(NowPlayingArtistProperty);
            set => SetValue(NowPlayingArtistProperty, value);
        }

        public static readonly DependencyProperty NowPlayingAlbumProperty = DependencyProperty.Register(
            nameof(NowPlayingAlbum), typeof(string), typeof(AudioManager), new PropertyMetadata("No Album Info"));

        public string NowPlayingAlbum
        {
            get => (string)GetValue(NowPlayingAlbumProperty);
            set => SetValue(NowPlayingAlbumProperty, value);
        }

        public static readonly DependencyProperty NowPlayingTotalTimeProperty = DependencyProperty.Register(
            nameof(NowPlayingTotalTime), typeof(string), typeof(AudioManager), new PropertyMetadata("0:00"));

        public string NowPlayingTotalTime
        {
            get => (string)GetValue(NowPlayingTotalTimeProperty);
            set => SetValue(NowPlayingTotalTimeProperty, value);
        }

        public static readonly DependencyProperty PropertyTypeProperty = DependencyProperty.Register(
            nameof(NowPlayingCurrentTime), typeof(string), typeof(AudioManager), new PropertyMetadata("0:00"));

        public string NowPlayingCurrentTime
        {
            get => (string)GetValue(PropertyTypeProperty);
            set => SetValue(PropertyTypeProperty, value);
        }

        public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register(
            nameof(IsPlaying), typeof(bool), typeof(AudioManager), new PropertyMetadata(default(bool)));

        public bool IsPlaying
        {
            get => (bool)GetValue(IsPlayingProperty);
            set => SetValue(IsPlayingProperty, value);
        }

        public WaveOutEvent WaveOut { get; private set; }

        public Mp3FileReader Mp3Reader { get; private set; }

        public Action<StoppedEventArgs> PlaybackStopped { get; set; }

        private DispatcherTimer _playTimer;

        public AudioManager()
        {
            _playTimer = new();
            _playTimer.Tick += new EventHandler(delegate (object o, EventArgs args)
            {
                if (this.Mp3Reader != null)
                {
                    this.UpdatePlayTime();
                }
            });

            _playTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        }

        public async Task Load(string filePath)
        {
            if (this.WaveOut == null)
            {
                this.WaveOut = new();
                this.WaveOut.PlaybackStopped += WaveOut_PlaybackStopped;
            }

            if (this.WaveOut.PlaybackState != PlaybackState.Stopped)
            {
                this.WaveOut.Stop();
            }

            if (this.Mp3Reader != null)
            {
                await this.Mp3Reader.DisposeAsync();
            }

            TrackIndex tr;

            using (var conn = AppServices.GetService<SqliteConnection>())
            {
                tr = await conn.QueryFirstOrDefaultAsync<TrackIndex>("SELECT * FROM TrackIndex WHERE FilePath=@filePath", new {filePath});
            }

            this.Mp3Reader = new(filePath);
            this.WaveOut.Init(this.Mp3Reader);

            this.NowPlayingSongTitle = tr?.Title ?? "Unknown Song";
            this.NowPlayingArtist = tr?.ArtistName ?? "Unknown Artist";
            this.NowPlayingAlbum = tr?.AlbumName ?? "Unknown Album";

            _ = await DbTasks.UpdateLastPlayed(filePath);
        }

        private void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (this.WaveOut.PlaybackState == PlaybackState.Stopped)
            {
                _playTimer?.Stop();
                this.IsPlaying = false;
            }

            this.PlaybackStopped?.Invoke(e);
        }

        public BitmapImage AlbumArtForCurrentTrack()
        {
            //if (this.Tags == null)
            //{
            //    return App.DefaultAlbumArt;
            //}

            //var cs = this.Tags.Tag.Pictures.FirstOrDefault();


            //if (cs == default(IPicture)
            //    || cs.Type == PictureType.NotAPicture
            //    || cs.Type == PictureType.Other)
            //{
            //    return App.DefaultAlbumArt;
            //}

            //using (var stream = new MemoryStream(cs.Data.Data))
            //{
            //    var bitmap = new BitmapImage();
            //    bitmap.BeginInit();
            //    bitmap.StreamSource = stream;
            //    bitmap.CacheOption = BitmapCacheOption.OnLoad;
            //    bitmap.EndInit();
            //    bitmap.Freeze();
            //    return bitmap;
            //}

            return App.DefaultAlbumArt;
        }

        public void Play()
        {
            if (this.Mp3Reader != null)
            {
                this.UpdatePlayTime();
            }

            if (this.WaveOut == null)
            {
                return;
            }

            this.WaveOut.Play();
            this.IsPlaying = true;
            _playTimer.Start();
        }

        public void Pause()
        {
            if (this.WaveOut == null)
            {
                return;
            }

            this.WaveOut.Pause();
            _playTimer.Stop();
            this.IsPlaying = false;
        }

        public void Stop()
        {
            if (this.WaveOut == null)
            {
                return;
            }

            this.WaveOut.Stop();
            _playTimer.Stop();
            this.IsPlaying = false;
        }

        public void Rewind()
        {
            if (this.Mp3Reader != null)
            {
                this.Mp3Reader.Position = 0;
                this.UpdatePlayTime();
            }
        }

        public void FastForward()
        {
            // TODO: This is wrong, it should go to the next song in the playlist or do nothing.
            if (this.Mp3Reader != null)
            {
                this.WaveOut.Stop();
                this.IsPlaying = false;
                this.Mp3Reader.Position = this.Mp3Reader.Length - 1;
                this.UpdatePlayTime();
            }
        }

        public void UpdatePlayTime()
        {
            this.NowPlayingCurrentTime = $"{this.Mp3Reader.CurrentTime.Minutes}:{(this.Mp3Reader.CurrentTime.Seconds > 9 ? this.Mp3Reader.CurrentTime.Seconds : "0" + this.Mp3Reader.CurrentTime.Seconds)}";
            this.NowPlayingTotalTime = $"{this.Mp3Reader.TotalTime.Minutes}:{(this.Mp3Reader.TotalTime.Seconds > 9 ? this.Mp3Reader.TotalTime.Seconds : "0" + this.Mp3Reader.TotalTime.Seconds)}";
        }

        public void Dispose()
        {
            if (this.WaveOut != null)
            {
                this.IsPlaying = false;
                this.WaveOut.Stop();
                this.WaveOut.PlaybackStopped -= WaveOut_PlaybackStopped;
                this.WaveOut.Dispose();
            }

            this.Mp3Reader?.Dispose();
        }
    }
}
