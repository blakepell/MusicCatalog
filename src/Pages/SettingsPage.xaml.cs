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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

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
            await this.LoadDb();
            ProgressRing.IsActive = false;
        }

        public async Task LoadDb()
        {
            await Task.Run(async () =>
            {
                using (var conn = AppServices.GetService<SqliteConnection>())
                {
                    await conn.OpenAsync();

                    await conn.ExecuteAsync("DELETE FROM Track");
                    await conn.ExecuteAsync("VACUUM");
                    await conn.ExecuteAsync("BEGIN");

                    var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                        {  ".mp3", ".wav" };

                    foreach (var dir in _appSettings.MusicDirectoryList)
                    {
                        if (!Directory.Exists(dir.DirectoryPath))
                        {
                            continue;
                        }

                        var root = new DirectoryInfo(dir.DirectoryPath);
                        var enumerable = new FileSystemEnumerable(root, "*", dir.ToSearchOption()).Where(x => extensions.Contains(Path.GetExtension(x.FullName)));

                        var fileCount = enumerable.Count();
                        //fileCount = Directory.EnumerateDirectories(settings.Directory, "*", so).Count();
                        // TODO - Same exclusions as below
                        // fileCount = enumerable.Count();

                        //foreach (var file in Directory.EnumerateFiles(settings.Directory, "*.*", so))
                        foreach (var fs in enumerable)
                        {
                            // Skip directories and junctions.
                            if (fs.Attributes.HasFlag(FileAttributes.Directory) || fs.Attributes.HasFlag(FileAttributes.ReparsePoint))
                            {
                                continue;
                            }

                            var fi = fs as FileInfo;

                            if (fi == null)
                            {
                                continue;
                            }

                            var tr = new Track
                            {
                                FilePath = fi.FullName,
                                FileName = fi.Name,
                                DirectoryPath = fi.DirectoryName,
                                Extension = fi.Extension.ToLower(),
                                FileSize = fi.Length,
                                DateCreated = fi.CreationTime,
                                DateModified = fi.LastWriteTime,
                                DateLastIndexed = DateTime.Now,
                                TagsProcessed = false
                            };

                            //MD5 = fi.CreateMD5(),
                            //tr.MD5 = fi.CreateMD5();

                            _ = await conn.InsertAsync(tr);
                        }
                    }

                    await conn.ExecuteAsync("COMMIT");
                }

            });

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