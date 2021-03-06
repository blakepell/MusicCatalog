/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MusicCatalog.Controls
{
    public partial class NavSearchBox
    {
        public NavSearchBox()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty SearchBoxIsFocusedProperty = DependencyProperty.Register(
            nameof(SearchBoxIsFocused), typeof(bool), typeof(NavSearchBox), new PropertyMetadata(false));

        public bool SearchBoxIsFocused
        {
            get => (bool) GetValue(SearchBoxIsFocusedProperty);
            set => SetValue(SearchBoxIsFocusedProperty, value);
        }

        public static readonly DependencyProperty InactiveBrushProperty = DependencyProperty.Register(
            nameof(InactiveBrush), typeof(Brush), typeof(NavSearchBox),
            new PropertyMetadata((SolidColorBrush) new BrushConverter().ConvertFrom("#66CCCCCC")));

        public Brush InactiveBrush
        {
            get => (Brush) GetValue(InactiveBrushProperty);
            set => SetValue(InactiveBrushProperty, value);
        }

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            nameof(IsActive), typeof(bool), typeof(NavSearchBox), new PropertyMetadata(false));

        public bool IsActive
        {
            get => (bool) GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty ClearTextOnSearchProperty = DependencyProperty.Register(
            nameof(ClearTextOnSearch), typeof(bool), typeof(NavSearchBox), new PropertyMetadata(true));

        public bool ClearTextOnSearch
        {
            get => (bool) GetValue(ClearTextOnSearchProperty);
            set => SetValue(ClearTextOnSearchProperty, value);
        }

        public static readonly DependencyProperty ProgressRingIsActiveProperty = DependencyProperty.Register(
            nameof(ProgressRingIsActive), typeof(bool), typeof(NavSearchBox), new PropertyMetadata(false));

        public bool ProgressRingIsActive
        {
            get => (bool) GetValue(ProgressRingIsActiveProperty);
            set => SetValue(ProgressRingIsActiveProperty, value);
        }

        public event EventHandler<string> Search;

        /// <summary>
        /// Sets the focus to the TextBox that represents the search box in this control.
        /// </summary>
        public new bool Focus()
        {
            return SearchBox.Focus();
        }

        private void SearchBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            this.SearchBoxIsFocused = true;
        }

        private void SearchBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            this.SearchBoxIsFocused = false;
        }

        private void SearchBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;

                    if (string.IsNullOrWhiteSpace(SearchBox.Text))
                    {
                        return;
                    }

                    this.Search?.Invoke(this, SearchBox.Text);

                    if (ClearTextOnSearch)
                    {
                        SearchBox.Text = "";
                    }

                    break;
                case Key.Escape:
                    e.Handled = true;
                    SearchBox.Text = "";
                    break;
            }
        }
    }
}