using Microsoft.AspNetCore.Mvc;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Services;

namespace LibraryManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IBookService bookService, ILogger<BooksController> logger)
    {
        _bookService = bookService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Book>>> GetAll(
        [FromQuery] string? searchTerm,
        [FromQuery] string? genre,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var books = await _bookService.GetAllAsync(searchTerm, genre, page, pageSize);
            return Ok(books);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Book>> GetById(int id)
    {
        try
        {
            var book = await _bookService.GetByIdAsync(id);
            if (book == null)
                return NotFound(new { message = $"Book with ID {id} not found" });

            return Ok(book);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving book {id}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Book>> Add([FromBody] Book book)
    {
        try
        {
            if (string.IsNullOrEmpty(book.Title) || string.IsNullOrEmpty(book.ISBN))
                return BadRequest(new { message = "Title and ISBN are required" });

            var createdBook = await _bookService.AddAsync(book);
            return CreatedAtAction(nameof(GetById), new { id = createdBook.Id }, createdBook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding book");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Book>> Update(int id, [FromBody] Book book)
    {
        try
        {
            var updatedBook = await _bookService.UpdateAsync(id, book);
            return Ok(updatedBook);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating book {id}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var book = await _bookService.GetByIdAsync(id);
            if (book == null)
                return NotFound(new { message = $"Book with ID {id} not found" });

            await _bookService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting book {id}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("by-isbn/{isbn}")]
    public async Task<ActionResult<Book>> GetByISBN(string isbn)
    {
        try
        {
            var book = await _bookService.GetByISBNAsync(isbn);
            if (book == null)
                return NotFound(new { message = $"Book with ISBN {isbn} not found" });

            return Ok(book);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving book by ISBN {isbn}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
