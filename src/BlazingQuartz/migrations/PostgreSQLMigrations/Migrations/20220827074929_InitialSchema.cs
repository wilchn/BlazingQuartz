using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PostgreSQLMigrations.Migrations
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
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RunInstanceId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LogType = table.Column<string>(type: "varchar(20)", nullable: false),
                    JobName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    JobGroup = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TriggerName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    TriggerGroup = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ScheduleFireTimeUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FireTimeUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    JobRunTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: true),
                    Result = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    IsVetoed = table.Column<bool>(type: "boolean", nullable: true),
                    IsException = table.Column<bool>(type: "boolean", nullable: true),
                    DateAddedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bqz_ExecutionLogs", x => x.LogId);
                });

            migrationBuilder.CreateTable(
                name: "bqz_ExecutionLogDetails",
                columns: table => new
                {
                    LogId = table.Column<long>(type: "bigint", nullable: false),
                    ExecutionDetails = table.Column<string>(type: "text", nullable: true),
                    ErrorStackTrace = table.Column<string>(type: "text", nullable: true),
                    ErrorCode = table.Column<int>(type: "integer", nullable: true),
                    ErrorHelpLink = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
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
                name: "IX_bqz_ExecutionLogs_TriggerName_TriggerGroup_JobName_JobGroup~",
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
