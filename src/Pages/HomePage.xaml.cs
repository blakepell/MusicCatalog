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
using Dapper;
using Microsoft.Data.Sqlite;
using MusicCatalog.Common;

namespace MusicCatalog.Pages
{
    public partial class HomePage : Page
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
