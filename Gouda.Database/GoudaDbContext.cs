using Microsoft.EntityFrameworkCore;

namespace Gouda.Database;

public class GoudaDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Locale> Locales { get; set; }

    public DbSet<Location> Locations { get; set; }

    public DbSet<Pins> Pins { get; set; }

    public DbSet<Geoname> Geonames { get; set; }

    public DbSet<GeonameAlternateNames> GeonameAlternateNames { get; set; }

    public DbSet<GeonameDate> GeonameDate { get; set; }

    public DbSet<GeonameAdmin1Codes> GeonameAdmin1Codes { get; set; }

    public DbSet<GuildChannel> GuildChannels { get; set; }

    public DbSet<GuildPin> GuildPins { get; set; }

    public DbSet<GeonameCountry> GeonameCountries { get; set; }
}
