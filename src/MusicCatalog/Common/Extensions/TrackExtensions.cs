/*
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
using System.Windows.Media.Imaging;
using MusicCatalog.Common.Models;
using TagLib;
using File = TagLib.File;

namespace MusicCatalog.Common.Extensions
{
    public static class TrackExtensions
    {
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