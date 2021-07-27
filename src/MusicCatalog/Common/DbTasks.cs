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
using Dapper.Contrib.Extensions;

namespace MusicCatalog.Common
{
    public static class DbTasks
    {
        /// <summary>
        /// Updates the last played timestamp for a specified file.
        /// </summary>
        /// <param name="fullPath">The full path to the file that should be updated.</param>
        public static async Task<int> UpdateLastPlayed(string fullPath)
        {
            await using var db = AppServices.GetService<SqliteConnection>();
            await db.OpenAsync();

            var trackEx = await db.QuerySingleOrDefaultAsync<TrackEx>("SELECT * FROM TrackEx WHERE FilePath = @fullPath", new { fullPath });

            if (trackEx == null)
            {
                trackEx = new TrackEx {FilePath = fullPath, DateLastPlayed = DateTime.Now, Favorite = false};
                trackEx.PlayCount++;
                return await db.InsertAsync(trackEx);
            }

            // TODO: Clean this up, inconsistent return
            trackEx.PlayCount++;
            trackEx.DateLastPlayed = DateTime.Now; 
            bool result = await db.UpdateAsync(trackEx);
            return result == true ? 1 : 0;
        }

        /// <summary>
        /// Returns the most recently played songs.
        /// </summary>
        /// <param name="count">The number of tracks to return.</param>
        public static async Task<IEnumerable<TrackIndex>> RecentPlays(int count)
        {
            count = Math.Clamp(1, count, 100);
            await using var db = AppServices.GetService<SqliteConnection>();
            await db.OpenAsync();
            return await db.QueryAsync<TrackIndex>($"SELECT * FROM TrackIndex Where PlayCount > 0 ORDER BY DateLastPlayed DESC LIMIT {count}");
        }

        /// <summary>
        /// Searches the tracks by keyword.
        /// </summary>
        /// <param name="searchTerm"></param>
        public static async Task<IEnumerable<TrackIndex>> SearchTracks(string searchTerm)
        {
            searchTerm = searchTerm.Replace('*', '%');

            if (!searchTerm.Contains('%'))
            {
                searchTerm = $"%{searchTerm}%";
            }

            await using var db = AppServices.GetService<SqliteConnection>();
            await db.OpenAsync();
            return await db.QueryAsync<TrackIndex>($"SELECT * FROM TrackIndex WHERE FileName LIKE @searchTerm or Title Like @searchTerm ORDER BY FileName", new { searchTerm });
        }
    }
}