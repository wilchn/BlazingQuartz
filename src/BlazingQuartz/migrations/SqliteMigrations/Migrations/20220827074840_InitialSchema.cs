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
                    ScheduleFireTimeUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    FireTimeUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    JobRunTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Result = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: true),
                    IsVetoed = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsException = table.Column<bool>(type: "INTEGER", nullable: true),
                    DateAddedUtc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bqz_ExecutionLogs", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "bqz_ExecutionLogDetails",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "INTEGER", nullable: false),
                    ExecutionDetails = table.Column<string>(type: "TEXT", nullable: true),
                    ErrorStackTrace = table.Column<string>(type: "TEXT", nullable: true),
                    ErrorCode = table.Column<int>(type: "INTEGER", nullable: true),
                    ErrorHelpLink = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bqz_ExecutionLogDetails", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_bqz_ExecutionLogDetails_bqz_ExecutionLogs_LogId",
                        column: x => x.LogId,
                        principalTable: "bqz_ExecutionLogs",
                        principalColumn: "LogId",
                        onDelete: ReferentialAction.Cascade);
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
                name: "bqz_ExecutionLogDetails");

            migrationBuilder.DropTable(
                name: "bqz_ExecutionLogs");
        }
    }
}
