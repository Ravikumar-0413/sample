using LibraryManagementAPI.Models;
using LibraryManagementAPI.Data;

namespace LibraryManagementAPI.Services;

public interface IBookService
{
    Task<List<Book>> GetAllAsync(string? searchTerm = null, string? genre = null, int page = 1, int pageSize = 10);
    Task<Book?> GetByIdAsync(int id);
    Task<Book> AddAsync(Book book);
    Task<Book> UpdateAsync(int id, Book book);
    Task DeleteAsync(int id);
    Task<Book?> GetByISBNAsync(string isbn);
}

public class BookService : IBookService
{
    private readonly IJsonStorageService _storage;
    private readonly ILogger<BookService> _logger;
    private const string FileName = "Books";

    public BookService(IJsonStorageService storage, ILogger<BookService> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    public async Task<List<Book>> GetAllAsync(string? searchTerm = null, string? genre = null, int page = 1, int pageSize = 10)
    {
        try
        {
            var books = await _storage.LoadAsync<Book>(FileName);
            
            // Filter
            if (!string.IsNullOrEmpty(searchTerm))
                books = books.Where(b => 
                    b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                    b.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (!string.IsNullOrEmpty(genre))
                books = books.Where(b => b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase)).ToList();

            // Pagination
            var totalCount = books.Count;
            books = books.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            _logger.LogInformation($"Retrieved {books.Count} books with filters: searchTerm={searchTerm}, genre={genre}");
            return books;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books");
            throw;
        }
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        try
        {
            return await _storage.GetByIdAsync<Book>(FileName, id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving book with ID {id}");
            throw;
        }
    }

    public async Task<Book> AddAsync(Book book)
    {
        try
        {
            var books = await _storage.LoadAsync<Book>(FileName);
            book.Id = books.Any() ? books.Max(b => b.Id) + 1 : 1;
            book.CreatedAt = DateTime.UtcNow;
            books.Add(book);
            await _storage.SaveAsync(FileName, books);
            _logger.LogInformation($"Book added with ID {book.Id}: {book.Title}");
            return book;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding book");
            throw;
        }
    }

    public async Task<Book> UpdateAsync(int id, Book book)
    {
        try
        {
            var books = await _storage.LoadAsync<Book>(FileName);
            var existingBook = books.FirstOrDefault(b => b.Id == id);
            
            if (existingBook == null)
                throw new KeyNotFoundException($"Book with ID {id} not found");

            book.Id = id;
            book.CreatedAt = existingBook.CreatedAt;
            book.UpdatedAt = DateTime.UtcNow;

            books[books.IndexOf(existingBook)] = book;
            await _storage.SaveAsync(FileName, books);
            _logger.LogInformation($"Book updated with ID {id}");
            return book;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating book with ID {id}");
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        try
        {
            await _storage.DeleteAsync<Book>(FileName, id);
            _logger.LogInformation($"Book deleted with ID {id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting book with ID {id}");
            throw;
        }
    }

    public async Task<Book?> GetByISBNAsync(string isbn)
    {
        try
        {
            var books = await _storage.LoadAsync<Book>(FileName);
            return books.FirstOrDefault(b => b.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving book with ISBN {isbn}");
            throw;
        }
    }
}
