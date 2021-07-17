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
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Configs;
using DbUp;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using MusicCatalog.Common;

namespace MusicCatalog
{
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var appSettings = ConfigsTools.GetConfigs<AppSettings>();
            var mainWindow = new MainWindow();

            AppServices.Init((sc) =>
            {
                sc.AddSingleton<AppSettings>(appSettings);
                sc.AddSingleton<MainWindow>(mainWindow);
                sc.AddTransient<SqliteConnection>(s => new SqliteConnection(appSettings.DatabaseConnectionString));
            });

            this.MainWindow = AppServices.GetService<MainWindow>();

            var dbMigrations = DeployChanges.To
                .SQLiteDatabase(appSettings.DatabaseConnectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToNowhere()
                .Build();

            var result = dbMigrations.PerformUpgrade();

            mainWindow.Show();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            var settings = AppServices.GetService<AppSettings>();
            settings.Save();
        }
    }
}