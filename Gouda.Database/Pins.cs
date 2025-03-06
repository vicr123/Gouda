using Microsoft.EntityFrameworkCore;

namespace Gouda.Database;

[PrimaryKey(nameof(UserId), nameof(Channel), nameof(Message))]
[Index(nameof(UserId), nameof(PinNumber), IsUnique = true)]
public class Pins
{
    public ulong UserId { get; set; }

    public ulong Channel { get; set; }

    public ulong Message { get; set; }

    public ulong PinNumber { get; set; }
}
