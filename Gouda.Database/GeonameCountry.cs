using System.ComponentModel.DataAnnotations;

namespace Gouda.Database;

public class GeonameCountry
{
    [Key]
    public ulong Id { get; set; }

    public string IsoCode { get; set; }

    public string Iso3Code { get; set; }

    public string IsoNumericCode { get; set; }

    public string ContinentCode { get; set; }

    public string Tld { get; set; }

    public string CurrencyCode { get; set; }

    public string CurrencyName { get; set; }

    public string TelephoneCode { get; set; }

    public string Languages { get; set; }
}