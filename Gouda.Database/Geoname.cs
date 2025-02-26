using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Gouda.Database;

[Index(nameof(AsciiName), IsUnique = false)]
public class Geoname
{
    [Key]
    public required ulong Id { get; set; }

    public required string Name { get; set; }

    public required string AsciiName { get; set; }

    public required double Latitude { get; set; }

    public required double Longitude { get; set; }

    public required char FeatureClass { get; set; }

    public required string FeatureCode { get; set; }

    public required string CountryCode { get; set; }

    public required string Admin1Code { get; set; }

    public required string Admin2Code { get; set; }

    public required string Admin3Code { get; set; }

    public required string Admin4Code { get; set; }

    public required ulong Population { get; set; }

    public required string Timezone { get; set; }
}
