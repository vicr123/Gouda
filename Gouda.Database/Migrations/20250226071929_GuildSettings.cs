using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gouda.Database.Migrations
{
    /// <inheritdoc />
    public partial class GuildSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildChannels",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    AlertChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    ChatLogChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    SuperpinChannel = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildChannels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuildPins",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    PinEmoji = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildPins", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildChannels");

            migrationBuilder.DropTable(
                name: "GuildPins");
        }
    }
}
