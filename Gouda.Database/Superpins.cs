using Microsoft.EntityFrameworkCore;

namespace Gouda.Database;

[PrimaryKey(nameof(Channel), nameof(Message))]
public class Superpins
{
    public ulong Channel { get; set; }

    public ulong Message { get; set; }

    public ulong SuperpinMessageChannel { get; set; }

    public ulong SuperpinMessage { get; set; }
}