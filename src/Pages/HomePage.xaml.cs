/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Dapper;
using Microsoft.Data.Sqlite;
using MusicCatalog.Common;
using System.Windows;
using System.Windows.Controls;

namespace MusicCatalog.Pages
{
    public partial class HomePage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Runs every time the page is loaded regardless of it's just being called back
        /// into memory from a previous run.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HomePage_OnLoaded(object sender, RoutedEventArgs e)
        {
            using (var db = AppServices.GetService<SqliteConnection>())
            {
                db.Open();
                int songCount = db.ExecuteScalar<int>("select count(*) from track");

                for (int i = 0; i < 10; i++)
                {
                    TextBlockTest.Text += $"{songCount} songs listed.\r\n";
                }

            }

        }
    }
}
