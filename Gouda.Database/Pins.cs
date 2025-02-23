using Microsoft.EntityFrameworkCore;

namespace Gouda.Database;

[PrimaryKey(nameof(UserId), nameof(Channel), nameof(Message))]
public class Pins
{
    public ulong UserId { get; set; }

    public ulong Channel { get; set; }

    public ulong Message { get; set; }
}
