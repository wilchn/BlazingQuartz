IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [migration_id] nvarchar(150) NOT NULL,
        [product_version] nvarchar(32) NOT NULL,
        CONSTRAINT [pk___ef_migrations_history] PRIMARY KEY ([migration_id])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [migration_id] = N'20220912031814_InitialSchema')
BEGIN
    CREATE TABLE [bqz_execution_logs] (
        [log_id] bigint NOT NULL IDENTITY,
        [run_instance_id] nvarchar(256) NULL,
        [log_type] varchar(20) NOT NULL,
        [job_name] nvarchar(256) NULL,
        [job_group] nvarchar(256) NULL,
        [trigger_name] nvarchar(256) NULL,
        [trigger_group] nvarchar(256) NULL,
        [schedule_fire_time_utc] datetimeoffset NULL,
        [fire_time_utc] datetimeoffset NULL,
        [job_run_time] time NULL,
        [retry_count] int NULL,
        [result] nvarchar(max) NULL,
        [error_message] nvarchar(max) NULL,
        [is_vetoed] bit NULL,
        [is_exception] bit NULL,
        [is_success] bit NULL,
        [return_code] nvarchar(28) NULL,
        [date_added_utc] datetimeoffset NOT NULL,
        CONSTRAINT [pk_bqz_execution_logs] PRIMARY KEY ([log_id])
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [migration_id] = N'20220912031814_InitialSchema')
BEGIN
    CREATE TABLE [bqz_execution_log_details] (
        [log_id] bigint NOT NULL,
        [execution_details] nvarchar(max) NULL,
        [error_stack_trace] nvarchar(max) NULL,
        [error_code] int NULL,
        [error_help_link] nvarchar(1000) NULL,
        CONSTRAINT [pk_bqz_execution_log_details] PRIMARY KEY ([log_id]),
        CONSTRAINT [fk_bqz_execution_log_details_bqz_execution_logs_log_id] FOREIGN KEY ([log_id]) REFERENCES [bqz_execution_logs] ([log_id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [migration_id] = N'20220912031814_InitialSchema')
BEGIN
    CREATE INDEX [ix_bqz_execution_logs_date_added_utc_log_type] ON [bqz_execution_logs] ([date_added_utc], [log_type]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [migration_id] = N'20220912031814_InitialSchema')
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [ix_bqz_execution_logs_run_instance_id] ON [bqz_execution_logs] ([run_instance_id]) WHERE [run_instance_id] IS NOT NULL');
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [migration_id] = N'20220912031814_InitialSchema')
BEGIN
    CREATE INDEX [ix_bqz_execution_logs_trigger_name_trigger_group_job_name_job_group_date_added_utc] ON [bqz_execution_logs] ([trigger_name], [trigger_group], [job_name], [job_group], [date_added_utc]);
END;
GO

IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [migration_id] = N'20220912031814_InitialSchema')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([migration_id], [product_version])
    VALUES (N'20220912031814_InitialSchema', N'6.0.8');
END;
GO

COMMIT;
GO

