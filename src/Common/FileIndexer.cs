using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Argus.Extensions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using MusicCatalog.Common.Models;

namespace MusicCatalog.Common
{
    public class FileIndexer
    {

        /// <summary>
        /// Rebuilds the index by clearing the table and reloading it in the fastest manner
        /// possible.  This function will not create the MD5 hashes or extract metadata, that
        /// will be done in separate steps.  This way the index can load 50,000 songs in 3-4
        /// seconds and start to be used while the rest of the data is filled in.
        /// </summary>
        public async Task RebuildIndex()
        {
            await Task.Run(async () =>
            {
                var settings = AppServices.GetService<AppSettings>();

                using (var conn = AppServices.GetService<SqliteConnection>())
                {
                    await conn.OpenAsync();

                    await conn.ExecuteAsync("DELETE FROM Track");
                    await conn.ExecuteAsync("VACUUM");
                    await conn.ExecuteAsync("BEGIN");

                    var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {  ".mp3", ".wav" };

                    foreach (var dir in settings.MusicDirectoryList)
                    {
                        if (!Directory.Exists(dir.DirectoryPath))
                        {
                            continue;
                        }

                        var root = new DirectoryInfo(dir.DirectoryPath);
                        var enumerable = new FileSystemEnumerable(root, "*", dir.ToSearchOption()).Where(x => extensions.Contains(Path.GetExtension(x.FullName)));

                        // var fileCount = enumerable.Count();

                        foreach (var fs in enumerable)
                        {
                            // Skip directories and junctions.
                            if (fs.Attributes.HasFlag(FileAttributes.Directory) || fs.Attributes.HasFlag(FileAttributes.ReparsePoint))
                            {
                                continue;
                            }

                            var fi = fs as FileInfo;

                            if (fi == null)
                            {
                                continue;
                            }

                            var tr = new Track
                            {
                                FilePath = fi.FullName,
                                FileName = fi.Name,
                                DirectoryPath = fi.DirectoryName,
                                Extension = fi.Extension.ToLower(),
                                FileSize = fi.Length,
                                DateCreated = fi.CreationTime,
                                DateModified = fi.LastWriteTime,
                                DateLastIndexed = DateTime.Now,
                                TagsProcessed = false
                            };

                            _ = await conn.InsertAsync(tr);
                        }
                    }

                    await conn.ExecuteAsync("COMMIT");
                }

            });
        }

        public async Task GenerateMd5Hashes()
        {
            await Task.Run(async () =>
            {
                using (var conn = AppServices.GetService<SqliteConnection>())
                {
                    await conn.OpenAsync();

                    var results = conn.Query<string>("select FilePath from Track");

                    await conn.ExecuteAsync("BEGIN");

                    foreach (var filePath in results)
                    {
                        var fi = new FileInfo(filePath);
                        string md5 = fi.CreateMD5();
                        await conn.ExecuteAsync("update Track set MD5 = @md5 where FilePath = @filePath", new {md5, filePath});
                    }

                    await conn.ExecuteAsync("COMMIT");
                }
            });
        }

    }
}