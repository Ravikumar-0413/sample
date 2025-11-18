using LibraryManagementAPI.Models;
using LibraryManagementAPI.Data;

namespace LibraryManagementAPI.Services;

public interface IBorrowerService
{
    Task<List<Borrower>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<Borrower?> GetByIdAsync(int id);
    Task<Borrower> AddAsync(Borrower borrower);
    Task<Borrower> UpdateAsync(int id, Borrower borrower);
    Task DeleteAsync(int id);
    Task<Borrower?> GetByMembershipIdAsync(string membershipId);
}

public class BorrowerService : IBorrowerService
{
    private readonly IJsonStorageService _storage;
    private readonly ILogger<BorrowerService> _logger;
    private const string FileName = "Borrowers";

    public BorrowerService(IJsonStorageService storage, ILogger<BorrowerService> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    public async Task<List<Borrower>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var borrowers = await _storage.LoadAsync<Borrower>(FileName);
            var totalCount = borrowers.Count;
            borrowers = borrowers.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            _logger.LogInformation($"Retrieved {borrowers.Count} borrowers");
            return borrowers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving borrowers");
            throw;
        }
    }

    public async Task<Borrower?> GetByIdAsync(int id)
    {
        try
        {
            return await _storage.GetByIdAsync<Borrower>(FileName, id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving borrower with ID {id}");
            throw;
        }
    }

    public async Task<Borrower> AddAsync(Borrower borrower)
    {
        try
        {
            var borrowers = await _storage.LoadAsync<Borrower>(FileName);
            
            // Check for duplicate membership ID
            if (borrowers.Any(b => b.MembershipId.Equals(borrower.MembershipId, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Membership ID {borrower.MembershipId} already exists");

            borrower.Id = borrowers.Any() ? borrowers.Max(b => b.Id) + 1 : 1;
            borrower.CreatedAt = DateTime.UtcNow;
            borrowers.Add(borrower);
            await _storage.SaveAsync(FileName, borrowers);
            _logger.LogInformation($"Borrower added with ID {borrower.Id}: {borrower.Name}");
            return borrower;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding borrower");
            throw;
        }
    }

    public async Task<Borrower> UpdateAsync(int id, Borrower borrower)
    {
        try
        {
            var borrowers = await _storage.LoadAsync<Borrower>(FileName);
            var existingBorrower = borrowers.FirstOrDefault(b => b.Id == id);
            
            if (existingBorrower == null)
                throw new KeyNotFoundException($"Borrower with ID {id} not found");

            borrower.Id = id;
            borrower.CreatedAt = existingBorrower.CreatedAt;
            borrower.UpdatedAt = DateTime.UtcNow;

            borrowers[borrowers.IndexOf(existingBorrower)] = borrower;
            await _storage.SaveAsync(FileName, borrowers);
            _logger.LogInformation($"Borrower updated with ID {id}");
            return borrower;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating borrower with ID {id}");
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            await _storage.DeleteAsync<Borrower>(FileName, id);
            _logger.LogInformation($"Borrower deleted with ID {id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting borrower with ID {id}");
            throw;
        }
    }

    public async Task<Borrower?> GetByMembershipIdAsync(string membershipId)
    {
        try
        {
            var borrowers = await _storage.LoadAsync<Borrower>(FileName);
            return borrowers.FirstOrDefault(b => b.MembershipId.Equals(membershipId, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving borrower with membership ID {membershipId}");
            throw;
        }
    }
}
