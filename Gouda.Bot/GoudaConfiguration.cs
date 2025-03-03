namespace Gouda.Bot;

internal sealed class GoudaConfiguration
{
    public required string DiscordToken { get; set; }

    public ulong[]? DebugServerId { get; set; }
}
