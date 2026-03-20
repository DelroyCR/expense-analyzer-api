using ExpenseAnalyzer.Application.DTOs;
using ExpenseAnalyzer.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseAnalyzer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TransactionSummaryDto>>> GetTransactions([FromQuery] TransactionFilterDto filter)
    {
        var result = await _transactionService.GetTransactionsAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionDetailDto>> GetTransactionById(Guid id)
    {
        var result = await _transactionService.GetTransactionByIdAsync(id);
        return Ok(result);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<TransactionSummaryStatsDto>> GetTransactionSummary([FromQuery] TransactionFilterDto filter)
    {
        var result = await _transactionService.GetTransactionSummaryAsync(filter);
        return Ok(result);
    }
}