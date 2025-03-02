using Gouda.ApiService.Services;
using Gouda.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.API.Objects;
using Remora.Discord.Extensions.Embeds;
using Remora.Discord.Interactivity;
using Remora.Rest.Core;

namespace Gouda.ApiService.Controllers;

[ApiController]
[Route("api/guild")]
public class GuildController(GoudaDbContext dbContext, DiscordUserService discordUserService, IDiscordRestGuildAPI guildApi, IDiscordRestUserAPI userApi, IDiscordRestChannelAPI channelApi) : Controller
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<GuildInfo>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetGuilds()
    {
        var userId = await discordUserService.LoggedInUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

        var botGuildsTask = userApi.GetCurrentUserGuildsAsync();

        using var requestCustomisationUser = await userApi.Userify(HttpContext);
        using var requestCustomisationGuild = await guildApi.Userify(HttpContext);
        var guildsTask = userApi.GetCurrentUserGuildsAsync();

        var guilds = await guildsTask;
        var botGuilds = await botGuildsTask;

        return Json(guilds.Entity
            .Where(g => g.IsOwner.Value || g.Permissions.Value.GetPermissions()
                .Any(x => x is DiscordPermission.ManageGuild or DiscordPermission.Administrator))
            .Select(g => new GuildInfo
            {
                Id = g.ID.Value.Value.ToString(),
                Name = g.Name.Value,
                Present = botGuilds.Entity.Any(x => x.ID.Value == g.ID.Value),
            }));
    }

    [HttpGet]
    [ProducesResponseType<FullGuildInfo>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Route("{guildId}")]
    public async Task<IActionResult> GetGuild(string guildId)
    {
        var guildIdOk = ulong.TryParse(guildId, out ulong guildIdNumber);
        if (!guildIdOk)
        {
            return NotFound();
        }

        var userId = await discordUserService.LoggedInUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

        var botGuildsTask = userApi.GetCurrentUserGuildsAsync();

        using var requestCustomisationUser = await userApi.Userify(HttpContext);
        var guildsTask = userApi.GetCurrentUserGuildsAsync();

        var guilds = await guildsTask;

        if (guilds.Entity.All(x => x.ID.Value.Value != guildIdNumber))
        {
            return NotFound();
        }

        var guild = guilds.Entity.First(x => x.ID.Value.Value == guildIdNumber);
        var channels = await guildApi.GetGuildChannelsAsync(guild.ID.Value);
        var guildChannels = await dbContext.GuildChannels.FirstOrDefaultAsync(x => x.Id == guild.ID.Value.Value) ?? new();
        var pinEmoji = await dbContext.GuildPins.FirstOrDefaultAsync(x => x.Id == guild.ID.Value.Value) ?? new()
        {
            PinEmoji = "\ufffd", // Japanese Goblin
        };

        var botGuilds = await botGuildsTask;
        var present = botGuilds.Entity.Any(x => x.ID.Value == guild.ID.Value);

        return Json(new FullGuildInfo
        {
            Id = guild.ID.Value.Value.ToString(),
            Name = guild.Name.Value,
            Present = present,
            Channels = present ? channels.Entity.Where(x => x.Type == ChannelType.GuildText).OrderBy(x => x.Position.Value).Select(x => new ChannelInfo
            {
                Id = x.ID.Value.ToString(),
                Name = x.Name.Value ?? string.Empty,
                Parent = x.ParentID.HasValue ? x.ParentID.Value.ToString() : null,
            }) : [],
            Categories = present ? channels.Entity.Where(x => x.Type == ChannelType.GuildCategory).OrderBy(x => x.Position.Value).Select(x => new CategoryInfo
            {
                Id = x.ID.Value.ToString(),
                Name = x.Name.Value ?? string.Empty,
                Parent = x.ParentID.HasValue ? x.ParentID.Value.ToString() : null,
            }) : [],
            AlertChannel = guildChannels.AlertChannel?.ToString(),
            ChatLogsChannel = guildChannels.ChatLogChannel?.ToString(),
            SuperpinsChannel = guildChannels.SuperpinChannel?.ToString(),
            PinEmoji = pinEmoji.PinEmoji,
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Route("{guildId}/channels")]
    public async Task<IActionResult> SetAlertChannel(string guildId, [FromBody] GuildChannelUpdateInfo channelUpdateInfo)
    {
        var guildIdOk = ulong.TryParse(guildId, out ulong guildIdNumber);
        if (!guildIdOk)
        {
            return NotFound();
        }

        var userId = await discordUserService.LoggedInUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

        using var requestCustomisationUser = await userApi.Userify(HttpContext);
        var guilds = await userApi.GetCurrentUserGuildsAsync();

        if (guilds.Entity.All(x => x.ID.Value.Value != guildIdNumber))
        {
            return NotFound();
        }

        var guild = guilds.Entity.First(x => x.ID.Value.Value == guildIdNumber);
        if (!guild.IsOwner.Value && !guild.Permissions.Value.GetPermissions()
                .Any(x => x is DiscordPermission.ManageGuild or DiscordPermission.Administrator))
        {
            return Unauthorized();
        }

        var alertChannelIdOk = ulong.TryParse(channelUpdateInfo.AlertChannel, out var alertChannelIdNumber);
        if (!alertChannelIdOk && channelUpdateInfo.AlertChannel is not null)
        {
            return BadRequest();
        }

        var chatLogsChannelIdOk = ulong.TryParse(channelUpdateInfo.ChatLogsChannel, out var chatLogsChannelIdNumber);
        if (!chatLogsChannelIdOk && channelUpdateInfo.ChatLogsChannel is not null)
        {
            return BadRequest();
        }

        var superpinsChannelIdOk = ulong.TryParse(channelUpdateInfo.SuperpinsChannel, out var superpinsChannelIdNumber);
        if (!superpinsChannelIdOk && channelUpdateInfo.SuperpinsChannel is not null)
        {
            return BadRequest();
        }

        var channelData = new GuildChannel
        {
            Id = guild.ID.Value.Value,
            AlertChannel = alertChannelIdOk ? alertChannelIdNumber : null,
            ChatLogChannel = chatLogsChannelIdOk ? chatLogsChannelIdNumber : null,
            SuperpinChannel = superpinsChannelIdOk ? superpinsChannelIdNumber : null,
        };

        await dbContext.GuildChannels.Upsert(channelData).WhenMatched(x => new()
        {
            AlertChannel = channelData.AlertChannel,
            ChatLogChannel = channelData.ChatLogChannel,
            SuperpinChannel = channelData.SuperpinChannel,
        }).RunAsync();

        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Route("{guildId}/tickets")]
    public async Task<IActionResult> CreateTicketChannel(string guildId, [FromBody] TicketChannelCreateArgs args)
    {
        var guildIdOk = ulong.TryParse(guildId, out var guildIdNumber);
        if (!guildIdOk)
        {
            return NotFound();
        }

        var userId = await discordUserService.LoggedInUserIdAsync();
        if (userId is null)
        {
            return Unauthorized();
        }

        using var requestCustomisationUser = await userApi.Userify(HttpContext);
        var guilds = await userApi.GetCurrentUserGuildsAsync();

        if (guilds.Entity.All(x => x.ID.Value.Value != guildIdNumber))
        {
            return NotFound();
        }

        var guild = guilds.Entity.First(x => x.ID.Value.Value == guildIdNumber);
        if (!guild.IsOwner.Value && !guild.Permissions.Value.GetPermissions()
                .Any(x => x is DiscordPermission.ManageGuild or DiscordPermission.Administrator))
        {
            return Unauthorized();
        }

        var channelIdOk = ulong.TryParse(args.ChannelId, out var channelIdNumber);
        if (!channelIdOk)
        {
            return BadRequest();
        }

        var channels = await guildApi.GetGuildChannelsAsync(new(guildIdNumber));
        if (channels.Entity.All(x => x.ID.Value != channelIdNumber))
        {
            return BadRequest();
        }

        await channelApi.CreateMessageAsync(new(channelIdNumber), embeds: new([
            new EmbedBuilder().WithTitle("Tickets").WithDescription(args.Message).Build()
                .Entity
        ]), components: new([
            new ActionRowComponent([
                new ButtonComponent(ButtonComponentStyle.Success, args.ButtonText, CustomID: CustomIDHelpers.CreateButtonID("ticket")),
            ]),
        ]));
        return NoContent();
    }

    private class GuildInfo
    {
        public required string Id { get; set; }

        public required string Name { get; set; }

        public required bool Present { get; set; }
    }

    private class FullGuildInfo
    {
        public required string Id { get; set; }

        public required string Name { get; set; }

        public required bool Present { get; set; }

        public required IEnumerable<CategoryInfo> Categories { get; set; }

        public required IEnumerable<ChannelInfo> Channels { get; set; }

        public required string? AlertChannel { get; set; }

        public required string? ChatLogsChannel { get; set; }

        public required string? SuperpinsChannel { get; set; }

        public required string PinEmoji { get; set; }
    }

    private class ChannelInfo
    {
        public required string Id { get; set; }

        public required string Name { get; set; }

        public required string? Parent { get; set; }
    }

    private class CategoryInfo
    {
        public required string Id { get; set; }

        public required string Name { get; set; }

        public required string? Parent { get; set; }
    }

    public class GuildChannelUpdateInfo
    {
        public string? AlertChannel { get; set; }

        public string? ChatLogsChannel { get; set; }

        public string? SuperpinsChannel { get; set; }
    }

    public class TicketChannelCreateArgs
    {
        public required string ChannelId { get; set; }

        public required string Message { get; set; }

        public required string ButtonText { get; set; }
    }
}
