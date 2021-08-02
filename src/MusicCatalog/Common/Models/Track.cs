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
        [Key]
        public int Id { get; set; }

        public int ArtistId { get; set; }

        public int AlbumId { get; set; }

        public string FileName { get; set; }

        public string Extension { get; set; }

        public string FilePath { get; set; }

        public string DirectoryPath { get; set; }

        public string MD5 { get; set; }

        public long FileSize { get; set; }

        public string Title { get; set; }

        public string ArtistName { get; set; }

        public string AlbumName { get; set; }

        public string Duration { get; set; }

        public int AudioBitRate { get; set; }

        public int AudioSampleRate { get; set; }

        public int BitsPerSample { get; set; }

        public int AudioChannels { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }

        public DateTime DateLastIndexed { get; set; }

        public bool TagsProcessed { get; set; }

    }
}