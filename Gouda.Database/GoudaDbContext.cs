using Microsoft.EntityFrameworkCore;

namespace Gouda.Database;

public class GoudaDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Locale> Locales { get; set; }
}
