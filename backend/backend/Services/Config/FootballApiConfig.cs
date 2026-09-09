namespace backend.Services.Config;

public class FootballApiConfig
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 2;
}