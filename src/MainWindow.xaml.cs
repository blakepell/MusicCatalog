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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
// ReSharper disable MemberCanBePrivate.Global

namespace MusicCatalog
{
    public partial class MainWindow
    {
        public AudioManager AudioManager { get; }

        public MainWindow()
        {
            InitializeComponent();
            this.AudioManager = new AudioManager();
            this.DataContext = this;
            MainFrame.LoadCompleted += MainFrameOnLoadCompleted;
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
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
            this.AudioManager.Play();
        }

        private void ButtonPause_OnClick(object sender, RoutedEventArgs e)
        {
            this.AudioManager.Pause();
        }

        private void ButtonBack_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.AudioManager.Mp3Reader == null)
            {
                return;
            }

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
            if (this.AudioManager.Mp3Reader == null)
            {
                return;
            }

            if (this.AudioManager.Mp3Reader.Position > 0)
            {
                this.AudioManager.FastForward();
            }
        }

        /// <summary>
        /// Loads the current filename into the AudioManager and prepares the UI for playback.
        /// </summary>
        /// <param name="fileName"></param>
        public async Task Load(string fileName)
        {
            await this.AudioManager.Load(fileName);
            this.NowPlayingAlbumArt.Source = this.AudioManager.AlbumArtForCurrentTrack();
        }

        /// <summary>
        /// Navigates to the search page and passes the search parameter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBox_OnSearch(object? sender, string e)
        {
            // Navigate to the first page we're going to show the user.
            MainFrame.Navigate(typeof(SearchPage), e);
        }

        /// <summary>
        /// When a frame navigation load is completed this will fire.  We can use this
        /// to pass values in the <see cref="NavigationEventArgs"/> to the actual <see cref="Page"/>
        /// that will be using them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainFrameOnLoadCompleted(object sender, NavigationEventArgs e)
        {
            if (e.Content is SearchPage page && e.ExtraData != null)
            {
                page.SearchText = e.ExtraData.ToString();
            }
        }
    }
}