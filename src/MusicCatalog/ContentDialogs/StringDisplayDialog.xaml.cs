/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System.Windows;

namespace MusicCatalog.ContentDialogs
{
    /// <summary>
    /// A dialog that can show string information to the user.
    /// </summary>
    public partial class StringDisplayDialog
    {
        public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register(
            nameof(DisplayText), typeof(string), typeof(StringDisplayDialog), new PropertyMetadata(default(string)));

        /// <summary>
        /// The text to display in the TextBox of the dialog.
        /// </summary>
        public string DisplayText
        {
            get => (string) GetValue(DisplayTextProperty);
            set => SetValue(DisplayTextProperty, value);
        }

        public StringDisplayDialog()
        {
            InitializeComponent();
            this.DataContext = this;
        }
    }
}
