/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using MusicCatalog.Common;
using System;
using System.Windows;
using ModernWpf;

namespace MusicCatalog.Pages
{
    public partial class SqlEditorPage
    {
        private AppSettings _appSettings;

        public SqlEditorPage()
        {
            InitializeComponent();
            _appSettings = AppServices.GetService<AppSettings>();
        }

        private async void SqlEditorPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            QueryControl.ConnectionString = _appSettings.DatabaseConnectionString;

            try
            {
                await QueryControl.RefreshSchemaAsync();
                QueryControl.ExpandTableNode();
                QueryControl.ExpandViewsNode();
            }
            catch (Exception ex)
            {
                // TODO: Log (remove this style message box also).
                MessageBox.Show(ex.Message);
            }
        }
    }
}
