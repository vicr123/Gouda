using System.Reflection;
using Gouda.ApiService.Services;
using Gouda.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gouda.ApiService.Controllers;

[ApiController]
[Route("/api/usersettings")]
public class UserSettingsController(GoudaDbContext dbContext, DiscordUserService discordUserService) : Controller
{
    [HttpGet]
    [ProducesResponseType<GetUserSettingsResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserSettings()
    {
        var userId = await discordUserService.LoggedInUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

        var locale = await dbContext.Locales.FirstOrDefaultAsync(x => x.UserId == userId);

        return Json(new GetUserSettingsResult
        {
            Locale = locale?.LocaleName ?? "en",
            AvailableLocales = BotLocales.BotLocales.AvailableLanguages(),
        });
    }

    [HttpPost]
    [Route("locale")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetLocale([FromBody] string locale)
    {
        var userId = await discordUserService.LoggedInUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

        await dbContext.Locales.Upsert(new()
        {
            LocaleName = locale,
            UserId = userId.Value,
        }).WhenMatched(x => new()
        {
            LocaleName = locale,
        }).RunAsync();
        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    private class GetUserSettingsResult
    {
        public required string Locale { get; set; }

        public required IEnumerable<string> AvailableLocales { get; set; }
    }
}
