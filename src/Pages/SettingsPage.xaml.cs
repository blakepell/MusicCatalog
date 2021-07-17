/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Argus.Extensions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using MusicCatalog.Common;
using MusicCatalog.Common.Models;

namespace MusicCatalog.Pages
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
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
            //_appSettings.Save();
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

                    var root = new DirectoryInfo(@"C:\Music");
                    var enumerable = new FileSystemEnumerable(root, "*.mp3", SearchOption.TopDirectoryOnly);

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

                    await conn.ExecuteAsync("COMMIT");
                }

            });

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", _appSettings.LocalAppData.AssemblyFolderPath);
        }
    }
}