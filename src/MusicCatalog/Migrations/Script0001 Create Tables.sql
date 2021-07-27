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
    Title            TEXT,
    ArtistName       TEXT,
    AlbumName        TEXT,
    Duration         TEXT,
    AudioBitrate     INTEGER,
    AudioSampleRate  INTEGER,
    BitsPerSample    INTEGER,
    AudioChannels    INTEGER,
    TagsProcessed    INTEGER,
    DateCreated      TEXT    NOT NULL,
    DateModified     TEXT    NOT NULL,
    DateLastIndexed  TEXT    NOT NULL
);

/*
 * User provided track data that should be persisted between full index loads.  This data
 * will have to be linked by file name since the Id of the main table can change (or it
 * can be linked by ID if an update statement is run after a full rebuild to update the keys.
 */
CREATE TABLE TrackEx (
    FilePath         TEXT NOT NULL CONSTRAINT PK_TrackEx PRIMARY KEY,
    Favorite         INTEGER,
    PlayCount        INTEGER,
    DateLastPlayed   TEXT
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