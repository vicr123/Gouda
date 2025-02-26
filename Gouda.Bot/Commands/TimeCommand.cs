using System.ComponentModel;
using Gouda.Database;
using Gouda.Geocoding;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.Commands.Contexts;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Feedback.Services;
using Remora.Results;

namespace Gouda.Bot.Commands;

public class TimeCommand(GoudaDbContext dbContext, GeocodingService geocodingService, IFeedbackService feedbackService, IInteractionContext interactionContext) : CommandGroup
{
    [Command("time")]
    [Description("Get the time for a user")]
    public async Task<Result> GetTime([Description("The user to get the time for")] IUser? user = null, [Description("The city to get the time for")] string? city = null)
    {
        try
        {
            ulong geonameId;
            if (city is not null)
            {
                geonameId = await geocodingService.SearchCity(city);
            }
            else if (user is not null)
            {
                geonameId = await geocodingService.ClosestCity(user);
            }
            else
            {
                if (!interactionContext.TryGetUserID(out var interactionUserId))
                {
                    await feedbackService.SendContextualAsync("There was a problem");
                    return Result.Success;
                }

                geonameId = await geocodingService.ClosestCity(interactionUserId.Value);
            }

            var geoname = await geocodingService.CityInformation(geonameId, "en");

            var localTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById(geoname.Timezone));

            string?[] parts = [geoname.Name, geoname.Admin1ShortName ?? geoname.Admin1, geoname.Country];
            await feedbackService.SendContextualAsync($":clock10: **{string.Join(", ", parts.Where(x => x is not null))}** {localTime:dddd, dd MMMM yyyy HH:mm}");
        }
        catch (InvalidLocationException)
        {
            await feedbackService.SendContextualAsync($"User has not set a location");
        }

        return Result.Success;
    }
}
