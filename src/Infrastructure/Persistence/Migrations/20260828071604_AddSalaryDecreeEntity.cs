using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryDecreeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalaryDecrees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    BaseDailySalary = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    AttractionAllowance = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: true),
                    SupervisionAllowance = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: true),
                    ShiftType = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ContractType = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    HousingAllowance = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: true),
                    FoodAllowance = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: true),
                    TransportationAllowanceNet = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: true),
                    KaranehAmountNet = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: true),
                    MaritalStatus = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    ChildrenCount = table.Column<int>(type: "int", nullable: false),
                    IsTaxSubject = table.Column<bool>(type: "bit", nullable: false),
                    InsuranceNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    SocialSecurityContractRow = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    PositionInInsuranceList = table.Column<string>(type: "nvarchar(100)", unicode: true, maxLength: 100, nullable: false),
                    IsSubjectTo7PercentInsurance = table.Column<bool>(type: "bit", nullable: false),
                    IsSubjectTo20PercentInsurance = table.Column<bool>(type: "bit", nullable: false),
                    IsSubjectTo3PercentInsurance = table.Column<bool>(type: "bit", nullable: false),
                    IsSubjectTo4PercentInsurance = table.Column<bool>(type: "bit", nullable: false),
                    InsuranceCalculationProfile = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryDecrees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryDecrees_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalaryDecrees_EmployeeId_EffectiveFrom",
                table: "SalaryDecrees",
                columns: new[] { "EmployeeId", "EffectiveFrom" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalaryDecrees");
        }
    }
}
