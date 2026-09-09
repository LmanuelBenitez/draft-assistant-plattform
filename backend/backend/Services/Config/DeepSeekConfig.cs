namespace backend.Services.Config
{
    public class DeepSeekConfig
    {
        public required string ApiKey { get; set; }
        public string BaseUrl { get; set; } = "https://api.deepseek.com/v1";
        public string Model { get; set; } = "deepseek-chat";
        public int MaxTokens { get; set; } = 500;
        public double Temperature { get; set; } = 0.7;
        public double TopP { get; set; } = 0.95;
        public int TimeoutSeconds { get; set; } = 30;
        public int MaxRetries { get; set; } = 3;
    }
}
