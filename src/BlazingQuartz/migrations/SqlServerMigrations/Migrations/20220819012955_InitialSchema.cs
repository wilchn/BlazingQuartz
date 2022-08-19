using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlServerMigrations.Migrations
{
    public partial class InitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bqz_ExecutionLogs",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RunInstanceId = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LogType = table.Column<string>(type: "varchar(20)", nullable: false),
                    JobName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    JobGroup = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TriggerName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TriggerGroup = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ScheduleFireTimeUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    FireTimeUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    JobRunTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    ExceptionMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExecutionDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsVetoed = table.Column<bool>(type: "bit", nullable: true),
                    IsException = table.Column<bool>(type: "bit", nullable: true),
                    DateAddedUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bqz_ExecutionLogs", x => x.LogId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bqz_ExecutionLogs_DateAddedUtc_LogType",
                table: "bqz_ExecutionLogs",
                columns: new[] { "DateAddedUtc", "LogType" });

            migrationBuilder.CreateIndex(
                name: "IX_bqz_ExecutionLogs_RunInstanceId",
                table: "bqz_ExecutionLogs",
                column: "RunInstanceId",
                unique: true,
                filter: "[RunInstanceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_bqz_ExecutionLogs_TriggerName_TriggerGroup_JobName_JobGroup_DateAddedUtc",
                table: "bqz_ExecutionLogs",
                columns: new[] { "TriggerName", "TriggerGroup", "JobName", "JobGroup", "DateAddedUtc" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bqz_ExecutionLogs");
        }
    }
}
