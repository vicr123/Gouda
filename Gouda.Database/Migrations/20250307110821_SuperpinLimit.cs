using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gouda.Database.Migrations
{
    /// <inheritdoc />
    public partial class SuperpinLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SuperpinLimit",
                table: "GuildPins",
                type: "integer",
                nullable: false,
                defaultValue: 5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuperpinLimit",
                table: "GuildPins");
        }
    }
}
