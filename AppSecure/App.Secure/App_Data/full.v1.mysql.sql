
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




-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;