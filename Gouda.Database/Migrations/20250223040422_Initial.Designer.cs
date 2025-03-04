﻿// <auto-generated />
using Gouda.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Gouda.Database.Migrations
{
    [DbContext(typeof(GoudaDbContext))]
    [Migration("20250223040422_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Gouda.Database.Locale", b =>
                {
                    b.Property<decimal>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("LocaleName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("UserId");

                    b.ToTable("Locales");
                });
#pragma warning restore 612, 618
        }
    }
}
