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
    [Migration("20250305103805_PinNumber")]
    partial class PinNumber
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Gouda.Database.Geoname", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Admin1Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Admin2Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Admin3Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Admin4Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("AsciiName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CountryCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<char>("FeatureClass")
                        .HasColumnType("character(1)");

                    b.Property<string>("FeatureCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("Latitude")
                        .HasColumnType("double precision");

                    b.Property<double>("Longitude")
                        .HasColumnType("double precision");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("Population")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Timezone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AsciiName");

                    b.ToTable("Geonames");
                });

            modelBuilder.Entity("Gouda.Database.GeonameAdmin1Codes", b =>
                {
                    b.Property<string>("Code")
                        .HasColumnType("text");

                    b.Property<string>("AsciiName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("GeonameId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Code");

                    b.ToTable("GeonameAdmin1Codes");
                });

            modelBuilder.Entity("Gouda.Database.GeonameAlternateNames", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("AlternateName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<decimal>("GeonameId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<bool>("IsColloquial")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsHistoric")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsPreferred")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsShort")
                        .HasColumnType("boolean");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AlternateName");

                    b.ToTable("GeonameAlternateNames");
                });

            modelBuilder.Entity("Gouda.Database.GeonameCountry", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("ContinentCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CurrencyCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CurrencyName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Iso3Code")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("IsoCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("IsoNumericCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Languages")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TelephoneCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Tld")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("GeonameCountries");
                });

            modelBuilder.Entity("Gouda.Database.GeonameDate", b =>
                {
                    b.Property<decimal>("Date")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Date");

                    b.ToTable("GeonameDate");
                });

            modelBuilder.Entity("Gouda.Database.GuildChannel", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("AlertChannel")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("ChatLogChannel")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal?>("SuperpinChannel")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("GuildChannels");
                });

            modelBuilder.Entity("Gouda.Database.GuildPin", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("PinEmoji")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("GuildPins");
                });

            modelBuilder.Entity("Gouda.Database.Locale", b =>
                {
                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("LocaleName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("UserId");

                    b.ToTable("Locales");
                });

            modelBuilder.Entity("Gouda.Database.Location", b =>
                {
                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<double>("Latitude")
                        .HasColumnType("double precision");

                    b.Property<double>("Longitude")
                        .HasColumnType("double precision");

                    b.HasKey("UserId");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("Gouda.Database.Pins", b =>
                {
                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Channel")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("Message")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("PinNumber")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("UserId", "Channel", "Message");

                    b.HasIndex("UserId", "PinNumber")
                        .IsUnique();

                    b.ToTable("Pins");
                });
#pragma warning restore 612, 618
        }
    }
}
