IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220827074935_InitialSchema')
BEGIN
    CREATE TABLE [bqz_ExecutionLogs] (
        [LogId] bigint NOT NULL IDENTITY,
        [RunInstanceId] nvarchar(256) NULL,
        [LogType] varchar(20) NOT NULL,
        [JobName] nvarchar(256) NULL,
        [JobGroup] nvarchar(256) NULL,
        [TriggerName] nvarchar(256) NULL,
        [TriggerGroup] nvarchar(256) NULL,
        [ScheduleFireTimeUtc] datetimeoffset NULL,
        [FireTimeUtc] datetimeoffset NULL,
        [JobRunTime] time NULL,
        [RetryCount] int NULL,
        [Result] nvarchar(max) NULL,
        [ErrorMessage] nvarchar(max) NULL,
        [IsVetoed] bit NULL,
        [IsException] bit NULL,
        [DateAddedUtc] datetimeoffset NOT NULL,
        CONSTRAINT [PK_bqz_ExecutionLogs] PRIMARY KEY ([LogId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220827074935_InitialSchema')
BEGIN
    CREATE TABLE [bqz_ExecutionLogDetails] (
        [LogId] bigint NOT NULL,
        [ExecutionDetails] nvarchar(max) NULL,
        [ErrorStackTrace] nvarchar(max) NULL,
        [ErrorCode] int NULL,
        [ErrorHelpLink] nvarchar(1000) NULL,
        CONSTRAINT [PK_bqz_ExecutionLogDetails] PRIMARY KEY ([LogId]),
        CONSTRAINT [FK_bqz_ExecutionLogDetails_bqz_ExecutionLogs_LogId] FOREIGN KEY ([LogId]) REFERENCES [bqz_ExecutionLogs] ([LogId]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220827074935_InitialSchema')
BEGIN
    CREATE INDEX [IX_bqz_ExecutionLogs_DateAddedUtc_LogType] ON [bqz_ExecutionLogs] ([DateAddedUtc], [LogType]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220827074935_InitialSchema')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_bqz_ExecutionLogs_RunInstanceId] ON [bqz_ExecutionLogs] ([RunInstanceId]) WHERE [RunInstanceId] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220827074935_InitialSchema')
BEGIN
    CREATE INDEX [IX_bqz_ExecutionLogs_TriggerName_TriggerGroup_JobName_JobGroup_DateAddedUtc] ON [bqz_ExecutionLogs] ([TriggerName], [TriggerGroup], [JobName], [JobGroup], [DateAddedUtc]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220827074935_InitialSchema')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20220827074935_InitialSchema', N'6.0.8');
END;
GO

COMMIT;
GO

