using Argus.Extensions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using MusicCatalog.Common.Extensions;
using MusicCatalog.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using File = System.IO.File;

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

                    // Clear the Track table, request its auto increment and start the transaction.
                    await conn.ExecuteAsync("DELETE FROM Track");
                    await conn.ExecuteAsync("DELETE FROM sqlite_sequence WHERE name = 'Track'");
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

                    conveyor.UpdateInfoOverlay("Loading", "Commiting records to the database.", true, true);
                    await conn.ExecuteAsync("COMMIT");

                    conveyor.HideInfoOverlay();
                }
            });
        }

        /// <summary>
        /// Attempts to index tags associated with the MP3.
        /// </summary>
        /// <returns></returns>
        public async Task IndexTags()
        {
            await Task.Run(async () =>
            {
                var settings = AppServices.GetService<AppSettings>();
                var conveyor = AppServices.CreateInstance<Conveyor>();
                var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".mp3" };
                int counter = 0;

                using (var conn = AppServices.GetService<SqliteConnection>())
                {
                    await conn.OpenAsync();
                    await conn.ExecuteAsync("BEGIN");

                    conveyor.UpdateInfoOverlay("Loading", "Getting Track List", true, true);
                    var tracks = conn.Query<Track>("SELECT * FROM Track");

                    foreach (var tr in tracks)
                    {
                        counter++;
                        conveyor.UpdateInfoOverlay("Loading", $"{counter.ToString()}: {tr.FileName}", true, true);

                        if (!File.Exists(tr.FilePath))
                        {
                            continue;
                        }

                        TagLib.File tags;

                        try
                        {
                            tags = TagLib.File.Create(tr.FilePath);
                        }
                        catch
                        {
                            // Files with corrupt tags will likely still be playable but for our purposes here
                            // we're going to eat the exception and continue.
                            continue;
                        }

                        if (tags != null)
                        {
                            // Title
                            tr.Title = !string.IsNullOrWhiteSpace(tags.Tag.Title) ? tags.Tag.Title : tr.TrackNameFromFileName();
                            tr.ArtistName = !string.IsNullOrWhiteSpace(tags.Tag.JoinedAlbumArtists) ? tags.Tag.JoinedAlbumArtists: tr.ArtistFromFileName();
                            tr.AlbumName = !string.IsNullOrWhiteSpace(tags.Tag.Album) ? tags.Tag.Album : string.Empty;
                            tr.AudioBitRate = tags.Properties.AudioBitrate;
                            tr.AudioSampleRate = tags.Properties.AudioSampleRate;
                            tr.BitsPerSample = tags.Properties.BitsPerSample;
                            tr.AudioChannels = tags.Properties.AudioChannels;
                            tr.Duration = tags.Properties.Duration.ToString();
                        }

                        tr.TagsProcessed = true;
                        tr.DateLastIndexed = DateTime.Now;
                        
                        _ = await conn.UpdateAsync(tr);
                    }

                    conveyor.UpdateInfoOverlay("Loading", "Commiting records to the database.", true, true);
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

                    var results = conn.Query<string>("SELECT FilePath FROM Track");

                    await conn.ExecuteAsync("BEGIN");

                    foreach (var filePath in results)
                    {
                        var fi = new FileInfo(filePath);
                        string md5 = fi.CreateMD5();
                        await conn.ExecuteAsync("update Track set MD5 = @md5 where FilePath = @filePath", new { md5, filePath });
                    }

                    await conn.ExecuteAsync("COMMIT");
                }
            });
        }

    }
}