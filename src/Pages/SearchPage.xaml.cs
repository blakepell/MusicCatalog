/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Windows;
using System.Windows.Controls;

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
        }
    }
}
