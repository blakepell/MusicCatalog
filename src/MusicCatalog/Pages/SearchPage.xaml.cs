/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using ModernWpf.Controls;
using MusicCatalog.Common;
using MusicCatalog.Common.Models;
using MusicCatalog.Common.Commands;
using MusicCatalog.Common.Extensions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

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
        }

        private async void SearchResultsView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Track tr)
            {
                var conveyor = AppServices.CreateInstance<Conveyor>();
                await conveyor.PlayTrack(tr.FilePath);
            }
        }

        public async Task ExecuteSearch()
        {
            var tracks = await DbTasks.SearchTracks(this.SearchText);

            foreach (var t in tracks)
            {
                if (!t.TagsProcessed)
                {
                    await t.UpdateTags();
                }
            }

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

    }
}