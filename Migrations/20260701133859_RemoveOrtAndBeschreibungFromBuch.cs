using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BibWpf.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOrtAndBeschreibungFromBuch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buecher_Orte_OrtId",
                table: "Buecher");

            migrationBuilder.DropIndex(
                name: "IX_Buecher_OrtId",
                table: "Buecher");

            migrationBuilder.DropColumn(
                name: "Beschreibung",
                table: "Buecher");

            migrationBuilder.DropColumn(
                name: "OrtId",
                table: "Buecher");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Beschreibung",
                table: "Buecher",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrtId",
                table: "Buecher",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buecher_OrtId",
                table: "Buecher",
                column: "OrtId");

            migrationBuilder.AddForeignKey(
                name: "FK_Buecher_Orte_OrtId",
                table: "Buecher",
                column: "OrtId",
                principalTable: "Orte",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
