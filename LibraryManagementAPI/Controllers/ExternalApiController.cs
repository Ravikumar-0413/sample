using Microsoft.AspNetCore.Mvc;
using LibraryManagementAPI.Services;

namespace LibraryManagementAPI.Controllers;

[ApiController]
[Route("api/external")]
public class ExternalApiController : ControllerBase
{
    private readonly IExternalApiService _externalApiService;
    private readonly ILogger<ExternalApiController> _logger;

    public ExternalApiController(IExternalApiService externalApiService, ILogger<ExternalApiController> logger)
    {
        _externalApiService = externalApiService;
        _logger = logger;
    }

    [HttpGet("bookinfo/{isbn}")]
    public async Task<IActionResult> GetBookInfo(string isbn)
    {
        try
        {
            if (string.IsNullOrEmpty(isbn))
                return BadRequest(new { message = "ISBN is required" });

            var bookInfo = await _externalApiService.GetBookInfoAsync(isbn);
            if (bookInfo == null)
                return NotFound(new { message = $"No book info found for ISBN {isbn}" });

            return Ok(bookInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving book info for ISBN {isbn}");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetApiLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var logs = await _externalApiService.GetApiLogsAsync(page, pageSize);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving API logs");
            return StatusCode(500, new { message = "Internal server error", error = ex.Message });
        }
    }
}
