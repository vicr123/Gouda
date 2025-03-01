using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Gouda.Database;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;

namespace Gouda.Geocoding;

public class GeocodingService(GoudaDbContext dbContext, IInteractionContext interactionContext, IDiscordRestUserAPI userApi)
{
    private const double EarthRadiusKm = 6371;

    /// <summary>
    /// Finds a city based on Discord arguments.
    /// </summary>
    /// <param name="user">User from Discord arguments.</param>
    /// <param name="city">City from Discord arguments.</param>
    /// <returns>A resolved location, or null otherwise.</returns>
    public async Task<LocationResult?> Locate(IUser? user, string? city)
    {
        ulong geonameId;
        IUser? userObject = null;
        Location? location = null;
        if (city is not null)
        {
            geonameId = await SearchCity(city);
        }
        else if (user is not null)
        {
            location = await UserLocation(user);
            geonameId = await ClosestCity(location);
            userObject = user;
        }
        else
        {
            if (!interactionContext.TryGetUserID(out var interactionUserId))
            {
                return null;
            }

            location = await UserLocation(interactionUserId.Value);
            var closestCityTask = ClosestCity(location);
            var userObjectTask = userApi.GetUserAsync(interactionUserId);

            geonameId = await closestCityTask;
            userObject = (await userObjectTask).Entity;
        }

        var cityInformation = await CityInformation(geonameId);
        return new(cityInformation, userObject, location ?? new()
        {
            Latitude = cityInformation.Latitude,
            Longitude = cityInformation.Longitude,
        });
    }

    public async Task<ulong> ClosestCity(double lat, double lon)
    {
        var places = from place in dbContext.Geonames
            let dLat = double.DegreesToRadians(place.Latitude - lat)
            let dLon = double.DegreesToRadians(place.Longitude - lon)
            let a = double.Sin(dLat / 2) * double.Sin(dLat / 2) +
                    double.Cos(double.DegreesToRadians(place.Latitude)) * double.Cos(double.DegreesToRadians(lat)) *
                    double.Sin(dLon / 2) * double.Sin(dLon / 2)
            let c = 2 * double.Atan2(double.Sqrt(a), double.Sqrt(1 - a))
            select new
            {
                place.Id,
                Distance = EarthRadiusKm * c,
            };

        var geonames = await places.ToListAsync();
        return geonames.MinBy(x => x.Distance)!.Id;
    }

    public async Task<Location> UserLocation(IUser user)
    {
        return await UserLocation(user.ID.Value);
    }

    public async Task<Location> UserLocation(ulong user)
    {
        var location = await dbContext.Locations.FirstOrDefaultAsync(x => x.UserId == user);
        if (location is null)
        {
            throw new InvalidLocationException();
        }

        return location;
    }

    public async Task<ulong> ClosestCity(IUser user)
    {
        return await ClosestCity(user.ID.Value);
    }

    public async Task<ulong> ClosestCity(ulong user)
    {
        return await ClosestCity(await UserLocation(user));
    }

    public async Task<ulong> ClosestCity(Location location)
    {
        return await ClosestCity(location.Latitude, location.Longitude);
    }

    [SuppressMessage("Performance", "CA1862:Use the \'StringComparison\' method overloads to perform case-insensitive string comparisons", Justification = "EF SQL translation")]
    public async Task<ulong> SearchCity(string city)
    {
        string? admin1 = null;
        if (city.Contains(','))
        {
            admin1 = city[(city.LastIndexOf(',') + 1)..].Trim();
            city = city[..city.LastIndexOf(',')].Trim();
        }

        var alternateNames = await dbContext.GeonameAlternateNames.Where(altName =>
                altName.AlternateName.ToLower().StartsWith(city.ToLower())).Select(altName => altName.GeonameId)
            .Distinct()
            .Join(dbContext.Geonames, x => x, x => x.Id, (x, y) => new
            {
                y.Id, y.Population, y.Admin1Code, y.CountryCode,
            })
            .GroupJoin(dbContext.GeonameAdmin1Codes, x => x.CountryCode + "." + x.Admin1Code, y => y.Code, (x, y) => new
            {
                x.Id, x.Population, x.CountryCode, PossibleCodes = y,
            })
            .SelectMany(x => x.PossibleCodes.DefaultIfEmpty(), (x, y) => new
            {
                x.Id, x.Population, x.CountryCode, Admin1 = y == null ? null : y.Name, Admin1Id = y == null ? null : (ulong?)y.GeonameId,
            })
            .GroupJoin(dbContext.GeonameAlternateNames, x => x.Admin1Id, y => y.GeonameId, (x, y) => new
            {
                x.Id, x.CountryCode, x.Admin1, x.Population, PossibleAlternates = y,
            })
            .SelectMany(x => x.PossibleAlternates.DefaultIfEmpty(), (x, y) => new
            {
                x.Id, x.CountryCode,
                Population =
                    admin1 != null && x.Admin1 != null && y != null && (y.AlternateName.ToLower().StartsWith(admin1.ToLower()) ||
                                                           x.Admin1.ToLower().StartsWith(admin1.ToLower()) ||
                                                           x.CountryCode.ToLower().StartsWith(admin1.ToLower()))
                        ? x.Population * 1000000000
                        : x.Population,
            })
            .OrderByDescending(x => x.Population)
            .FirstOrDefaultAsync();

        if (alternateNames is null)
        {
            throw new InvalidLocationException();
        }

        return alternateNames.Id;
    }

    public async Task<LocalisedGeoname> CityInformation(ulong id)
    {
        var geoname = await dbContext.Geonames.SingleAsync(x => x.Id == id);
        var alternateName = await BestAlternateName(id);

        var admin1 =
            await dbContext.GeonameAdmin1Codes.FirstOrDefaultAsync(x =>
                x.Code == $"{geoname.CountryCode}.{geoname.Admin1Code}");
        var admin1Short =
            admin1 is null ? null : await dbContext.GeonameAlternateNames.FirstOrDefaultAsync(x =>
                x.GeonameId == admin1.GeonameId && x.Language == "abbr");
        var admin1AlternateName = admin1 is null ? null : await BestAlternateName(admin1.GeonameId);

        return new(id, alternateName ?? geoname.Name, admin1AlternateName ?? admin1?.Name, admin1Short?.AlternateName, geoname.Timezone, geoname.CountryCode, geoname.Latitude, geoname.Longitude);
    }

    private async Task<string?> BestAlternateName(ulong id)
    {
        var fullLang = CultureInfo.CurrentCulture.Name;
        var lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

        var alternateName = await dbContext.GeonameAlternateNames
            .Where(x => x.GeonameId == id && (x.Language == lang || x.Language == "en" || x.Language == fullLang))
            .Select(x => new
            {
                x.AlternateName,
                Score = 1 * (x.Language == fullLang ? 1000 : 1) * (x.Language == lang ? 100 : 1) * (x.IsPreferred ? 10 : 1),
            })
            .OrderByDescending(x => x.Score)
            .FirstOrDefaultAsync();

        return alternateName?.AlternateName;
    }

    public record LocalisedGeoname(ulong Id, string Name, string? Admin1, string? Admin1ShortName, string Timezone, string Country, double Latitude, double Longitude);

    public record LocationResult(LocalisedGeoname Geoname, IUser? User, Location location);
}
