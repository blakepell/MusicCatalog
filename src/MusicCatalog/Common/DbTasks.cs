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
using MusicCatalog.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MusicCatalog.Common
{
    public static class DbTasks
    {
        /// <summary>
        /// Updates the last played timestamp for a specified file.
        /// </summary>
        /// <param name="fileName">The full path to the file that should be updated.</param>
        public static async Task<int> UpdateLastPlayed(string fileName)
        {
            await using var db = AppServices.GetService<SqliteConnection>();
            await db.OpenAsync();
            await db.ExecuteAsync("UPDATE Track SET PlayCount = PlayCount + 1 WHERE FilePath = @fileName", new { DateLastPlayed = DateTime.Now, fileName });
            return await db.ExecuteAsync("UPDATE Track SET DateLastPlayed = @DateLastPlayed WHERE FilePath = @fileName", new { DateLastPlayed = DateTime.Now, fileName });
        }

        /// <summary>
        /// Returns the most recently played songs.
        /// </summary>
        /// <param name="count">The number of tracks to return.</param>
        public static async Task<IEnumerable<Track>> RecentPlays(int count)
        {
            count = Math.Clamp(1, count, 100);
            await using var db = AppServices.GetService<SqliteConnection>();
            await db.OpenAsync();
            return await db.QueryAsync<Track>($"SELECT * FROM Track Where PlayCount > 0 ORDER BY DateLastPlayed DESC LIMIT {count}");
        }

        /// <summary>
        /// Searches the tracks by keyword.
        /// </summary>
        /// <param name="searchTerm"></param>
        public static async Task<IEnumerable<Track>> SearchTracks(string searchTerm)
        {
            searchTerm = searchTerm.Replace('*', '%');

            if (!searchTerm.Contains('%'))
            {
                searchTerm = $"%{searchTerm}%";
            }

            await using var db = AppServices.GetService<SqliteConnection>();
            await db.OpenAsync();
            return await db.QueryAsync<Track>($"SELECT * FROM Track WHERE FileName LIKE @searchTerm or TrackName Like @searchTerm ORDER BY FileName", new { searchTerm });
        }
    }
}