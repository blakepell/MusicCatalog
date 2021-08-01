/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

/*
 * Accent Color Picker
 *
 * @project lead      : Kinnara
 * @website           : http://github.com/Kinnara/ModernWpf
 * @license           : MIT
 */

using ModernWpf;
using MusicCatalog.Common;
using MusicCatalog.Common.Wpf;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MusicCatalog.Controls
{
    /// <summary>
    /// Accent Color Picker Control.
    /// </summary>
    public partial class AccentColorPicker
    {
        public AppSettings Settings { get; set; }

        public AccentColorPicker()
        {
            this.InitializeComponent();
            var settings = AppServices.GetService<AppSettings>();
            this.Settings = settings;
            this.DataContext = this.Settings;
        }

        /// <summary>
        /// Resets the default access color to the one defined for the program.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetAccentColor(object sender, RoutedEventArgs e)
        {
            DispatcherHelper.RunOnMainThread(() =>
            {
                var settings = AppServices.GetService<AppSettings>();
                ThemeManager.Current.AccentColor = settings.DefaultAccentColor;
                App.SetTheme(ApplicationTheme.Light, settings.DefaultAccentColor);
            });
        }

        /// <summary>
        /// When the <see cref="AccentColor"/> has changed the app's theme will be updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ColorGridView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var settings = AppServices.GetService<AppSettings>();
            App.SetTheme(settings.Theme.GetValueOrDefault(ApplicationTheme.Light), this.Settings.AccentColor);
        }
    }

    public class AccentColors : List<AccentColor>
    {
        public AccentColors()
        {
            this.Add("#FFB900", "Yellow gold");
            this.Add("#FF8C00", "Gold");
            this.Add("#F7630C", "Orange bright");
            this.Add("#CA5010", "Orange dark");
            this.Add("#DA3B01", "Rust");
            this.Add("#EF6950", "Pale rust");
            this.Add("#D13438", "Brick red");
            this.Add("#FF4343", "Mod red");
            this.Add("#E74856", "Pale red");
            this.Add("#E81123", "Red");
            this.Add("#EA005E", "Rose bright");
            this.Add("#C30052", "Rose");
            this.Add("#E3008C", "Plum light");
            this.Add("#BF0077", "Plum");
            this.Add("#C239B3", "Orchid light");
            this.Add("#9A0089", "Orchid");
            this.Add("#0078D7", "Default blue");
            this.Add("#0063B1", "Navy blue");
            this.Add("#8E8CD8", "Purple shadow");
            this.Add("#6B69D6", "Purple shadow Dark");
            this.Add("#8764B8", "Iris pastel");
            this.Add("#744DA9", "Iris spring");
            this.Add("#B146C2", "Violet red light");
            this.Add("#881798", "Violet red");
            this.Add("#0099BC", "Cool blue bright");
            this.Add("#2D7D9A", "Cool blue");
            this.Add("#00B7C3", "Seafoam");
            this.Add("#038387", "Seafoam team");
            this.Add("#00B294", "Mint light");
            this.Add("#018574", "Mint dark");
            this.Add("#00CC6A", "Turf green");
            this.Add("#10893E", "Sport green");
            this.Add("#7A7574", "Gray");
            this.Add("#5D5A58", "Gray brown");
            this.Add("#68768A", "Steel blue");
            this.Add("#515C6B", "Metal blue");
            this.Add("#567C73", "Pale moss");
            this.Add("#486860", "Moss");
            this.Add("#498205", "Meadow green");
            this.Add("#107C10", "Green");
            this.Add("#767676", "Overcast");
            this.Add("#4C4A48", "Storm");
            this.Add("#69797E", "Blue gray");
            this.Add("#4A5459", "Gray dark");
            this.Add("#647C64", "Liddy green");
            this.Add("#525E54", "Sage");
            this.Add("#847545", "Camouflage desert");
            this.Add("#7E735F", "Camouflage");
        }

        private void Add(string color, string name)
        {
            this.Add(new AccentColor((Color)ColorConverter.ConvertFromString(color), name));
        }
    }

    public class AccentColor
    {
        public AccentColor(Color color, string name)
        {
            this.Color = color;
            this.Name = name;
            this.Brush = new SolidColorBrush(color);
        }

        public Color Color { get; }

        public string Name { get; }

        public SolidColorBrush Brush { get; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}