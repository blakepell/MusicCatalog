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

        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }

        public DateTime DateLastIndexed { get; set; }

        public DateTime DateLastPlayed { get; set; }

        public bool TagsProcessed { get; set; }
    }
}
