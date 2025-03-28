﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gouda.Database.Migrations
{
    /// <inheritdoc />
    public partial class SuperpinMessageChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SuperpinMessageChannel",
                table: "Superpins",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuperpinMessageChannel",
                table: "Superpins");
        }
    }
}
