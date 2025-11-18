using Microsoft.AspNetCore.Mvc;
using LibraryManagementAPI.Models;
using LibraryManagementAPI.Services;

namespace LibraryManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BorrowersController : ControllerBase
{
    private readonly IBorrowerService _borrowerService;
    private readonly ILogger<BorrowersController> _logger;

    public BorrowersController(IBorrowerService borrowerService, ILogger<BorrowersController> logger)
    {
        _borrowerService = borrowerService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<Borrower>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var borrowers = await _borrowerService.GetAllAsync(page, pageSize);
            return Ok(borrowers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving borrowers");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Borrower>> GetById(int id)
    {
        try
        {
            var borrower = await _borrowerService.GetByIdAsync(id);
            if (borrower == null)
                return NotFound(new { message = $"Borrower with ID {id} not found" });

            return Ok(borrower);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving borrower {id}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Borrower>> Add([FromBody] Borrower borrower)
    {
        try
        {
            if (string.IsNullOrEmpty(borrower.Name) || string.IsNullOrEmpty(borrower.MembershipId))
                return BadRequest(new { message = "Name and MembershipId are required" });

            var createdBorrower = await _borrowerService.AddAsync(borrower);
            return CreatedAtAction(nameof(GetById), new { id = createdBorrower.Id }, createdBorrower);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding borrower");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Borrower>> Update(int id, [FromBody] Borrower borrower)
    {
        try
        {
            var updatedBorrower = await _borrowerService.UpdateAsync(id, borrower);
            return Ok(updatedBorrower);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating borrower {id}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var borrower = await _borrowerService.GetByIdAsync(id);
            if (borrower == null)
                return NotFound(new { message = $"Borrower with ID {id} not found" });

            await _borrowerService.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting borrower {id}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("by-membership/{membershipId}")]
    public async Task<ActionResult<Borrower>> GetByMembershipId(string membershipId)
    {
        try
        {
            var borrower = await _borrowerService.GetByMembershipIdAsync(membershipId);
            if (borrower == null)
                return NotFound(new { message = $"Borrower with membership ID {membershipId} not found" });

            return Ok(borrower);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving borrower by membership ID {membershipId}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
