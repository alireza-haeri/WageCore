using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOnPayrollRecordPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayrollRecords_EmployeeId_PeriodStart",
                table: "PayrollRecords");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_EmployeeId_PeriodStart_PeriodEnd",
                table: "PayrollRecords",
                columns: new[] { "EmployeeId", "PeriodStart", "PeriodEnd" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayrollRecords_EmployeeId_PeriodStart_PeriodEnd",
                table: "PayrollRecords");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_EmployeeId_PeriodStart",
                table: "PayrollRecords",
                columns: new[] { "EmployeeId", "PeriodStart" });
        }
    }
}
