﻿/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Dapper;
using Dapper.Contrib.Extensions;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Data.Sqlite;
using MusicCatalog.Common.Models;
using TagLib;
using File = TagLib.File;

namespace MusicCatalog.Common.Extensions
{
    public static class TrackExtensions
    {
        /// <summary>
        /// Inserts a <see cref="Track"/> if it's a new object otherwise an existing
        /// object will be updated.
        /// </summary>
        /// <param name="t"></param>
        public static async Task Upsert(this Track t)
        {
            await using var conn = AppServices.GetService<SqliteConnection>();
            await conn.OpenAsync();

            if (t.Id <= 0)
            {
                await conn.InsertAsync(t);
            }
            else
            {
                await conn.UpdateAsync(t);
            }

            await conn.CloseAsync();
        }

        /// <summary>
        /// Attempts to update the tags from the ID tags of the file or if those don't exist
        /// from the filename.
        /// </summary>
        /// <param name="t"></param>
        public static async Task UpdateTags(this Track t)
        {
            var tags = t.TagLib();

            var id1 = tags.GetTag(TagTypes.Id3v1);
            var id2 = tags.GetTag(TagTypes.Id3v2);

            // ID1 => ID2 => From File
            if (!string.IsNullOrWhiteSpace(id1?.Title))
            {
                t.TrackName = id1.Title;
            }
            else if (!string.IsNullOrWhiteSpace(id2?.Title))
            {
                t.TrackName = id2.Title;
            }
            else
            {
                t.TrackName = t.TrackNameFromFileName();
            }

            await using var conn = AppServices.GetService<SqliteConnection>();
            await conn.OpenAsync();

            t.TagsProcessed = true;

            if (t.Id <= 0)
            {
                await conn.InsertAsync(t);
            }
            else
            {
                await conn.UpdateAsync(t);
            }

            await conn.CloseAsync();
        }

        /// <summary>
        /// Acquires the TagLib record for the <see cref="Track"/>.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static File TagLib(this Track t)
        {
            return File.Create(t.FilePath);
        }

        /// <summary>
        /// Attempts to infer the artist's name from the filename.
        /// </summary>
        /// <param name="t"></param>
        public static string ArtistFromFileName(this Track t)
        {
            var parts = t.FileName.Split('-');

            if (parts.Length > 1)
            {
                return parts[0].Replace(t.Extension, "").Trim();
            }

            return "Unknown Artist";
        }

        /// <summary>
        /// Attempts to infer the track title from the filename.
        /// </summary>
        /// <param name="t"></param>
        public static string TrackNameFromFileName(this Track t)
        {
            var parts = t.FileName.Split('-');

            if (parts.Length > 1)
            {
                return parts[1].Replace(t.Extension, "").Trim();
            }

            return "Unknown Track";
        }

        public static BitmapImage GetAlbumArt(this Track t)
        {
            var tabLib = t.TagLib();

            // Nothing's there, return the static default image.
            if (tabLib?.Tag == null)
            {
                return App.DefaultAlbumArt;
            }

            // Try to the album art but weed out other pictures or things
            // that aren't pictures or might be corrupted.
            var cs = tabLib.Tag.Pictures.FirstOrDefault();

            if (cs == default(IPicture)
                || cs.Type == PictureType.NotAPicture
                || cs.Type == PictureType.Other)
            {
                return App.DefaultAlbumArt;
            }

            try
            {
                // Now we have something, put it into a frozen bitmap.  Consider
                // setting a DecodePixelHeight and DecodePixelWidth to lower memory
                // consumption.
                using (var stream = new MemoryStream(cs.Data.Data))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                // TODO: Log
                Console.WriteLine(ex);
            }

            // If we get here something went wrong.
            return App.DefaultAlbumArt;
        }
    }
}