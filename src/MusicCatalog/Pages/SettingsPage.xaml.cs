/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using Argus.Extensions;
using MusicCatalog.Common;
using MusicCatalog.Common.Models;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using ModernWpf;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MusicCatalog.Common.Wpf;
using MusicCatalog.Theme;
using Application = System.Windows.Application;

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
            var indexer = new FileIndexer();
            await indexer.RebuildIndex();
        }

        private async void ButtonIndexTags_OnClickAsync(object sender, RoutedEventArgs e)
        {
            var indexer = new FileIndexer();
            await indexer.IndexTags();
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

        private ResourceDictionary GetCurrentThemeDictionary()
        {
            // Determine the current theme by looking at the application resources and return the first dictionary having the resource key 'Brush_AppearanceService' defined.
            return (from dict in Application.Current.Resources.MergedDictionaries
                    where dict.Contains("Brush_AppearanceService")
                    select dict).FirstOrDefault();
        }

        private void ButtonToggleTheme_OnClick(object sender, RoutedEventArgs e)
        {
            DispatcherHelper.RunOnMainThread(() =>
            {
                if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
                {
                    App.ApplyTheme(ApplicationTheme.Light, Colors.Green);
                }
                else
                {
                    App.ApplyTheme(ApplicationTheme.Dark, Colors.DarkGoldenrod);
                }
            });
        }

        /// <summary>
        /// Changes the theme between light, dark and the system vault.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButtonsTheme_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1)
            {
                if (e.AddedItems[0] is AppTheme item)
                {
                    if (item.Value != null)
                    {
                        App.ApplyTheme(item.Value, _appSettings.AccentColor);
                    }
                }
            }
        }
    }
}