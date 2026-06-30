using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BibWpf.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orte",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Land = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orte", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Autoren",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Vorname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nachname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Geburtsjahr = table.Column<int>(type: "integer", nullable: true),
                    OrtId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Autoren", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Autoren_Orte_OrtId",
                        column: x => x.OrtId,
                        principalTable: "Orte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Verlage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Gruendungsjahr = table.Column<int>(type: "integer", nullable: true),
                    OrtId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Verlage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Verlage_Orte_OrtId",
                        column: x => x.OrtId,
                        principalTable: "Orte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Buecher",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titel = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Isbn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Erscheinungsjahr = table.Column<int>(type: "integer", nullable: false),
                    Seiten = table.Column<int>(type: "integer", nullable: true),
                    Beschreibung = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AutorId = table.Column<int>(type: "integer", nullable: false),
                    VerlagId = table.Column<int>(type: "integer", nullable: false),
                    OrtId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buecher", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buecher_Autoren_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Autoren",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Buecher_Orte_OrtId",
                        column: x => x.OrtId,
                        principalTable: "Orte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Buecher_Verlage_VerlagId",
                        column: x => x.VerlagId,
                        principalTable: "Verlage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Autoren_OrtId",
                table: "Autoren",
                column: "OrtId");

            migrationBuilder.CreateIndex(
                name: "IX_Buecher_AutorId",
                table: "Buecher",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Buecher_OrtId",
                table: "Buecher",
                column: "OrtId");

            migrationBuilder.CreateIndex(
                name: "IX_Buecher_Titel",
                table: "Buecher",
                column: "Titel");

            migrationBuilder.CreateIndex(
                name: "IX_Buecher_VerlagId",
                table: "Buecher",
                column: "VerlagId");

            migrationBuilder.CreateIndex(
                name: "IX_Orte_Name",
                table: "Orte",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Verlage_Name",
                table: "Verlage",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Verlage_OrtId",
                table: "Verlage",
                column: "OrtId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Buecher");

            migrationBuilder.DropTable(
                name: "Autoren");

            migrationBuilder.DropTable(
                name: "Verlage");

            migrationBuilder.DropTable(
                name: "Orte");
        }
    }
}
