using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gouda.Database.Migrations
{
    /// <inheritdoc />
    public partial class Superpins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Superpins",
                columns: table => new
                {
                    Channel = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Message = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    SuperpinMessage = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Superpins", x => new { x.Channel, x.Message });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Superpins");
        }
    }
}
