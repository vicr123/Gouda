namespace Gouda.Bot;

public sealed class GoudaConfiguration
{
    public required string DiscordToken { get; set; }

    public ulong[]? DebugServerId { get; set; }

    public required string ConfigurationUrl { get; set; }
}
