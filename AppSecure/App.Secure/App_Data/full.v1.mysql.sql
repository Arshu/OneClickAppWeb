
DROP TABLE IF EXISTS `SYS_Version` ;
CREATE TABLE `SYS_Version` (
	`VersionNo`		FLOAT NOT NULL,
	`VersionNoGUID`		CHAR(36) NOT NULL,
	`VersionName`		VARCHAR(50) NOT NULL  ,
    PRIMARY KEY (`VersionNo`)

)
ENGINE = INNODB;
INSERT INTO `SYS_Version` (`VersionNo`, `VersionNoGUID`, `VersionName`) VALUES (1, '45dfe6ba-ab12-4445-bf49-cf3e356cf3dd', 'Rev 1');

-- Disable foreign key checks
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;


DROP TABLE IF EXISTS `SYD_User` ;
CREATE TABLE `SYD_User` (
	`UserID`		BIGINT(19)   AUTO_INCREMENT NOT NULL,
	`UserGUID`		CHAR(36) NOT NULL,
	`UserAlias`		VARCHAR(25) NOT NULL  ,
	`UserHash`		VARCHAR(250)  ,
	`IsAdmin`		TINYINT NOT NULL DEFAULT 0
,
 PRIMARY KEY (`UserID`),
UNIQUE KEY `SYD_User_GK_SYD_User`(`UserID`, `UserGUID`),
UNIQUE KEY `SYD_User_UK_SYD_User`(`UserAlias`))
ENGINE = INNODB;


INSERT INTO `SYD_User` (`UserID`, `UserGUID`, `UserAlias`, `UserHash`, `IsAdmin`) VALUES (1, '254c3b0b-d09e-4974-9ab1-933cdc4795f0', 'Admin', 'Admin', 1);
INSERT INTO `SYD_User` (`UserID`, `UserGUID`, `UserAlias`, `UserHash`, `IsAdmin`) VALUES (2, 'c5331555-c2c4-4593-a8f2-0aa362ac8554', 'test 1', 'zNuR8dd8/p6Sm7w1pIpim6jIRHZWmArPO1JCuCXsFdG9SfqDlFtGZDE6nmOkdk7tq0AUL6vKflIhmbZjALC1Ig==', 0);
INSERT INTO `SYD_User` (`UserID`, `UserGUID`, `UserAlias`, `UserHash`, `IsAdmin`) VALUES (3, '6348b25d-8a99-4f3b-960c-fdc1ffd5a476', 'sri', '7dWOX8ZjR5zHrjek/TaAw0CL3sxUB3CGUdQ1/DXM9ObN/XsA6/mNTZCDu78HXCOCtwAV4Be8BFLIHKQjy3GA9A==', 0);


-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;