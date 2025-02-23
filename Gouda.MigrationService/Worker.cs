using System.Diagnostics;
using Gouda.Database;
using Microsoft.EntityFrameworkCore;

namespace Gouda.MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime) : BackgroundService
{
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GoudaDbContext>();

        using var activity = ActivitySource.StartActivity("Migrating database", ActivityKind.Client);
        await InitializeDatabaseAsync(dbContext, cancellationToken);

        hostApplicationLifetime.StopApplication();
    }

    private async Task InitializeDatabaseAsync(GoudaDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
    }
}
