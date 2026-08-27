using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLaborLawRuleEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LaborLawRuleItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaborLawRuleItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LaborLawRuleItems_Key_EffectiveFrom",
                table: "LaborLawRuleItems",
                columns: new[] { "Key", "EffectiveFrom" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LaborLawRuleItems");
        }
    }
}
