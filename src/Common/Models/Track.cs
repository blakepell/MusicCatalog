/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using TagLib;

namespace MusicCatalog.Common.Models
{
    [Table("Track")]
    public class Track
    {
        public int Id { get; set; }

        public int ArtistId { get; set; }

        public int AlbumId { get; set; }

        public string FileName { get; set; }

        public string Extension { get; set; }

        public string FilePath { get; set; }

        public string DirectoryPath { get; set; }

        public string MD5 { get; set; }

        public long FileSize { get; set; }

        public string TrackName { get; set; }

        public bool Favorite { get; set; }

        public int PlayCount { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }

        public DateTime DateLastIndexed { get; set; }

        public DateTime DateLastPlayed { get; set; }

        public bool TagsProcessed { get; set; }

        [JsonIgnore]
        public string DisplayTrackName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.TrackName))
                {
                    return this.TrackName;
                }

                if (!string.IsNullOrWhiteSpace((Tags?.Tag?.Title)))
                {
                    return this.Tags.Tag.Title;
                }

                return this.FileName;
            }
        }

        private TagLib.File _tags;

        [Write(false)]
        [JsonIgnore]
        public TagLib.File Tags
        {
            get
            {
                if (_tags != null)
                {
                    return _tags;
                }

                _tags = TagLib.File.Create(FilePath);
                return _tags;
            }
        }

        [Write(false)]
        [JsonIgnore]
        public BitmapImage AlbumArt
        {
            get
            {
                var tags = TagLib.File.Create(FilePath);

                if (tags.Tag == null)
                {
                    return App.DefaultAlbumArt;
                }

                var cs = tags.Tag.Pictures.FirstOrDefault();

                if (cs == default(IPicture) 
                    || cs.Type == PictureType.NotAPicture
                    || cs.Type == PictureType.Other)
                {
                    return App.DefaultAlbumArt;
                }

                try
                {
                    using (var stream = new MemoryStream(cs.Data.Data))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        //bitmap.DecodePixelHeight = 256;
                        //bitmap.DecodePixelHeight = 256;
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();
                        return bitmap;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw;
                }
            }
        }
    }
}