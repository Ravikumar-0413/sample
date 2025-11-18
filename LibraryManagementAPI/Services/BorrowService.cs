using LibraryManagementAPI.Models;
using LibraryManagementAPI.Data;

namespace LibraryManagementAPI.Services;

public interface IBorrowService
{
    Task<List<BorrowRecord>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<BorrowRecord?> GetByIdAsync(int id);
    Task<BorrowRecord> BorrowBookAsync(int borrowerId, int bookId, int days);
    Task<BorrowRecord> ReturnBookAsync(int borrowerId, int bookId);
    Task<List<BorrowRecord>> GetActiveBorrowsAsync(int borrowerId);
    Task<List<BorrowRecord>> GetOverdueAsync();
}

public class BorrowService : IBorrowService
{
    private readonly IJsonStorageService _storage;
    private readonly IBookService _bookService;
    private readonly IBorrowerService _borrowerService;
    private readonly ILogger<BorrowService> _logger;
    private const string FileName = "BorrowRecords";
    private const decimal FineDayRate = 10m; // Rs 10 per day

    public BorrowService(
        IJsonStorageService storage,
        IBookService bookService,
        IBorrowerService borrowerService,
        ILogger<BorrowService> logger)
    {
        _storage = storage;
        _bookService = bookService;
        _borrowerService = borrowerService;
        _logger = logger;
    }

    public async Task<List<BorrowRecord>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var records = await _storage.LoadAsync<BorrowRecord>(FileName);
            var totalCount = records.Count;
            records = records.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            _logger.LogInformation($"Retrieved {records.Count} borrow records");
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving borrow records");
            throw;
        }
    }

    public async Task<BorrowRecord?> GetByIdAsync(int id)
    {
        try
        {
            return await _storage.GetByIdAsync<BorrowRecord>(FileName, id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving borrow record with ID {id}");
            throw;
        }
    }

    public async Task<BorrowRecord> BorrowBookAsync(int borrowerId, int bookId, int days)
    {
        try
        {
            var book = await _bookService.GetByIdAsync(bookId);
            if (book == null)
                throw new KeyNotFoundException($"Book with ID {bookId} not found");

            if (book.Quantity <= 0)
                throw new InvalidOperationException($"Book '{book.Title}' is not available");

            var borrower = await _borrowerService.GetByIdAsync(borrowerId);
            if (borrower == null)
                throw new KeyNotFoundException($"Borrower with ID {borrowerId} not found");

            if (DateTime.UtcNow > borrower.MembershipExpiryDate)
                throw new InvalidOperationException($"Borrower's membership has expired");

            // Create borrow record
            var records = await _storage.LoadAsync<BorrowRecord>(FileName);
            var record = new BorrowRecord
            {
                Id = records.Any() ? records.Max(r => r.Id) + 1 : 1,
                BorrowerId = borrowerId,
                BookId = bookId,
                BorrowDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(days),
                Status = "Active"
            };

            records.Add(record);
            await _storage.SaveAsync(FileName, records);

            // Decrease book quantity
            book.Quantity--;
            await _bookService.UpdateAsync(bookId, book);

            _logger.LogInformation($"Book borrowed: BorrowerId={borrowerId}, BookId={bookId}, DueDate={record.DueDate}");
            return record;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error borrowing book");
            throw;
        }
    }

    public async Task<BorrowRecord> ReturnBookAsync(int borrowerId, int bookId)
    {
        try
        {
            var records = await _storage.LoadAsync<BorrowRecord>(FileName);
            var record = records.FirstOrDefault(r => 
                r.BorrowerId == borrowerId && 
                r.BookId == bookId && 
                r.Status == "Active");

            if (record == null)
                throw new KeyNotFoundException("No active borrow record found");

            // Check for overdue
            record.ReturnDate = DateTime.UtcNow;
            record.IsOverdue = record.ReturnDate > record.DueDate;

            if (record.IsOverdue)
            {
                var daysOverdue = (int)(record.ReturnDate.Value - record.DueDate).TotalDays;
                record.FineAmount = daysOverdue * FineDayRate;
                record.Status = "Overdue";
            }
            else
            {
                record.Status = "Returned";
            }

            records[records.IndexOf(record)] = record;
            await _storage.SaveAsync(FileName, records);

            // Increase book quantity
            var book = await _bookService.GetByIdAsync(bookId);
            if (book != null)
            {
                book.Quantity++;
                await _bookService.UpdateAsync(bookId, book);
            }

            _logger.LogInformation($"Book returned: BorrowerId={borrowerId}, BookId={bookId}, Fine={record.FineAmount}");
            return record;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error returning book");
            throw;
        }
    }

    public async Task<List<BorrowRecord>> GetActiveBorrowsAsync(int borrowerId)
    {
        try
        {
            var records = await _storage.LoadAsync<BorrowRecord>(FileName);
            return records.Where(r => r.BorrowerId == borrowerId && r.Status == "Active").ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving active borrows for borrower {borrowerId}");
            throw;
        }
    }

    public async Task<List<BorrowRecord>> GetOverdueAsync()
    {
        try
        {
            var records = await _storage.LoadAsync<BorrowRecord>(FileName);
            return records.Where(r => 
                r.Status == "Active" && 
                DateTime.UtcNow > r.DueDate).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue records");
            throw;
        }
    }
}
