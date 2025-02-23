using Microsoft.EntityFrameworkCore;

namespace Gouda.Database;

public class GoudaDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Locale> Locales { get; set; }

    public DbSet<Location> Locations { get; set; }

    public DbSet<Pins> Pins { get; set; }
}
