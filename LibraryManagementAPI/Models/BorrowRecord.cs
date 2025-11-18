namespace LibraryManagementAPI.Models;

public class BorrowRecord
{
    public int Id { get; set; }
    public int BorrowerId { get; set; }
    public int BookId { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsOverdue { get; set; }
    public decimal FineAmount { get; set; }
    public string Status { get; set; } = "Active"; // Active, Returned, Overdue
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
