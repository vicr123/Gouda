using Gouda;
using Gouda.Bot;
using Gouda.Bot.Commands;
using Remora.Commands.Extensions;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Caching.Extensions;
using Remora.Discord.Commands.Extensions;
using Remora.Discord.Commands.Services;
using Remora.Discord.Extensions.Extensions;
using Remora.Discord.Hosting.Extensions;
using Remora.Discord.Interactivity.Extensions;
using Remora.Rest.Core;

var builder = Host.CreateApplicationBuilder(args);

var goudaConfig = builder.Configuration.GetSection("Gouda").Get<GoudaConfiguration>() ?? throw new InvalidOperationException("Could not bind configuration.");

builder.Services
    .AddDiscordService(_ => goudaConfig.DiscordToken)
    .AddDiscordCaching()
    .AddDiscordCommands(enableSlash: true)
    .AddInteractivity()
    .AddCommandTree().WithCommandGroup<PingCommand>();
    // .AddCommandGroupsFromAssembly(typeof(Program).Assembly);

var host = builder.Build();

var slashService = host.Services.GetRequiredService<SlashService>();

if (goudaConfig.DebugServerId.Length > 0)
{
    foreach (var serverId in goudaConfig.DebugServerId)
    {
        if (builder.Environment.IsDevelopment())
        {
            var guildId = new Snowflake(serverId);
            await slashService.UpdateSlashCommandsAsync(guildId);
        }
        else
        {
            var appInfo = await host.Services.GetRequiredService<IDiscordRestOAuth2API>()
                .GetCurrentBotApplicationInformationAsync();

            var api = host.Services.GetRequiredService<IDiscordRestApplicationAPI>();
            await api.BulkOverwriteGuildApplicationCommandsAsync(appInfo.Entity.ID, new(serverId), []);
        }
    }
}
else
{
    await slashService.UpdateSlashCommandsAsync();
}

host.Run();
