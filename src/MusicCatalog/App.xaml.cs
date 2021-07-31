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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ModernWpf;

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
                sc.AddSingleton(appSettings);
                sc.AddSingleton(mainWindow);
                sc.AddTransient(s => new SqliteConnection(appSettings.DatabaseConnectionString));
            });

            // Since we're loading the main window manually we have to set it here.
            this.MainWindow = AppServices.GetService<MainWindow>();

            // Perform any database changes that need to be made
            var dbMigrations = DeployChanges.To
                .SQLiteDatabase(appSettings.DatabaseConnectionString)
                .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                .LogToNowhere()
                .Build();

            _ = dbMigrations.PerformUpgrade();

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

        /// <summary>
        /// Sets the applications current theme and accent color.
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="accentColor"></param>
        public static void SetTheme(ApplicationTheme theme, Color accentColor)
        {
            var cpr = new ColorPaletteResources { Accent = accentColor };

            Application.Current.Resources.BeginInit();

            ThemeManager.Current.ApplicationTheme = theme;

            // Wipe the slate clean
            Application.Current.Resources.MergedDictionaries.Clear();

            // Step through loading our resources in order.
            Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(@"/Resources/DataTemplates.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary);
            Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(@"/FluentWPF;component/Styles/Controls.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary);

            if (theme == ApplicationTheme.Dark)
            {
                Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(@"/ModernWpf;component/ThemeResources/Dark.xaml", UriKind.Relative)) as ResourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(@"/FluentWPF;component/Styles/Colors.Dark.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(@"/Themes/Dark.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(@"/Avalon.Sqlite;component/Resources/Dark.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary);
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(@"/ModernWpf;component/ThemeResources/Light.xaml", UriKind.Relative)) as ResourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(@"/FluentWPF;component/Styles/Colors.Light.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(@"/Themes/Light.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(@"/Avalon.Sqlite;component/Resources/Light.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary);
            }

            Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(@"/FluentWPF;component/Styles/Brushes.xaml", UriKind.RelativeOrAbsolute)) as ResourceDictionary);
            Application.Current.Resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(@"/ModernWpf;component/ControlsResources.xaml", UriKind.Relative)) as ResourceDictionary);
            Application.Current.Resources.MergedDictionaries.Add(cpr);

            System.Windows.Application.Current.Resources.EndInit();
        }

    }
}