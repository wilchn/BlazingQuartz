﻿CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    migration_id character varying(150) NOT NULL,
    product_version character varying(32) NOT NULL,
    CONSTRAINT pk___ef_migrations_history PRIMARY KEY (migration_id)
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20220903132027_InitialSchema') THEN
    CREATE TABLE bqz_execution_logs (
        log_id bigint GENERATED BY DEFAULT AS IDENTITY,
        run_instance_id character varying(256) NULL,
        log_type varchar(20) NOT NULL,
        job_name character varying(256) NULL,
        job_group character varying(256) NULL,
        trigger_name character varying(256) NULL,
        trigger_group character varying(256) NULL,
        schedule_fire_time_utc timestamp with time zone NULL,
        fire_time_utc timestamp with time zone NULL,
        job_run_time interval NULL,
        retry_count integer NULL,
        result character varying(8000) NULL,
        error_message character varying(8000) NULL,
        is_vetoed boolean NULL,
        is_exception boolean NULL,
        date_added_utc timestamp with time zone NOT NULL,
        CONSTRAINT pk_bqz_execution_logs PRIMARY KEY (log_id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20220903132027_InitialSchema') THEN
    CREATE TABLE bqz_execution_log_details (
        log_id bigint NOT NULL,
        execution_details text NULL,
        error_stack_trace text NULL,
        error_code integer NULL,
        error_help_link character varying(1000) NULL,
        CONSTRAINT pk_bqz_execution_log_details PRIMARY KEY (log_id),
        CONSTRAINT fk_bqz_execution_log_details_bqz_execution_logs_log_id FOREIGN KEY (log_id) REFERENCES bqz_execution_logs (log_id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20220903132027_InitialSchema') THEN
    CREATE INDEX ix_bqz_execution_logs_date_added_utc_log_type ON bqz_execution_logs (date_added_utc, log_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20220903132027_InitialSchema') THEN
    CREATE UNIQUE INDEX ix_bqz_execution_logs_run_instance_id ON bqz_execution_logs (run_instance_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20220903132027_InitialSchema') THEN
    CREATE INDEX ix_bqz_execution_logs_trigger_name_trigger_group_job_name_job_ ON bqz_execution_logs (trigger_name, trigger_group, job_name, job_group, date_added_utc);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "migration_id" = '20220903132027_InitialSchema') THEN
    INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
    VALUES ('20220903132027_InitialSchema', '6.0.8');
    END IF;
END $EF$;
COMMIT;

