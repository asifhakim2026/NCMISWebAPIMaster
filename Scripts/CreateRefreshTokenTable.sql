-- Run manually against the NCMIS database (no EF migration).
-- Creates dbo.RefreshTokens for JWT refresh-token session storage
-- (API RefreshToken model / NcmisDbContext mapping).

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'RefreshTokens' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.RefreshTokens
    (
        Id                   INT            NOT NULL IDENTITY(1,1) CONSTRAINT PK_RefreshTokens PRIMARY KEY,
        UserId               INT            NOT NULL,
        TokenHash            NVARCHAR(128)  NOT NULL,
        ExpiresAt            DATETIME2(7)   NOT NULL,
        CreatedAt            DATETIME2(7)   NOT NULL,
        RevokedAt            DATETIME2(7)   NULL,
        ReplacedByTokenHash  NVARCHAR(128)  NULL,
        DeviceInfo           NVARCHAR(200)  NULL
    );

    CREATE UNIQUE INDEX IX_RefreshTokens_TokenHash ON dbo.RefreshTokens (TokenHash);
    CREATE INDEX IX_RefreshTokens_UserId ON dbo.RefreshTokens (UserId);
END
GO
