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
using ModernWpf;
using MusicCatalog.Common;
using MusicCatalog.Common.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MusicCatalog.Theme;

namespace MusicCatalog
{
    public partial class App
    {
        internal static BitmapImage DefaultAlbumArt;

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var appSettings = ConfigsTools.GetConfigs<AppSettings>();

            if (!Directory.Exists(appSettings.CacheFolder))
            {
                Directory.CreateDirectory(appSettings.CacheFolder);
            }

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

            // If there is no accent color, set it from the actual accent color that's used in
            // the theme manager which -should- be from Windows.
            if (appSettings.AccentColor == null)
            {
                appSettings.AccentColor = ThemeManager.Current.AccentColor ?? ThemeManager.Current.ActualAccentColor;
            }
            else
            {
                ApplyTheme(appSettings.Theme, appSettings.AccentColor);
            }

            mainWindow.Show();
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            var settings = AppServices.GetService<AppSettings>();
            settings.Save();
        }

        /// <summary>
        /// Gets the current application theme resource instance.
        /// </summary>
        public static ResourceDictionary GetCurrentThemeDictionary()
        {
            // Determine the current theme by looking at the application resources and return the first dictionary having the resource key 'Brush_AppearanceService' defined.
            return (from dict in Application.Current.Resources.MergedDictionaries
                    where dict.Contains("Brush_AppearanceService")
                    select dict).FirstOrDefault();
        }

        /// <summary>
        /// Gets the current application color palette resource instance.
        /// </summary>
        public static ColorPaletteResourcesEx GetCurrentColorPalette()
        {
            // Determine the current color palette by looking at the application resources and return the first dictionary having the resource key 'Brush_AppearanceService' defined.
            return (ColorPaletteResourcesEx)(from dict in Application.Current.Resources.MergedDictionaries
                    where dict.Contains("ColorPalette_AppearanceService")
                    select dict).FirstOrDefault();
        }

        /// <summary>
        /// Applies a new theme and accent color.
        /// </summary>
        /// <param name="theme"></param>
        /// <param name="accentColor"></param>
        public static void ApplyTheme(ApplicationTheme? theme, Color? accentColor)
        {
            string themeName = "Dark";
            ThemeManager.Current.ApplicationTheme = theme;

            if (theme == ApplicationTheme.Light)
            {
                themeName = "Light";
            }

            ResourceDictionary currentThemeDict = GetCurrentThemeDictionary();

            var newThemeDict = new ResourceDictionary { Source = new Uri($"/MusicCatalog;component/Themes/{themeName}.xaml", UriKind.RelativeOrAbsolute) };

            if (accentColor != null)
            {
                var currentCpr = GetCurrentColorPalette();

                var cpr = new ColorPaletteResourcesEx { Accent = accentColor };
                Application.Current.Resources.MergedDictionaries.Add(cpr);

                if (currentCpr != null)
                {
                    Application.Current.Resources.MergedDictionaries.Remove(currentCpr);
                }

            }

            // Prevent exceptions by adding the new dictionary before removing the old one
            Application.Current.Resources.MergedDictionaries.Add(newThemeDict);
            Application.Current.Resources.MergedDictionaries.Remove(currentThemeDict);
        }
    }
}