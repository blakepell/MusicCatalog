/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Configs;
using DbUp;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MusicCatalog.Common;
using MusicCatalog.Common.Models;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MusicCatalog
{
    public partial class App
    {
        internal static BitmapImage DefaultAlbumArt;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var appSettings = ConfigsTools.GetConfigs<AppSettings>();
            var mainWindow = new MainWindow();

            // Register our services with the dependency injection.
            AppServices.Init((sc) =>
            {
                sc.AddSingleton<AppSettings>(appSettings);
                sc.AddSingleton<MainWindow>(mainWindow);
                sc.AddTransient<SqliteConnection>(s => new SqliteConnection(appSettings.DatabaseConnectionString));
            });

            // Since we're loading the main window manually we have to set it here.
            this.MainWindow = AppServices.GetService<MainWindow>();

            // Perform any database changes that need to be made
            var dbMigrations = DeployChanges.To
                .SQLiteDatabase(appSettings.DatabaseConnectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToNowhere()
                .Build();

            var result = dbMigrations.PerformUpgrade();

            // Default settings if required.
            if (!appSettings.MusicDirectoryList.Any())
            {
                appSettings.MusicDirectoryList.Add(new IndexDirectory(Environment.GetFolderPath(Environment.SpecialFolder.CommonMusic)));
                appSettings.MusicDirectoryList.Add(new IndexDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)));
            }

            // Since this will be used a lot we're going to cache a static copy that's frozen.
            DefaultAlbumArt = new BitmapImage(new Uri("Assets/Unknown.png", UriKind.Relative))
            {
                CacheOption = BitmapCacheOption.OnLoad
            };

            DefaultAlbumArt.Freeze();

            mainWindow.Show();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            var settings = AppServices.GetService<AppSettings>();
            settings.Save();
        }
    }
}