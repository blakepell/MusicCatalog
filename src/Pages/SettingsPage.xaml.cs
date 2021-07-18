/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Argus.Extensions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using MusicCatalog.Common;
using MusicCatalog.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace MusicCatalog.Pages
{
    public partial class SettingsPage
    {
        private AppSettings _appSettings;

        public SettingsPage()
        {
            InitializeComponent();
            _appSettings = AppServices.GetService<AppSettings>();
            this.DataContext = _appSettings;
        }

        private void SettingsPage_OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private async void ButtonBase_OnClickAsync(object sender, RoutedEventArgs e)
        {
            ProgressRing.IsActive = true;

            var indexer = new FileIndexer();

            var sw = new Stopwatch();

            sw.Start();
            await indexer.RebuildIndex();
            sw.Stop();
            TextStatus.Text = $"Rebuild took {sw.Elapsed.TotalSeconds.ToString()}s => ";

            sw.Restart();
            await indexer.GenerateMd5Hashes();
            sw.Stop();
            TextStatus.Text = $"MD5 hashes took {sw.Elapsed.TotalSeconds.ToString()}s";

            ProgressRing.IsActive = false;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", _appSettings.LocalAppData.AssemblyFolderPath);
        }

        /// <summary>
        /// Key handling for the folder list view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewMusicDirectories_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    e.Handled = true;

                    // Casting the selected items to an array to avoid the enumerable changing exception.
                    var selected = ListViewMusicDirectories.SelectedItems.Cast<IndexDirectory>().ToArray();

                    foreach (var item in selected)
                    {
                        _appSettings.MusicDirectoryList.Remove(item);
                    }

                    break;
                case Key.Escape:
                    e.Handled = true;
                    ListViewMusicDirectories.SelectedItems.Clear();
                    break;
            }
        }

        /// <summary>
        /// Allows the user to add a folder location to be indexed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAddDirectory_OnClick(object sender, RoutedEventArgs e)
        {
            var fd = new FolderBrowserDialog();
            var result = fd.ShowDialog();

            if (result == DialogResult.OK)
            {
                _appSettings.MusicDirectoryList.Add( new IndexDirectory(fd.SelectedPath));
            }
        }
    }
}