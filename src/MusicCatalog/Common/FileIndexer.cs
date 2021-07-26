using Argus.Extensions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using MusicCatalog.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
                var conveyor = AppServices.CreateInstance<Conveyor>();
                int counter = 0;

                using (var conn = AppServices.GetService<SqliteConnection>())
                {
                    await conn.OpenAsync();

                    conveyor.UpdateInfoOverlay("Loading", "Deleting Index", true, true);

                    // Save user provided data specific to a track.
                    await conn.ExecuteAsync("DELETE FROM TrackUpdate");
                    await conn.ExecuteAsync("INSERT INTO TrackUpdate(FilePath, DateLastPlayed, Favorite, PlayCount) SELECT FilePath, DateLastPlayed, Favorite, PlayCount FROM Track");

                    // Clear the Track table and begin.
                    await conn.ExecuteAsync("DELETE FROM Track");
                    await conn.ExecuteAsync("VACUUM");
                    await conn.ExecuteAsync("BEGIN");

                    var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                    {  ".mp3" };

                    foreach (var dir in settings.MusicDirectoryList)
                    {
                        counter++;
                        conveyor.UpdateInfoOverlay("Loading", $"Directory {counter} of {settings.MusicDirectoryList.Count}", true, true);

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

                    // Reload our user supplied data back into the Track table.
                    conveyor.UpdateInfoOverlay("Loading", "Reloading Track Metadata", true, true);
                    await conn.ExecuteAsync("UPDATE Track SET DateLastPlayed = t.DateLastPlayed, Favorite = t.Favorite, PlayCount = t.PlayCount FROM (SELECT FilePath, DateLastPlayed, Favorite, PlayCount FROM TrackUpdate) t WHERE Track.FilePath = t.FilePath");

                    conveyor.UpdateInfoOverlay("Loading", $"Commiting records to the database.", true, true);
                    await conn.ExecuteAsync("COMMIT");

                    conveyor.HideInfoOverlay();
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