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
    [Table("Album")]
    public class Album
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime ReleaseDate { get; set; }

        public int ReleaseYear { get; set; }

        public string AlbumArtFilePath { get; set; }
    }
}