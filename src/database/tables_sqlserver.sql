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

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220819012955_InitialSchema')
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
        [ExceptionMessage] nvarchar(max) NULL,
        [ExecutionDetails] nvarchar(max) NULL,
        [IsVetoed] bit NULL,
        [IsException] bit NULL,
        [DateAddedUtc] datetimeoffset NOT NULL,
        CONSTRAINT [PK_bqz_ExecutionLogs] PRIMARY KEY ([LogId])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220819012955_InitialSchema')
BEGIN
    CREATE INDEX [IX_bqz_ExecutionLogs_DateAddedUtc_LogType] ON [bqz_ExecutionLogs] ([DateAddedUtc], [LogType]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220819012955_InitialSchema')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_bqz_ExecutionLogs_RunInstanceId] ON [bqz_ExecutionLogs] ([RunInstanceId]) WHERE [RunInstanceId] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220819012955_InitialSchema')
BEGIN
    CREATE INDEX [IX_bqz_ExecutionLogs_TriggerName_TriggerGroup_JobName_JobGroup_DateAddedUtc] ON [bqz_ExecutionLogs] ([TriggerName], [TriggerGroup], [JobName], [JobGroup], [DateAddedUtc]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20220819012955_InitialSchema')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20220819012955_InitialSchema', N'6.0.8');
END;
GO

COMMIT;
GO

