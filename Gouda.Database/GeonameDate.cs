using System.ComponentModel.DataAnnotations;

namespace Gouda.Database;

public class GeonameDate
{
    [Key]
    public ulong Date { get; set; }
}
