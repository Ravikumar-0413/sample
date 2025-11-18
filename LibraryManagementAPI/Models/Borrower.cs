namespace LibraryManagementAPI.Models;

public class Borrower
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MembershipId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime MembershipStartDate { get; set; }
    public DateTime MembershipExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
