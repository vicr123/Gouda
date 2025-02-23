namespace Gouda.Bot;

internal sealed class GoudaConfiguration
{
    public required string DiscordToken { get; set; }
    public required ulong[] DebugServerId { get; set; }
}