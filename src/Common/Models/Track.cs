/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Dapper.Contrib.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
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

                if (cs == default(IPicture) || cs.Type == PictureType.NotAPicture)
                {
                    return App.DefaultAlbumArt;
                }

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
        }
    }
}