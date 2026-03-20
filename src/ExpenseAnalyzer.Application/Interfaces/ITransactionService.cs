using ExpenseAnalyzer.Application.DTOs;

namespace ExpenseAnalyzer.Application.Interfaces;

public interface ITransactionService
{
    Task<IReadOnlyList<TransactionSummaryDto>> GetTransactionsAsync(TransactionFilterDto filter);
    Task<TransactionDetailDto> GetTransactionByIdAsync(Guid transactionId);
    Task<TransactionSummaryStatsDto> GetTransactionSummaryAsync(TransactionFilterDto filter);
}