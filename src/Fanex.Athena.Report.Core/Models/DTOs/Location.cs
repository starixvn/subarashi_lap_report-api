namespace Fanex.Athena.Report.Core.Models.DTOs;

public class Location
{
    private string _countryCode;

    private string _countryName;

    public string CountryCode
    {
        get => string.IsNullOrEmpty(_countryCode) || _countryCode == "-" ? "N/A" : _countryCode;
        set => _countryCode = value;
    }

    public string CountryName
    {
        get => string.IsNullOrEmpty(_countryName) || _countryName == "-" ? "N/A" : _countryName;
        set => _countryName = value;
    }
}