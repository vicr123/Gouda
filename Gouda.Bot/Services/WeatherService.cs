using OpenMeteoApi;
using OpenMeteoApi.Models;

namespace Gouda.Bot.Services;

public class WeatherService
{
    public async Task<List<HourlyForecastItem>> HourlyForecast(double latitude, double longitude, string timezone)
    {
        var client = new OpenMeteoClient
        {
            ForecastParameters = new()
            {
                { "timezone", timezone },
            },
        };
        return await client.GetHourlyForecasts(latitude, longitude);
    }

    public async Task<CurrentWeather> CurrentWeather(double latitude, double longitude, string timezone)
    {
        var client = new OpenMeteoClient
        {
            ForecastParameters = new()
            {
                { "timezone", timezone },
            },
        };
        return await client.GetCurrentWeather(latitude, longitude);
    }
}
