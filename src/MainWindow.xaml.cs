/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using MusicCatalog.Pages;
using System.Windows;

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
    }
}