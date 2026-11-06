using System.ComponentModel;

namespace LearnAI.Tools.Weather
{
    public class WeatherService
    {
        [Description("Retrieves meteorological parameters and climate telemetry forecasts for a location.")]
        public string GetForecast(
            [Description("The targeting geographical location city text identity name, e.g. Tokyo.")] string location,
            [Description("The target window array range size specification count.")] int daysForecast)
        {
            return $"Simulated forecast evaluation matching '{location}' area tracking for the upcoming {daysForecast} days.";
        }
    }
}