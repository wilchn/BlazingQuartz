using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqliteMigrations.Migrations
{
    public partial class InitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bqz_ExecutionLogs",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RunInstanceId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    LogType = table.Column<string>(type: "varchar(20)", nullable: false),
                    JobName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    JobGroup = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TriggerName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    TriggerGroup = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ScheduleFireTimeUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    FireTimeUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    JobRunTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Result = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: true),
                    ExceptionMessage = table.Column<string>(type: "TEXT", nullable: true),
                    ExecutionDetails = table.Column<string>(type: "TEXT", nullable: true),
                    IsVetoed = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsException = table.Column<bool>(type: "INTEGER", nullable: true),
                    DateAddedUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
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
                unique: true);

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
