/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using MusicCatalog.Common;
using MusicCatalog.Pages;
using System.Windows;

namespace MusicCatalog
{
    public partial class MainWindow
    {
        public AudioManager AudioManager { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            this.AudioManager = new AudioManager();
            this.DataContext = this;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
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
            await this.AudioManager.Load(@"C:\Music\Richard Edwards - The Bride On The Boxcar - A Decade Of Margot Rarities- 2004-2014 - 46 Jesus Breaks Your Heart - Demo.mp3");
            this.NowPlayingAlbumArt.Source = this.AudioManager.AlbumArtForCurrentTrack();
            this.AudioManager.Play();

            this.ButtonPlay.Visibility = Visibility.Collapsed;
            this.ButtonResume.Visibility = Visibility.Collapsed;
            this.ButtonPause.Visibility = Visibility.Visible;
        }

        private void ButtonPause_OnClick(object sender, RoutedEventArgs e)
        {
            this.AudioManager.Pause();
            this.ButtonPlay.Visibility = Visibility.Collapsed;
            this.ButtonResume.Visibility = Visibility.Visible;
            this.ButtonPause.Visibility = Visibility.Collapsed;
        }

        private void ButtonResume_OnClick(object sender, RoutedEventArgs e)
        {
            this.AudioManager.Resume();
            this.ButtonPlay.Visibility = Visibility.Collapsed;
            this.ButtonResume.Visibility = Visibility.Collapsed;
            this.ButtonPause.Visibility = Visibility.Visible;
        }

        private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
        {
            // If the position is greater than 0 then rewind to the start of the track
            // but continue playing.  If the position is 0, go then start going back
            // through previous songs in the stack.
            if (this.AudioManager.Mp3Reader.Position > 0)
            {
                this.AudioManager.Rewind();
            }
        }

        private void ButtonForward_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.AudioManager.Mp3Reader.Position > 0)
            {
                this.AudioManager.FastForward();
            }
        }
    }
}