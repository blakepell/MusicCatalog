/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Threading.Tasks;
using MusicCatalog.Pages;
using System.Windows;
using NAudio.Wave;

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

        private WaveOutEvent _waveOut;

        private Mp3FileReader _mp3Reader;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Navigate to the first page we're going to show the user.
            MainFrame.Navigate(typeof(HomePage));

            // Give the search box the initial focus.
            SearchBox.Focus();
        }

        private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
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

        public async Task Play(string fileName)
        {
            if (_waveOut == null)
            {
                _waveOut = new();
            }

            if (_mp3Reader != null)
            {
                _waveOut?.Stop();
                await _mp3Reader.DisposeAsync();
            }

            _mp3Reader = new(fileName);
            _waveOut?.Init(_mp3Reader);
            _waveOut.Play();

            ButtonPlay.Visibility = Visibility.Collapsed;
            ButtonResume.Visibility = Visibility.Collapsed;
            ButtonPause.Visibility = Visibility.Visible;
        }

        public void Resume()
        {
            _waveOut?.Play();

            ButtonPlay.Visibility = Visibility.Collapsed;
            ButtonResume.Visibility = Visibility.Collapsed;
            ButtonPause.Visibility = Visibility.Visible;
        }

        public void Pause()
        {
            if (_waveOut == null)
            {
                return;
            }

            _waveOut?.Pause();

            ButtonPlay.Visibility = Visibility.Collapsed;
            ButtonResume.Visibility = Visibility.Visible;
            ButtonPause.Visibility = Visibility.Collapsed;
        }

    }
}