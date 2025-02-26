using System.ComponentModel.DataAnnotations;

namespace Gouda.Database;

public class GeonameAdmin1Codes
{
    [Key]
    public required string Code { get; set; }

    public required string Name { get; set; }

    public required string AsciiName { get; set; }

    public required ulong GeonameId { get; set; }
}
