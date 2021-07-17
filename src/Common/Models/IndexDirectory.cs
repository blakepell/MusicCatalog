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
    [Table("IndexDirectory")]
    public class IndexDirectory
    {
        public int Id { get; set; }

        public string DirectoryPath { get; set; }

        public bool IndexChildDirectories { get; set; }
    }
}