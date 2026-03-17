namespace ExpenseAnalyzer.Application.DTOs;

public sealed class ImportCsvRowErrorDto
{
    public int RowNumber { get; init; }
    public string RawLine { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}