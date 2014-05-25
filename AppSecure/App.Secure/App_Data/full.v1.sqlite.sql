
DROP TABLE IF EXISTS [SYS_Version];
CREATE TABLE [SYS_Version] (
	"VersionNo"		float NOT NULL,
	"VersionNoGUID"		guid NOT NULL,
	"VersionName"		varchar(50) NOT NULL COLLATE NOCASE,
    PRIMARY KEY ([VersionNo])

);
INSERT INTO SYS_Version (VersionNo, VersionNoGUID, VersionName) VALUES (1, '45dfe6ba-ab12-4445-bf49-cf3e356cf3dd', 'Rev 1');


DROP TABLE IF EXISTS [SYD_User];
CREATE TABLE [SYD_User] (
	"UserID"		integer PRIMARY KEY AUTOINCREMENT NOT NULL,
	"UserGUID"		guid NOT NULL,
	"UserAlias"		varchar(25) NOT NULL COLLATE NOCASE,
	"UserHash"		varchar(250) COLLATE NOCASE,
	"IsAdmin"		bit NOT NULL DEFAULT 0

);

CREATE UNIQUE INDEX [SYD_User_GK_SYD_User]
ON [SYD_User]
([UserID], [UserGUID]);

CREATE UNIQUE INDEX [SYD_User_UK_SYD_User]
ON [SYD_User]
([UserAlias]);




