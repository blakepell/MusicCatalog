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

namespace MusicCatalog.Controls
{
    public partial class InfoOverlay : UserControl
    {
        public InfoOverlay()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty TopTextProperty = DependencyProperty.Register(
            nameof(TopText), typeof(string), typeof(InfoOverlay), new PropertyMetadata(default(string)));

        public string TopText
        {
            get => (string) GetValue(TopTextProperty);
            set => SetValue(TopTextProperty, value);
        }

        public static readonly DependencyProperty BottomTextProperty = DependencyProperty.Register(
            nameof(BottomText), typeof(string), typeof(InfoOverlay), new PropertyMetadata(default(string)));

        public string BottomText
        {
            get => (string) GetValue(BottomTextProperty);
            set => SetValue(BottomTextProperty, value);
        }

        public static readonly DependencyProperty ProgressIsActiveProperty = DependencyProperty.Register(
            nameof(ProgressIsActive), typeof(bool), typeof(InfoOverlay), new PropertyMetadata(default(bool)));

        public bool ProgressIsActive
        {
            get => (bool) GetValue(ProgressIsActiveProperty);
            set => SetValue(ProgressIsActiveProperty, value);
        }

        /// <summary>
        /// Shows the overlay control.
        /// </summary>
        public void Show()
        {
            this.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Hides the overlay control.
        /// </summary>
        public void Hide()
        {
            this.ProgressIsActive = false;
            this.Visibility = Visibility.Collapsed;
        }
    }
}
