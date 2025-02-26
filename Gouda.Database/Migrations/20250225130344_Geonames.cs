using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gouda.Database.Migrations
{
    /// <inheritdoc />
    public partial class Geonames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeonameAdmin1Codes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AsciiName = table.Column<string>(type: "text", nullable: false),
                    GeonameId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeonameAdmin1Codes", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "GeonameAlternateNames",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    GeonameId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Language = table.Column<string>(type: "text", nullable: false),
                    AlternateName = table.Column<string>(type: "text", nullable: false),
                    IsPreferred = table.Column<bool>(type: "boolean", nullable: false),
                    IsShort = table.Column<bool>(type: "boolean", nullable: false),
                    IsColloquial = table.Column<bool>(type: "boolean", nullable: false),
                    IsHistoric = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeonameAlternateNames", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GeonameDate",
                columns: table => new
                {
                    Date = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeonameDate", x => x.Date);
                });

            migrationBuilder.CreateTable(
                name: "Geonames",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    AsciiName = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    FeatureClass = table.Column<char>(type: "character(1)", nullable: false),
                    FeatureCode = table.Column<string>(type: "text", nullable: false),
                    CountryCode = table.Column<string>(type: "text", nullable: false),
                    Admin1Code = table.Column<string>(type: "text", nullable: false),
                    Admin2Code = table.Column<string>(type: "text", nullable: false),
                    Admin3Code = table.Column<string>(type: "text", nullable: false),
                    Admin4Code = table.Column<string>(type: "text", nullable: false),
                    Population = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Timezone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Geonames", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeonameAlternateNames_AlternateName",
                table: "GeonameAlternateNames",
                column: "AlternateName");

            migrationBuilder.CreateIndex(
                name: "IX_Geonames_AsciiName",
                table: "Geonames",
                column: "AsciiName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeonameAdmin1Codes");

            migrationBuilder.DropTable(
                name: "GeonameAlternateNames");

            migrationBuilder.DropTable(
                name: "GeonameDate");

            migrationBuilder.DropTable(
                name: "Geonames");
        }
    }
}
