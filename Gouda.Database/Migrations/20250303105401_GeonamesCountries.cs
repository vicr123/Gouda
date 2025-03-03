using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gouda.Database.Migrations
{
    /// <inheritdoc />
    public partial class GeonamesCountries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GeonameCountries",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    IsoCode = table.Column<string>(type: "text", nullable: false),
                    Iso3Code = table.Column<string>(type: "text", nullable: false),
                    IsoNumericCode = table.Column<string>(type: "text", nullable: false),
                    ContinentCode = table.Column<string>(type: "text", nullable: false),
                    Tld = table.Column<string>(type: "text", nullable: false),
                    CurrencyCode = table.Column<string>(type: "text", nullable: false),
                    CurrencyName = table.Column<string>(type: "text", nullable: false),
                    TelephoneCode = table.Column<string>(type: "text", nullable: false),
                    Languages = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeonameCountries", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GeonameCountries");
        }
    }
}
