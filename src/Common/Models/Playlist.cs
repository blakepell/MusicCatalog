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
    [Table("Playlist")]
    public class Playlist
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime DateLastPlayed { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime DateModified { get; set; }
    }
}
