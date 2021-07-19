/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MusicCatalog.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NAudio.Wave;
using TagLib;
using Argus.Extensions;

namespace MusicCatalog
{
    public partial class MainWindow
    {
        public static readonly DependencyProperty NowPlayingSongTitleProperty = DependencyProperty.Register(
            nameof(NowPlayingSongTitle), typeof(string), typeof(MainWindow), new PropertyMetadata("No Song Info"));

        public string NowPlayingSongTitle
        {
            get => (string) GetValue(NowPlayingSongTitleProperty);
            set => SetValue(NowPlayingSongTitleProperty, value);
        }

        public static readonly DependencyProperty NowPlayingArtistProperty = DependencyProperty.Register(
            nameof(NowPlayingArtist), typeof(string), typeof(MainWindow), new PropertyMetadata("No Artist Info"));

        public string NowPlayingArtist
        {
            get => (string) GetValue(NowPlayingArtistProperty);
            set => SetValue(NowPlayingArtistProperty, value);
        }

        public static readonly DependencyProperty NowPlayingAlbumProperty = DependencyProperty.Register(
            nameof(NowPlayingAlbum), typeof(string), typeof(MainWindow), new PropertyMetadata("No Album Info"));

        public string NowPlayingAlbum
        {
            get => (string) GetValue(NowPlayingAlbumProperty);
            set => SetValue(NowPlayingAlbumProperty, value);
        }

        public static readonly DependencyProperty NowPlayingTotalTimeProperty = DependencyProperty.Register(
            nameof(NowPlayingTotalTime), typeof(string), typeof(MainWindow), new PropertyMetadata("0:00"));

        public string NowPlayingTotalTime
        {
            get => (string) GetValue(NowPlayingTotalTimeProperty);
            set => SetValue(NowPlayingTotalTimeProperty, value);
        }

        public static readonly DependencyProperty PropertyTypeProperty = DependencyProperty.Register(
            nameof(NowPlayingCurrentTime), typeof(string), typeof(MainWindow), new PropertyMetadata("0:00"));

        public string NowPlayingCurrentTime
        {
            get => (string) GetValue(PropertyTypeProperty);
            set => SetValue(PropertyTypeProperty, value);
        }

        private WaveOutEvent _waveOut;

        private Mp3FileReader _mp3Reader;

        private DispatcherTimer _playTimer;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            _playTimer = new();
            _playTimer.Tick += new EventHandler(delegate(object? o, EventArgs args)
            {
                this.NowPlayingCurrentTime = $"{_mp3Reader.CurrentTime.Minutes}:{(_mp3Reader.CurrentTime.Seconds > 9 ? _mp3Reader.CurrentTime.Seconds : "0" + _mp3Reader.CurrentTime.Seconds)}";
            });
            _playTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            // Navigate to the first page we're going to show the user.
            MainFrame.Navigate(typeof(HomePage));

            // Give the search box the initial focus.
            SearchBox.Focus();
        }

        private void ButtonPageBack_OnClick(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private async void ButtonPlayAsync_OnClick(object sender, RoutedEventArgs e)
        {
            await this.Play(@"C:\Music\Richard Edwards - The Bride On The Boxcar - A Decade Of Margot Rarities- 2004-2014 - 46 Jesus Breaks Your Heart - Demo.mp3");
        }

        private void ButtonPause_OnClick(object sender, RoutedEventArgs e)
        {
            this.Pause();
        }

        private void ButtonResume_OnClick(object sender, RoutedEventArgs e)
        {
            this.Resume();
        }

        public void Back()
        {
            if (_mp3Reader != null)
            {
                _mp3Reader.Position = 0;
            }
        }

        public async Task Play(string fileName)
        {
            if (_waveOut == null)
            {
                _waveOut = new();
                _waveOut.PlaybackStopped += _waveOut_PlaybackStopped;
            }

            if (_mp3Reader != null)
            {
                _waveOut?.Stop();
                await _mp3Reader.DisposeAsync();
            }

            var song = TagLib.File.Create(fileName);
            this.NowPlayingSongTitle = song.Tag.Title;
            this.NowPlayingArtist = song.Tag.FirstAlbumArtist;
            this.NowPlayingAlbum = song.Tag.Album;

            var cs = song.Tag.Pictures.FirstOrDefault();

            if (cs != default(IPicture))
            {
                using (var stream = new MemoryStream(cs.Data.Data))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    this.NowPlayingAlbumArt.Source = bitmap;
                }
            }

            _mp3Reader = new(fileName);

            this.NowPlayingTotalTime = $"{_mp3Reader.TotalTime.Minutes}:{(_mp3Reader.TotalTime.Seconds > 9 ? _mp3Reader.TotalTime.Seconds : "0" + _mp3Reader.TotalTime.Seconds)}";

            _waveOut?.Init(_mp3Reader);
            _waveOut.Play();
            _playTimer.Start();

            ButtonPlay.Visibility = Visibility.Collapsed;
            ButtonResume.Visibility = Visibility.Collapsed;
            ButtonPause.Visibility = Visibility.Visible;
        }

        public void Resume()
        {
            _waveOut?.Play();
            _playTimer.Start();
            ButtonPlay.Visibility = Visibility.Collapsed;
            ButtonResume.Visibility = Visibility.Collapsed;
            ButtonPause.Visibility = Visibility.Visible;
        }

        private void _waveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            this.Stop();
            _playTimer.Stop();
            this.Title = "Stopped @" + DateTime.Now.ToString();
        }

        public void Pause()
        {
            if (_waveOut == null)
            {
                return;
            }

            _waveOut?.Pause();
            _playTimer.Stop();

            ButtonPlay.Visibility = Visibility.Collapsed;
            ButtonResume.Visibility = Visibility.Visible;
            ButtonPause.Visibility = Visibility.Collapsed;
        }

        public void Stop()
        {
            if (_waveOut == null)
            {
                return;
            }

            _waveOut?.Stop();
            _playTimer.Stop();

            ButtonPlay.Visibility = Visibility.Visible;
            ButtonResume.Visibility = Visibility.Collapsed;
            ButtonPause.Visibility = Visibility.Collapsed;
        }

        private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
        {
            this.Back();
        }
    }
}