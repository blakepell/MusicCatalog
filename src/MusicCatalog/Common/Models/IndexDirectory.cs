/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using Argus.ComponentModel;
using System.IO;

namespace MusicCatalog.Common.Models
{
    public class IndexDirectory : Observable
    {
        public IndexDirectory()
        {

        }

        public IndexDirectory(string directoryPath)
        {
            this.DirectoryPath = directoryPath;
        }

        private string _directoryPath;

        public string DirectoryPath
        {
            get => _directoryPath;
            set => this.Set(ref _directoryPath, value);
        }

        private bool _indexChildDirectories = true;

        public bool IndexChildDirectories
        {
            get => _indexChildDirectories;
            set => this.Set(ref _indexChildDirectories, value);
        }

        public SearchOption ToSearchOption()
        {
            return _indexChildDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        }

    }
}