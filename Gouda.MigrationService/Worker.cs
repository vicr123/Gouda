using System.Diagnostics;
using System.IO.Compression;
using EFCore.BulkExtensions;
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

        using var migrateActivity = ActivitySource.StartActivity("Migrating database", ActivityKind.Client);
        await InitializeDatabaseAsync(dbContext, cancellationToken);

        using var populateGeonamesActivity = ActivitySource.StartActivity("Populating Geonames Database", ActivityKind.Client);
        await PopulateGeonamesDatabaseAsync(dbContext, cancellationToken);

        hostApplicationLifetime.StopApplication();
    }

    private async Task InitializeDatabaseAsync(GoudaDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    private async Task PopulateGeonamesDatabaseAsync(GoudaDbContext dbContext, CancellationToken cancellationToken = default)
    {
        // Check if the Geonames database needs to be updated
        var today = ulong.Parse(DateTime.UtcNow.ToString("yyyyMMdd"));
        var oneYearAgo = ulong.Parse(DateTime.UtcNow.AddYears(-1).ToString("yyyyMMdd"));

        var geonameDate = await dbContext.GeonameDate.FirstOrDefaultAsync(cancellationToken);
        var lastUpdate = geonameDate?.Date ?? 0;
        if (lastUpdate > oneYearAgo)
        {
            // No update required
            return;
        }

        // Save the downloaded data
        await dbContext.TruncateAsync<Geoname>(cancellationToken: cancellationToken);
        await dbContext.TruncateAsync<GeonameAlternateNames>(cancellationToken: cancellationToken);
        await dbContext.TruncateAsync<GeonameAdmin1Codes>(cancellationToken: cancellationToken);
        await BatchInsert(
            dbContext,
            (await ReadGeonamesFileFromInternet("cities500.zip", cancellationToken)).Select(city =>
                {
                    var parts = city.Split("\t");
                    return new Geoname
                    {
                        Id = ulong.Parse(parts[0]),
                        Name = parts[1],
                        AsciiName = parts[2],
                        Latitude = double.Parse(parts[4]),
                        Longitude = double.Parse(parts[5]),
                        FeatureClass = parts[6][0],
                        FeatureCode = parts[7],
                        CountryCode = parts[8],
                        Admin1Code = parts[10],
                        Admin2Code = parts[11],
                        Admin3Code = parts[12],
                        Admin4Code = parts[13],
                        Population = ulong.Parse(parts[14]),
                        Timezone = parts[17],
                    };
                }),
            "cities500.txt");

        await BatchInsert(
            dbContext,
            (await ReadGeonamesFileFromInternet("admin1CodesASCII.txt", cancellationToken)).Select(admin =>
            {
                var parts = admin.Split("\t");
                return new GeonameAdmin1Codes
                {
                    Code = parts[0],
                    Name = parts[1],
                    AsciiName = parts[2],
                    GeonameId = ulong.Parse(parts[3]),
                };
            }),
            "admin1CodesASCII.txt");

        await BatchInsert(
            dbContext,
            (await ReadGeonamesFileFromInternet("alternateNamesV2.zip", cancellationToken)).Select(altName =>
                {
                    var parts = altName.Split("\t");
                    if (parts.Length <= 3)
                    {
                        return null;
                    }

                    return new GeonameAlternateNames
                    {
                        Id = ulong.Parse(parts[0]),
                        GeonameId = ulong.Parse(parts[1]),
                        Language = parts[2],
                        AlternateName = parts[3],
                        IsPreferred = parts.Length > 4 && parts[4] == "1",
                        IsShort = parts.Length > 5 && parts[5] == "1",
                        IsColloquial = parts.Length > 6 && parts[6] == "1",
                        IsHistoric = parts.Length > 7 && parts[7] == "1",
                    };
                }).Where(x => x is not null && x.Language != string.Empty).Cast<GeonameAlternateNames>(),
            "alternateNamesV2.txt");

        if (geonameDate is not null)
        {
            dbContext.Remove(geonameDate);
        }

        dbContext.Add(new GeonameDate
        {
            Date = today,
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task<IEnumerable<string>> ReadGeonamesFileFromInternet(string file, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Downloading Geonames ({file})...");

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.CacheControl = new()
        {
            NoCache = true,
        };
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("gouda/1.0");
        var response = await httpClient.GetAsync($"https://download.geonames.org/export/dump/{file}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        if (file.EndsWith(".zip"))
        {
            var z = new ZipArchive(responseStream, ZipArchiveMode.Read);
            foreach (var entry in z.Entries.Where(n => !n.Name.StartsWith("readme", StringComparison.OrdinalIgnoreCase) && !n.Name.StartsWith("iso-languagecodes", StringComparison.OrdinalIgnoreCase)))
            {
                using var reader = new StreamReader(entry.Open());
                var str = await reader.ReadToEndAsync(cancellationToken);
                return str.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            }
        }
        else
        {
            using var reader = new StreamReader(responseStream);
            var str = await reader.ReadToEndAsync(cancellationToken);
            return str.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }

        throw new();
    }

    private static async Task BatchInsert<T>(GoudaDbContext dbContext, IEnumerable<T> enumerable, string file) where T : class
    {
        Console.WriteLine($"Inserting {file}...");
        await dbContext.BulkInsertAsync(enumerable);
    }
}
