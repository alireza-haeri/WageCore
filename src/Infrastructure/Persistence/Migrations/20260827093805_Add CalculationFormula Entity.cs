using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCalculationFormulaEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LaborLawRuleItems_Key_EffectiveFrom",
                table: "LaborLawRuleItems");

            migrationBuilder.CreateTable(
                name: "CalculationFormulas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Expression = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculationFormulas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LaborLawRuleItems_EffectiveFrom",
                table: "LaborLawRuleItems",
                column: "EffectiveFrom",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalculationFormulas_EffectiveFrom",
                table: "CalculationFormulas",
                column: "EffectiveFrom",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalculationFormulas");

            migrationBuilder.DropIndex(
                name: "IX_LaborLawRuleItems_EffectiveFrom",
                table: "LaborLawRuleItems");

            migrationBuilder.CreateIndex(
                name: "IX_LaborLawRuleItems_Key_EffectiveFrom",
                table: "LaborLawRuleItems",
                columns: new[] { "Key", "EffectiveFrom" });
        }
    }
}
