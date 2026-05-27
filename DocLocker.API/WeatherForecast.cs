namespace DocLocker.API
{
    // Simple weather forecast model for sample endpoint.
    public class WeatherForecast
    {
        // Date of the forecast.
        public DateOnly Date { get; set; }

        // Temperature in Celsius.
        public int TemperatureC { get; set; }

        // Temperature in Fahrenheit.
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        // Short summary of the weather.
        public string? Summary { get; set; }
    }
}
