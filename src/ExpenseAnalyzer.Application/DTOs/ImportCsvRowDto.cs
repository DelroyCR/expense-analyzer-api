namespace ExpenseAnalyzer.Application.DTOs;

public class ImportCsvRowDto{
    public string Date { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
}