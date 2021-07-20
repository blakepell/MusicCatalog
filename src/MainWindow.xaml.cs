/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Threading.Tasks;
using MusicCatalog.Common;
using MusicCatalog.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TagLib.IFD.Entries;
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
            this.AudioManager.PlaybackStopped = new((e =>
            {
                if (this.AudioManager.Mp3Reader.Position == this.AudioManager.Mp3Reader.Length
                    || this.AudioManager.Mp3Reader.Position + 1 >= this.AudioManager.Mp3Reader.Length)
                {
                    this.ButtonPlay.Visibility = Visibility.Visible;
                    this.ButtonPause.Visibility = Visibility.Collapsed;
                }
            }));

            this.DataContext = this;

            MainFrame.LoadCompleted += MainFrameOnLoadCompleted;
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Navigate to the first page we're going to show the user.
            MainFrame.Navigate(typeof(HomePage));

            // TODO: Temp, remove this but show the last song played if it exists.
            await this.Load(@"C:\Music\Blake Pell - Pandemic - Quiet as a Mouse (June 2020).mp3");
            //await this.Load("C:\\Music\\Blake Pell - Pandemic - Jesus Breaks Your Heart (June 2021).mp3");
            //await this.Load(@"C:\Music\Richard Edwards - The Bride On The Boxcar - A Decade Of Margot Rarities- 2004-2014 - 46 Jesus Breaks Your Heart - Demo.mp3");
            // await this.Load(@"C:\Music\The Beatles - I Don't Want To Spoil The Party (Remastered).mp3");
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
            this.ButtonPlay.Visibility = Visibility.Collapsed;
            this.ButtonPause.Visibility = Visibility.Visible;
        }

        private void ButtonPause_OnClick(object sender, RoutedEventArgs e)
        {
            this.AudioManager.Pause();
            this.ButtonPlay.Visibility = Visibility.Visible;
            this.ButtonPause.Visibility = Visibility.Collapsed;
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