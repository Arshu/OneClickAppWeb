
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



INSERT INTO SYD_User (UserID, UserGUID, UserAlias, UserHash, IsAdmin) VALUES (1, '254c3b0b-d09e-4974-9ab1-933cdc4795f0', 'Admin', 'Admin', 1);
INSERT INTO SYD_User (UserID, UserGUID, UserAlias, UserHash, IsAdmin) VALUES (2, 'c5331555-c2c4-4593-a8f2-0aa362ac8554', 'test 1', 'zNuR8dd8/p6Sm7w1pIpim6jIRHZWmArPO1JCuCXsFdG9SfqDlFtGZDE6nmOkdk7tq0AUL6vKflIhmbZjALC1Ig==', 0);
INSERT INTO SYD_User (UserID, UserGUID, UserAlias, UserHash, IsAdmin) VALUES (3, '6348b25d-8a99-4f3b-960c-fdc1ffd5a476', 'sri', '7dWOX8ZjR5zHrjek/TaAw0CL3sxUB3CGUdQ1/DXM9ObN/XsA6/mNTZCDu78HXCOCtwAV4Be8BFLIHKQjy3GA9A==', 0);

