using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;

namespace Gouda.Bot.Services;

public class ThreadPresenceService(IDiscordRestGuildAPI guildApi, IDiscordRestUserAPI userApi, IDiscordRestChannelAPI channelApi)
{
    public async Task ProcessGuild(IPartialGuild guild, CancellationToken ct = default)
    {
        var activeThreads = await guildApi.ListActiveGuildThreadsAsync(guild.ID.Value, ct);
        if (activeThreads.IsSuccess)
        {
            await JoinAllThreads(activeThreads.Entity.Threads, ct);
        }

        var guildChannels = await guildApi.GetGuildChannelsAsync(guild.ID.Value, ct);
        if (!guildChannels.IsSuccess)
        {
            return;
        }

        foreach (var guildChannel in guildChannels.Entity)
        {
            if (guildChannel.Type is not (ChannelType.GuildText or ChannelType.GuildForum))
            {
                continue;
            }

            var publicThreads = await channelApi.ListPublicArchivedThreadsAsync(guildChannel.ID, ct: ct);
            if (publicThreads.IsSuccess)
            {
                await JoinAllThreads(publicThreads.Entity.Threads, ct);
            }

            var privateThreads = await channelApi.ListPrivateArchivedThreadsAsync(guildChannel.ID, ct: ct);
            if (privateThreads.IsSuccess)
            {
                await JoinAllThreads(privateThreads.Entity.Threads, ct);
            }
        }
    }

    private async Task JoinAllThreads(IEnumerable<IChannel> channels, CancellationToken ct)
    {
        foreach (var threadChannel in channels)
        {
            await JoinThread(threadChannel, ct);
        }
    }

    private async Task JoinThread(IChannel channel, CancellationToken ct)
    {
        if (channel.Type is not (ChannelType.PrivateThread or ChannelType.PublicThread))
        {
            return;
        }

        var thisUser = await userApi.GetCurrentUserAsync(ct);
        var result = await channelApi.GetThreadMemberAsync(channel.ID, thisUser.Entity.ID, withMember: false, ct);
        if (!result.IsSuccess)
        {
            await channelApi.JoinThreadAsync(channel.ID, ct);
        }
    }
}
