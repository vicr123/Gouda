using System.ComponentModel;
using System.Globalization;
using Gouda.Bot.Services;
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

public class TimeCommand(GeocodingService geocodingService, IFeedbackService feedbackService, TranslationService translationService) : CommandGroup
{
    [Command("time")]
    [Description("Get the time for a user")]
    public async Task<Result> GetTime([Description("The user to get the time for")] IUser? user = null, [Description("The city to get the time for")] string? city = null)
    {
        try
        {
            var response = await geocodingService.Locate(user, city);
            if (response is null)
            {
                await feedbackService.SendContextualAsync(translationService["TIME_ERROR"]);
                return Result.Success;
            }

            var (geoname, userObject, _) = response;

            var localTime = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, TimeZoneInfo.FindSystemTimeZoneById(geoname.Timezone));

            string?[] parts = [geoname.Name, geoname.Admin1ShortName ?? geoname.Admin1, geoname.CountryCode];
            await feedbackService.SendContextualAsync($":clock10: **{userObject?.Username ?? string.Join(", ", parts.Where(x => x is not null))}** {localTime:dddd, dd MMMM yyyy HH:mm}");
        }
        catch (InvalidLocationException)
        {
            await feedbackService.SendContextualAsync(translationService["TIME_ERROR"]);
        }

        return Result.Success;
    }
}
