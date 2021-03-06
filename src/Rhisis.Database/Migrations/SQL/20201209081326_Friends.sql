CREATE TABLE `Friends` (
    `CharacterId` int NOT NULL,
    `FriendId` int NOT NULL,
    `IsBlocked` BIT NOT NULL DEFAULT 0,
    CONSTRAINT `PK_Friends` PRIMARY KEY (`CharacterId`),
    CONSTRAINT `FK_Friends_Characters_CharacterId` FOREIGN KEY (`CharacterId`) REFERENCES `Characters` (`Id`),
    CONSTRAINT `FK_Friends_Characters_FriendId` FOREIGN KEY (`FriendId`) REFERENCES `Characters` (`Id`)
);

CREATE INDEX `IX_Friends_FriendId` ON `Friends` (`FriendId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20201209081326_Friends', '3.1.3');
