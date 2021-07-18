/*
 * Provides the abililty to see duplicate files as per their MD5 hash.  Helps in the case
 * where you might have two files that are identical but with different names.
 */
CREATE VIEW DuplicateFiles AS
    SELECT t1.Id,
           t1.ArtistId,
           t1.AlbumId,
           t1.FileName,
           t1.Extension,
           t1.FilePath,
           t1.DirectoryPath,
           t1.MD5,
           t1.FileSize,
           t1.TrackName,
           t1.DateCreated,
           t1.DateModified,
           t1.DateLastIndexed,
           t1.DateLastPlayed,
           t1.TagsProcessed
      FROM Track t1
           INNER JOIN
           (
               SELECT MD5
                 FROM Track
                GROUP BY MD5
               HAVING COUNT(MD5) > 1
           )
           t2 ON t1.MD5 = t2.MD5
     ORDER BY t1.MD5;