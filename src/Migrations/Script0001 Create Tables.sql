/*
 * A list of known albums.
 */
CREATE TABLE Album (
    Id               INTEGER NOT NULL CONSTRAINT PK_Album PRIMARY KEY AUTOINCREMENT,
    Name             TEXT,
    ReleaseDate      TEXT,
    ReleaseYear      INTEGER,
    AlbumArtFilePath TEXT
);

/*
 * A list of known artists.
 */
CREATE TABLE Artist (
    Id               INTEGER NOT NULL CONSTRAINT PK_Artist PRIMARY KEY AUTOINCREMENT,
    Name             TEXT
);

/*
 * Because the index can be totally rebuilt, the an autonumber Id could be different
 * which will mess playlists up.
 */
CREATE TABLE Track (
    Id               INTEGER NOT NULL CONSTRAINT PK_Track PRIMARY KEY AUTOINCREMENT,
    ArtistId         INTEGER,
    AlbumId          INTEGER,
    FileName         TEXT,
    Extension        TEXT,
    FilePath         TEXT,
    DirectoryPath    TEXT,
    MD5              TEXT,
    FileSize         INTEGER NOT NULL,
    TrackName        TEXT,
    Favorite         INTEGER,
    PlayCount        INTEGER,
    DateCreated      TEXT    NOT NULL,
    DateModified     TEXT    NOT NULL,
    DateLastIndexed  TEXT    NOT NULL,
    DateLastPlayed   TEXT,
    TagsProcessed    INTEGER
);

/*
 * The list of playlists.  The UI will then allow this to be exported into various
 * common formats if using with external players.
 */
CREATE TABLE Playlist (
    Id               INTEGER NOT NULL CONSTRAINT PK_Playlist PRIMARY KEY AUTOINCREMENT,
    Name             TEXT,
    DateLastPlayed   TEXT    NOT NULL,
    DateCreated      TEXT    NOT NULL,
    DateModified     TEXT    NOT NULL
);

/*
 * So playlist entries can survive an index rebuild we have to use the FilePath instead
 * of the Id of the file (which is an autoincrement and subject to changing on a reload).
 */
CREATE TABLE PlaylistTrack (
    PlaylistId       INTEGER,
    FilePath         TEXT
);