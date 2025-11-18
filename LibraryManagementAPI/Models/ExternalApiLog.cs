namespace LibraryManagementAPI.Models;

public class ExternalApiLog
{
    public int Id { get; set; }
    public string ApiName { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string RequestData { get; set; } = string.Empty;
    public string ResponseData { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public long ResponseTimeMs { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
