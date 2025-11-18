namespace LibraryManagementAPI.Models;

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime PublishedDate { get; set; }
    public string Publisher { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string ShelfLocation { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
