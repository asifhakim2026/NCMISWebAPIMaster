-- Run manually against the NCMIS database (no EF migration).
-- Creates dbo.ErrorLogs for Error / Warning / Information persistence
-- (API ErrorLog model / ErrorLogHelper, aligned with NCMIS MVC).
-- FileName column stores SourceFile (caller path or export context).

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = N'ErrorLogs' AND schema_id = SCHEMA_ID(N'dbo'))
BEGIN
    CREATE TABLE dbo.ErrorLogs
    (
        ErrorLogId         INT            NOT NULL IDENTITY(1,1) CONSTRAINT PK_ErrorLogs PRIMARY KEY,
        [Type]             NVARCHAR(50)   NOT NULL,          -- Error | Warning | Information
        ControllerName     NVARCHAR(200)  NULL,
        ClassName          NVARCHAR(200)  NULL,
        MethodName         NVARCHAR(200)  NULL,
        ErrorDescription   NVARCHAR(MAX)  NOT NULL,
        LineNumber         INT            NULL,
        CreatedAt          DATETIME2(7)   NOT NULL CONSTRAINT DF_ErrorLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UserName           NVARCHAR(300)  NULL,
        ModuleName         NVARCHAR(200)  NULL,
        ExceptionType      NVARCHAR(500)  NULL,
        StackTrace         NVARCHAR(4000) NULL,
        InnerException     NVARCHAR(2000) NULL,
        FileName           NVARCHAR(500)  NULL,            -- SourceFile
        RequestPath        NVARCHAR(1000) NULL,
        MachineName        NVARCHAR(200)  NULL,
        AdditionalData     NVARCHAR(MAX)  NULL               -- optional JSON (filters / template / notes)
    );

    CREATE INDEX IX_ErrorLogs_CreatedAt ON dbo.ErrorLogs (CreatedAt DESC);
    CREATE INDEX IX_ErrorLogs_Type_Module ON dbo.ErrorLogs ([Type], ModuleName);
END
GO
