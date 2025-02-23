using System.ComponentModel.DataAnnotations;

namespace Gouda.Database;

public class Locale
{
    [Key]
    public ulong UserId { get; set; }

    public required string LocaleName { get; set; }
}
