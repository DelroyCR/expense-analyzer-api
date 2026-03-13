using System.Text.Json;
using ExpenseAnalyzer.Api.Common;
using ExpenseAnalyzer.Application.Common.Exceptions;

namespace ExpenseAnalyzer.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = new ApiErrorResponse();
        context.Response.ContentType = "application/json";

        switch (exception)
        {
            case ValidationException:
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            response.StatusCode = StatusCodes.Status400BadRequest;
            response.Message = exception.Message;
            break;

            case ConflictException:
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            response.StatusCode = StatusCodes.Status409Conflict;
            response.Message = exception.Message;
            break;

            default:
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            response.StatusCode = StatusCodes.Status500InternalServerError;
            response.Message = "An unexpected eroor ocurred.";
            break;
        }

        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}