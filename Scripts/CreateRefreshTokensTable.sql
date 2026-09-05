-- Run this against the ncmis database once
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UserId INT NOT NULL,
        TokenHash NVARCHAR(128) NOT NULL,
        ExpiresAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL,
        RevokedAt DATETIME2 NULL,
        ReplacedByTokenHash NVARCHAR(128) NULL,
        DeviceInfo NVARCHAR(200) NULL
    );

    CREATE UNIQUE INDEX IX_RefreshTokens_TokenHash ON RefreshTokens(TokenHash);
    CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
END
GO
