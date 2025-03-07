using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gouda.Database;

public class GuildPin
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong Id { get; set; }

    public required string PinEmoji { get; set; }

    public int SuperpinLimit { get; set; } = 5;
}
