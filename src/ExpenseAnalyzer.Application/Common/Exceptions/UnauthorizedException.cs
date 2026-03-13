namespace ExpenseAnalyzer.Application.Common.Exceptions;

public class UnauthorizedException : Exception 
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}