using Microsoft.AspNetCore.Mvc;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Services;

namespace LibraryManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BorrowsController : ControllerBase
{
    private readonly IBorrowService _borrowService;
    private readonly ILogger<BorrowsController> _logger;

    public BorrowsController(IBorrowService borrowService, ILogger<BorrowsController> logger)
    {
        _borrowService = borrowService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<BorrowRecord>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var records = await _borrowService.GetAllAsync(page, pageSize);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving borrow records");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost("borrow")]
    public async Task<ActionResult<BorrowRecord>> Borrow([FromBody] BorrowRequest request)
    {
        try
        {
            if (request.BorrowerId <= 0 || request.BookId <= 0 || request.Days <= 0)
                return BadRequest(new { message = "Invalid borrowerId, bookId, or days" });

            var record = await _borrowService.BorrowBookAsync(request.BorrowerId, request.BookId, request.Days);
            return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error borrowing book");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost("return")]
    public async Task<ActionResult<BorrowRecord>> Return([FromBody] ReturnRequest request)
    {
        try
        {
            if (request.BorrowerId <= 0 || request.BookId <= 0)
                return BadRequest(new { message = "Invalid borrowerId or bookId" });

            var record = await _borrowService.ReturnBookAsync(request.BorrowerId, request.BookId);
            return Ok(record);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error returning book");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BorrowRecord>> GetById(int id)
    {
        try
        {
            var record = await _borrowService.GetByIdAsync(id);
            if (record == null)
                return NotFound(new { message = $"Borrow record with ID {id} not found" });

            return Ok(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving borrow record {id}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("borrower/{borrowerId}")]
    public async Task<ActionResult<List<BorrowRecord>>> GetActiveBorrows(int borrowerId)
    {
        try
        {
            var records = await _borrowService.GetActiveBorrowsAsync(borrowerId);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving active borrows for borrower {borrowerId}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("overdue/list")]
    public async Task<ActionResult<List<BorrowRecord>>> GetOverdue()
    {
        try
        {
            var records = await _borrowService.GetOverdueAsync();
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving overdue records");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}

public class BorrowRequest
{
    public int BorrowerId { get; set; }
    public int BookId { get; set; }
    public int Days { get; set; }
}

public class ReturnRequest
{
    public int BorrowerId { get; set; }
    public int BookId { get; set; }
}
