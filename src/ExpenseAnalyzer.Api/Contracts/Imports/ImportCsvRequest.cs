using Microsoft.AspNetCore.Http;

namespace ExpenseAnalyzer.Api.Contracts.Imports;

public class ImportCsvRequest
{
    public IFormFile File { get; set; } = null!;
}