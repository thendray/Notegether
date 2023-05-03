namespace Notegether;

public record Settings()
{
    public string Token { get; set; } = string.Empty;
    public string DataBasePath { get; set; } = string.Empty;
}