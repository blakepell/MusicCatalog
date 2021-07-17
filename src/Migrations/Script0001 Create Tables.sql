CREATE TABLE Album (
    Id               INTEGER NOT NULL CONSTRAINT PK_Album PRIMARY KEY AUTOINCREMENT,
    Name             TEXT,
    ReleaseDate      TEXT,
    ReleaseYear      INTEGER,
    AlbumArtFilePath TEXT
);

CREATE TABLE Artist (
    Id               INTEGER NOT NULL CONSTRAINT PK_Artist PRIMARY KEY AUTOINCREMENT,
    Name             TEXT
);

CREATE TABLE IndexDirectory(
    Id                    INTEGER NOT NULL CONSTRAINT PK_Folders PRIMARY KEY AUTOINCREMENT,
    DirectoryPath         TEXT,
    IndexChildDirectories INTEGER
);

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
    DateCreated      TEXT    NOT NULL,
    DateModified     TEXT    NOT NULL,
    DateLastIndexed  TEXT    NOT NULL,
    DateLastPlayed   TEXT,
    TagsProcessed    INTEGER
);

CREATE TABLE Playlist (
    Id               INTEGER NOT NULL CONSTRAINT PK_Playlist PRIMARY KEY AUTOINCREMENT,
    Name             TEXT,
    DateLastPlayed   TEXT    NOT NULL,
    DateCreated      TEXT    NOT NULL,
    DateModified     TEXT    NOT NULL
);

CREATE TABLE PlaylistTrack (
    PlaylistId       INTEGER,
    TrackId          INTEGER
);
