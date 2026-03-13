namespace ExpenseAnalyzer.Api.Common;

public class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
}