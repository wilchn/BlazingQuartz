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
                name: "bqz_execution_logs",
                columns: table => new
                {
                    log_id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    run_instance_id = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    log_type = table.Column<string>(type: "varchar(20)", nullable: false),
                    job_name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    job_group = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    trigger_name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    trigger_group = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    schedule_fire_time_utc = table.Column<long>(type: "INTEGER", nullable: true),
                    fire_time_utc = table.Column<long>(type: "INTEGER", nullable: true),
                    job_run_time = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    retry_count = table.Column<int>(type: "INTEGER", nullable: true),
                    result = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: true),
                    error_message = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: true),
                    is_vetoed = table.Column<bool>(type: "INTEGER", nullable: true),
                    is_exception = table.Column<bool>(type: "INTEGER", nullable: true),
                    date_added_utc = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bqz_execution_logs", x => x.log_id);
                });

            migrationBuilder.CreateTable(
                name: "bqz_execution_log_details",
                columns: table => new
                {
                    log_id = table.Column<long>(type: "INTEGER", nullable: false),
                    execution_details = table.Column<string>(type: "TEXT", nullable: true),
                    error_stack_trace = table.Column<string>(type: "TEXT", nullable: true),
                    error_code = table.Column<int>(type: "INTEGER", nullable: true),
                    error_help_link = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bqz_execution_log_details", x => x.log_id);
                    table.ForeignKey(
                        name: "fk_bqz_execution_log_details_bqz_execution_logs_log_id",
                        column: x => x.log_id,
                        principalTable: "bqz_execution_logs",
                        principalColumn: "log_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bqz_execution_logs_date_added_utc_log_type",
                table: "bqz_execution_logs",
                columns: new[] { "date_added_utc", "log_type" });

            migrationBuilder.CreateIndex(
                name: "ix_bqz_execution_logs_run_instance_id",
                table: "bqz_execution_logs",
                column: "run_instance_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bqz_execution_logs_trigger_name_trigger_group_job_name_job_group_date_added_utc",
                table: "bqz_execution_logs",
                columns: new[] { "trigger_name", "trigger_group", "job_name", "job_group", "date_added_utc" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bqz_execution_log_details");

            migrationBuilder.DropTable(
                name: "bqz_execution_logs");
        }
    }
}
