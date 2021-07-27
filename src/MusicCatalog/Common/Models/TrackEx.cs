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
    [Table("TrackEx")]
    public class TrackEx
    {
        [ExplicitKey]
        public string FilePath { get; set; }

        public bool Favorite { get; set; }

        public int PlayCount { get; set; }

        public DateTime DateLastPlayed { get; set; }
    }
}