using ExpenseAnalyzer.Application.Common.Exceptions;
using ExpenseAnalyzer.Application.DTOs;
using ExpenseAnalyzer.Application.Interfaces;
using ExpenseAnalyzer.Application.Services;
using ExpenseAnalyzer.Domain.Entities;
using Moq;
using Xunit;

namespace ExpenseAnalyzer.UnitTests.Services;

public class TransactionServiceTests
{
    [Fact]
    public async Task GetTransactionByIdAsync_ShouldThrowValidationException_WhenPageNumberisLessThan1()
    {
        //Arrange
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var transactionRepositoryMock = new Mock<ITransactionRepository>();

        currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)Guid.NewGuid());

        var service = new TransactionService(
            currentUserServiceMock.Object,
            transactionRepositoryMock.Object);

        var filter = new TransactionFilterDto
        {
            PageNumber = 0,
            PageSize = 20

        };

        //Act
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.GetTransactionsAsync(filter));

        //Assert
        Assert.Equal("The page number must be greater than 0.", exception.Message);

        transactionRepositoryMock.Verify(
            x => x.GetPagedByUserIdAsync(It.IsAny<Guid>(), It.IsAny<TransactionFilterDto>()), Times.Never);
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldSetOutOfRangeMessage_WhenPageNumberExceedsTotalPages()
    {
        // Arrange
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var transactionRepositoryMock = new Mock<ITransactionRepository>();

        var userId = Guid.NewGuid();

        currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)userId);

        transactionRepositoryMock
            .Setup(x => x.GetPagedByUserIdAsync(userId, It.IsAny<TransactionFilterDto>()))
            .ReturnsAsync((new List<Transaction>(), 1));

        var service = new TransactionService(
            currentUserServiceMock.Object,
            transactionRepositoryMock.Object);

        var filter = new TransactionFilterDto
        {
            PageNumber = 2,
            PageSize = 20,
            SortBy = "date",
            SortDirection = "desc"
        };

        // Act
        var result = await service.GetTransactionsAsync(filter);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(1, result.TotalPages);
        Assert.True(result.IsPageOutOfRange);
        Assert.Equal(
            "There are no records on page 2. With the current filters, only 1 page(s) exist.",
            result.Message);

        transactionRepositoryMock.Verify(
            x => x.GetPagedByUserIdAsync(userId, It.IsAny<TransactionFilterDto>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldThrowValidationException_WhenSortByIsInvalid()
    {
        // Arrange
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var transactionRepositoryMock = new Mock<ITransactionRepository>();

        currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)Guid.NewGuid());

        var service = new TransactionService(
            currentUserServiceMock.Object,
            transactionRepositoryMock.Object);

        var filter = new TransactionFilterDto
        {
            PageNumber = 1,
            PageSize = 20,
            SortBy = "category",
            SortDirection = "desc"
        };

        // Act
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.GetTransactionsAsync(filter));

        // Assert
        Assert.Equal("SortBy must be either 'date' or 'amount'.", exception.Message);

        transactionRepositoryMock.Verify(
            x => x.GetPagedByUserIdAsync(It.IsAny<Guid>(), It.IsAny<TransactionFilterDto>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldThrowValidationException_WhenSortDirectionIsInvalid()
    {
        // Arrange
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var transactionRepositoryMock = new Mock<ITransactionRepository>();

        currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)Guid.NewGuid());

        var service = new TransactionService(
            currentUserServiceMock.Object,
            transactionRepositoryMock.Object);

        var filter = new TransactionFilterDto
        {
            PageNumber = 1,
            PageSize = 20,
            SortBy = "date",
            SortDirection = "up"
        };

        // Act
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => service.GetTransactionsAsync(filter));

        // Assert
        Assert.Equal("SortDirection must be either 'asc' or 'desc'.", exception.Message);

        transactionRepositoryMock.Verify(
            x => x.GetPagedByUserIdAsync(It.IsAny<Guid>(), It.IsAny<TransactionFilterDto>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldSetMessage_WhenNoTransactionsAreFound()
    {
        // Arrange
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var transactionRepositoryMock = new Mock<ITransactionRepository>();

        var userId = Guid.NewGuid();

        currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)userId);

        transactionRepositoryMock
            .Setup(x => x.GetPagedByUserIdAsync(userId, It.IsAny<TransactionFilterDto>()))
            .ReturnsAsync((new List<Transaction>(), 0));

        var service = new TransactionService(
            currentUserServiceMock.Object,
            transactionRepositoryMock.Object);

        var filter = new TransactionFilterDto
        {
            PageNumber = 1,
            PageSize = 20,
            SortBy = "date",
            SortDirection = "desc"
        };

        // Act
        var result = await service.GetTransactionsAsync(filter);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        Assert.Equal(0, result.TotalPages);
        Assert.False(result.IsPageOutOfRange);
        Assert.Equal("No transactions were found for the provided filters.", result.Message);

        transactionRepositoryMock.Verify(
            x => x.GetPagedByUserIdAsync(userId, It.IsAny<TransactionFilterDto>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTransactionsAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var transactionRepositoryMock = new Mock<ITransactionRepository>();

        currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)null);

        var service = new TransactionService(
            currentUserServiceMock.Object,
            transactionRepositoryMock.Object);

        var filter = new TransactionFilterDto
        {
            PageNumber = 1,
            PageSize = 20,
            SortBy = "date",
            SortDirection = "desc"
        };

        // Act
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.GetTransactionsAsync(filter));

        // Assert
        Assert.Equal("User is not authenticated.", exception.Message);

        transactionRepositoryMock.Verify(
            x => x.GetPagedByUserIdAsync(It.IsAny<Guid>(), It.IsAny<TransactionFilterDto>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTransactionSummaryAsync_ShouldReturnZeros_WhenNoTransactionsAreFound()
    {
        // Arrange
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var transactionRepositoryMock = new Mock<ITransactionRepository>();

        var userId = Guid.NewGuid();

        currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)userId);

        transactionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<TransactionFilterDto>()))
            .ReturnsAsync(new List<Transaction>());

        var service = new TransactionService(
            currentUserServiceMock.Object,
            transactionRepositoryMock.Object);

        var filter = new TransactionFilterDto
        {
            From = null,
            To = null,
            MinAmount = null,
            MaxAmount = null,
            Description = null,
            ImportJobId = null
        };

        // Act
        var result = await service.GetTransactionSummaryAsync(filter);

        // Assert
        Assert.Equal(0, result.TotalTransactions);
        Assert.Equal(0, result.TotalAmount);
        Assert.Equal(0, result.AverageAmount);
        Assert.Equal(0, result.HighestAmount);
        Assert.Equal(0, result.LowestAmount);

        transactionRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<TransactionFilterDto>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_ShouldThrowKeyNotFoundException_WhenTransactionDoesNotExist()
    {
        // Arrange
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var transactionRepositoryMock = new Mock<ITransactionRepository>();

        var userId = Guid.NewGuid();
        var transactionId = Guid.NewGuid();

        currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)userId);

        transactionRepositoryMock
            .Setup(x => x.GetByIdAsync(transactionId, userId))
            .ReturnsAsync((Transaction?)null);

        var service = new TransactionService(
            currentUserServiceMock.Object,
            transactionRepositoryMock.Object);

        // Act
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.GetTransactionByIdAsync(transactionId));

        // Assert
        Assert.Equal("Transaction not found.", exception.Message);

        transactionRepositoryMock.Verify(
            x => x.GetByIdAsync(transactionId, userId),
            Times.Once);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var transactionRepositoryMock = new Mock<ITransactionRepository>();

        var transactionId = Guid.NewGuid();

        currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)null);

        var service = new TransactionService(
            currentUserServiceMock.Object,
            transactionRepositoryMock.Object);

        // Act
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.GetTransactionByIdAsync(transactionId));

        // Assert
        Assert.Equal("User is not authenticated.", exception.Message);

        transactionRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task GetTransactionSummaryAsync_ShouldReturnCorrectSummary_WhenTransactionsExist()
    {
        // Arrange
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        var transactionRepositoryMock = new Mock<ITransactionRepository>();

        var userId = Guid.NewGuid();

        currentUserServiceMock
            .Setup(x => x.UserId)
            .Returns((Guid?)userId);

        var transactions = new List<Transaction>
        {
            new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ImportJobId = Guid.NewGuid(),
                Date = new DateTime(2026, 1, 10),
                Description = "Amazon",
                Amount = 100m,
                CreatedAtUtc = DateTime.UtcNow
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ImportJobId = Guid.NewGuid(),
                Date = new DateTime(2026, 1, 11),
                Description = "Uber",
                Amount = 50m,
                CreatedAtUtc = DateTime.UtcNow
            },
            new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ImportJobId = Guid.NewGuid(),
                Date = new DateTime(2026, 1, 12),
                Description = "Salary",
                Amount = 200m,
                CreatedAtUtc = DateTime.UtcNow
            }
        };

        transactionRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<TransactionFilterDto>()))
            .ReturnsAsync(transactions);

        var service = new TransactionService(
            currentUserServiceMock.Object,
            transactionRepositoryMock.Object);

        var filter = new TransactionFilterDto();

        // Act
        var result = await service.GetTransactionSummaryAsync(filter);

        // Assert
        Assert.Equal(3, result.TotalTransactions);
        Assert.Equal(350m, result.TotalAmount);
        Assert.Equal(116.67m, Math.Round(result.AverageAmount, 2));
        Assert.Equal(200m, result.HighestAmount);
        Assert.Equal(50m, result.LowestAmount);

        transactionRepositoryMock.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<TransactionFilterDto>()),
            Times.Once);
    }
}