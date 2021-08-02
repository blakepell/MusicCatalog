/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using ModernWpf.Controls;
using MusicCatalog.Common;
using MusicCatalog.Common.Models;
using System.Windows;
using System.Windows.Controls;
using GridView = ModernWpf.Controls.GridView;

namespace MusicCatalog.Pages
{
    public partial class HomePage
    {
        public HomePage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// Runs every time the page is loaded regardless of it's just being called back
        /// into memory from a previous run.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void HomePage_OnLoaded(object sender, RoutedEventArgs e)
        {
            RecentPlaysView.BeginInit();

            try
            {
                RecentPlaysView.ItemsSource = await DbTasks.RecentPlays(15);
            }
            catch
            {
                // TODO: Error Logging
            }
            finally
            {
                RecentPlaysView.EndInit();
            }
        }

        private async void RecentPlaysView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TrackIndex tr)
            {
                var conveyor = AppServices.CreateInstance<Conveyor>();
                await conveyor.PlayTrack(tr.FilePath);
            }
        }

        private void RecentPlaysView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is GridView gv)
            {
                gv.SelectedIndex = -1;
            }
        }
    }
}