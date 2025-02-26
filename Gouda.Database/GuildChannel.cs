using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Gouda.Database;

public class GuildChannel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong Id { get; set; }

    public ulong? AlertChannel { get; set; }

    public ulong? ChatLogChannel { get; set; }

    public ulong? SuperpinChannel { get; set; }
}
