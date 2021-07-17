/*
 * Music Catalog
 *
 * @project lead      : Blake Pell
 * @website           : http://www.blakepell.com
 * @copyright         : Copyright (c), 2018-2021 All rights reserved.
 * @license           : MIT
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MusicCatalog.Common
{
    /// <summary>
    /// Searches the file system but also handles common exceptions that cause the .NET
    /// provided enumerables to fail in cases such as accessing junctions, symbolic links, etc.
    /// </summary>
    public class FileSystemEnumerable : IEnumerable<FileSystemInfo>
    {
        private readonly DirectoryInfo _root;
        private readonly IList<string> _patterns;
        private readonly SearchOption _option;

        public FileSystemEnumerable(DirectoryInfo root, string pattern, SearchOption option)
        {
            _root = root;
            _patterns = new List<string> { pattern };
            _option = option;
        }

        public FileSystemEnumerable(DirectoryInfo root, IList<string> patterns, SearchOption option)
        {
            _root = root;
            _patterns = patterns;
            _option = option;
        }

        public IEnumerator<FileSystemInfo> GetEnumerator()
        {
            if (_root == null || !_root.Exists)
            {
                yield break;
            }

            IEnumerable<FileSystemInfo> matches = new List<FileSystemInfo>();
            
            try
            {
                foreach (var pattern in _patterns)
                {
                    matches = matches.Concat(_root.EnumerateDirectories(pattern, SearchOption.TopDirectoryOnly))
                                     .Concat(_root.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly));
                }
            }
            catch (UnauthorizedAccessException)
            {
                yield break;
            }
            catch (PathTooLongException)
            {
                yield break;
            }
            catch (IOException)
            {
                // "The symbolic link cannot be followed because its type is disabled."
                // "The specified network name is no longer available."
                yield break;
            }

            foreach (var file in matches)
            {
                yield return file;
            }

            if (_option == SearchOption.AllDirectories)
            {
                foreach (var dir in _root.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
                {
                    var fileSystemInfos = new FileSystemEnumerable(dir, _patterns, _option);

                    foreach (var match in fileSystemInfos)
                    {
                        yield return match;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
