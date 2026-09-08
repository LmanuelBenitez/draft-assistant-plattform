namespace backend.Services.Config;

public class FootballApiConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 2;
    public string LeagueId { get; set; } = string.Empty;  // Ej: "140" para La Liga
    public string Season { get; set; } = string.Empty;    // Ej: "2024"
}