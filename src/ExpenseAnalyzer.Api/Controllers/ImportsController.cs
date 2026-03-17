using ExpenseAnalyzer.Api.Contracts.Imports;
using ExpenseAnalyzer.Application.DTOs;
using ExpenseAnalyzer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseAnalyzer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImportsController : ControllerBase
{
    private readonly IImportService _importService;

    public ImportsController(IImportService importService)
    {
        _importService = importService;
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ImportCsvResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ImportCsvResponseDto>> ImportCsv([FromForm] ImportCsvRequest request)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest("A CSV file is required.");
        }

        using var stream = request.File.OpenReadStream();
        var response = await _importService.ImportCsvAsync(stream, request.File.FileName);

        return Ok(response);
    }
}