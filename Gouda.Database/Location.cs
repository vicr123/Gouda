using System.ComponentModel.DataAnnotations;

namespace Gouda.Database;

public class Location
{
    [Key]
    public ulong UserId { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}
