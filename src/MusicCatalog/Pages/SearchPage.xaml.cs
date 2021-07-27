﻿/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using System.Text;
using System.Dynamic;
using System.Linq;
using ModernWpf.Controls;
using MusicCatalog.Common;
using MusicCatalog.Common.Models;
using MusicCatalog.Common.Commands;
using MusicCatalog.Common.Extensions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Argus.Extensions;
using MusicCatalog.ContentDialogs;
using Newtonsoft.Json;
using TagLib;

namespace MusicCatalog.Pages
{
    public partial class SearchPage
    {
        public static readonly DependencyProperty SearchTextProperty = DependencyProperty.Register(
            nameof(SearchText), typeof(string), typeof(SearchPage), new PropertyMetadata(default(string)));

        public string SearchText
        {
            get => (string) GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }
        public SearchPage()
        {
            InitializeComponent();
            this.DataContext = this;
            this.CommandBindings.Add(new CommandBinding(DataTemplateCommands.CopyFilePath, MenuItem_CopyFilePathClicked));
            this.CommandBindings.Add(new CommandBinding(DataTemplateCommands.DisplayIdTagInfo, MenuItem_DisplayIdTagInfoClickedAsync));
        }

        private async void SearchResultsView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TrackIndex tr)
            {
                var conveyor = AppServices.CreateInstance<Conveyor>();
                await conveyor.PlayTrack(tr.FilePath);
            }
        }

        public async Task ExecuteSearch()
        {
            var tracks = await DbTasks.SearchTracks(this.SearchText);
            SearchResultsView.ItemsSource = await DbTasks.SearchTracks(this.SearchText);
            TextResultsCount.Text = $"{SearchResultsView.Items.Count.ToString()} ";
        }

        public async Task ExecuteSearch(string searchTerm)
        {
            this.SearchText = searchTerm;
            await this.ExecuteSearch();
        }

        /// <summary>
        /// Attempts to copy the file path from the selected item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void MenuItem_CopyFilePathClicked(object sender, ExecutedRoutedEventArgs args)
        {
            try
            {
                if (args?.Parameter != null)
                {
                    Clipboard.SetText(args.Parameter.ToString() ?? "");
                }
            }
            catch
            {
                // TODO: Log error
            }
        }

        private async void MenuItem_DisplayIdTagInfoClickedAsync(object sender, ExecutedRoutedEventArgs args)
        {
            try
            {
                if (args?.Parameter != null)
                {
                    if (System.IO.File.Exists(args.Parameter.ToString()))
                    {
                        File tags = TagLib.File.Create(args.Parameter.ToString());
                        
                        //var id3v1 = tags.GetTag(TagTypes.Id3v1);
                        //var id3v2 = tags.GetTag(TagTypes.Id3v2);

                        var sb = new StringBuilder();

                        sb.AppendLine("/* tag */");
                        sb.AppendLine(JsonConvert.SerializeObject(tags.Tag, Formatting.Indented));

                        sb.AppendLine("/* properties */");
                        sb.AppendLine(JsonConvert.SerializeObject(tags.Properties, Formatting.Indented));

                        //sb.AppendLine("/* Id3v1 */");
                        //sb.AppendLine(JsonConvert.SerializeObject(id3v1, Formatting.Indented));

                        //sb.AppendLine("\r\n/* Id3v2 */");
                        //sb.AppendLine(JsonConvert.SerializeObject(id3v2, Formatting.Indented));

                        var dialog = new StringDisplayDialog()
                        {
                            Title = "ID Tag Info",
                            DisplayText = sb.ToString()
                        };

                        _ = await dialog.ShowAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                int x = 0;
                // TODO: Log error
            }
        }

    }
}