using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Gouda.Database;

[Index(nameof(AlternateName), IsUnique = false)]
public class GeonameAlternateNames
{
    [Key]
    public required ulong Id { get; set; }

    public required ulong GeonameId { get; set; }

    public required string Language { get; set; }

    public required string AlternateName { get; set; }

    public required bool IsPreferred { get; set; }

    public required bool IsShort { get; set; }

    public required bool IsColloquial { get; set; }

    public required bool IsHistoric { get; set; }
}
