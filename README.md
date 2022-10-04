# BlazingQuartz
BlazingQuartz is an easy to use [Blazor](https://blazor.net) web UI to manage [Quartz.NET](https://github.com/quartznet/quartznet) scheduler jobs.

BlazingQuartz is created with [ASP.NET Core Blazor Server](https://blazor.net) and use [MudBlazor](https://mudblazor.com) component library.

## Requirements
* .NET 6
* Quartz 3.5+

## Features
* Add, modify jobs and triggers
* Support Cron, Daily, Simple trigger
* Pause, resume, clone scheduled jobs
* Create custom UI to configure job
* [Dynamic variables support](#dynamic-variables)
* Monitor currently executing jobs
* Load custom job DLLs through configuration
* Display job execution logs, state, return message and error message
* Filter execution logs
* Store execution logs into any database
  * Build-in support for SQLite, MSSQL and PostgreSQL
* Auto cleanup of old execution logs
  * Configurable logs retention days
* Build-in Jobs
  * HTTP API client job

## Quick Start
### Using Docker
1. Create the following folders:
   * <blazingquartz_path>
   * <blazingquartz_path>/logs
   * <blazingquartz_path>/logs

2. Copy [BlazingQuartzDb.db](../blob/main/src/BlazingQuartz/BlazingQuartzApp/BlazingQuartzDb.db) to <blazingquartz_path>

3. Run below docker command:
    ```
    docker run -d \
    --name=BlazingQuartzApp \
    -v /<blazingquartz_path>/BlazingQuartzDb.db:/app/BlazingQuartzDb.db \
    -v /<blazingquartz_path>/logs:/app/logs \
    -v /<blazingquartz_path>/certs:/app/certs \
    -p 9090:80 \
    wilchn/blazingquartzapp:latest
    ```
    Note: Replace `<blazingquartz_path>`
4. Navigate to http://localhost:9090

## Details

### Dynamic Variables
Dynamic variables provide pre-defined set of variables that can be used when assigning value to `JobDataMap`. Their values are generated at the time of job execution. Any `JobDataMap` field that support `DataMapValueType.InterpolatedString` can use dynamic variable in format ``{{$variable}}``. For example, "list/`{{$datetime 'yyyy-mm-dd'}}`" will be replaced to "list/2022-09-26" during job execution. 

Below are list of pre-defined variables. Note that variable names are case-sensitive.
- `{{$guid}}` - Replace it with a RFC 4122 v4 UUID
- `{{$datetime rfc1123|iso8601|"custom format"|'custom format' [offset unit]}}` - Replace it with a UTC datetime string in either ISO8601, RFC1123 or a custom display format. You can also specify an offset relative to current date time. For example, `{{datetime iso8601 -1 d}}` to represent one day ago.
- `{{$localDatetime rfc1123|iso8601|"custom format"|'custom format' [offset unit]}}` - Similar to `$datetime` except it returns local date time.

`$datetime` and `$localDatetime` supports below offset units:
Unit   | Description
-------|------------
y      | Year
M      | Month
d      | Day
h      | Hour
m      | Minute
s      | Second
ms     | Millisecond
