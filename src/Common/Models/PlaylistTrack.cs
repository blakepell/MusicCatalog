/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Dapper.Contrib.Extensions;

namespace MusicCatalog.Common.Models
{
    [Table("PlaylistTrack")]
    public class PlaylistTrack
    {
        public int PlaylistId { get; set; }

        public int PlaylistTrackId { get; set; }

    }
}
