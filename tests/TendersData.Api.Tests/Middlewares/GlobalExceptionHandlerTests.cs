using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace TendersData.Api.Tests.Middlewares;

public class GlobalExceptionHandlerTests : GlobalExceptionHandlerMockHelper
{
    [Fact]
    public async Task TryHandleAsync_WithGenericException_Returns500StatusCode()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var handler = CreateHandler(Environments.Production);

        // Act
        var result = await handler.TryHandleAsync(HttpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        HttpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task TryHandleAsync_WithGenericException_LogsError()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var handler = CreateHandler(Environments.Production);

        // Act
        await handler.TryHandleAsync(HttpContext, exception, CancellationToken.None);

        // Assert
        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An unhandled exception occurred")),
                It.Is<Exception>(e => e == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task TryHandleAsync_WithGenericException_ReturnsProblemDetails()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var handler = CreateHandler(Environments.Production);
        HttpContext.Response.Body = new MemoryStream();

        // Act
        await handler.TryHandleAsync(HttpContext, exception, CancellationToken.None);

        // Assert
        HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(HttpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be((int)HttpStatusCode.InternalServerError);
        problemDetails.Title.Should().Be("Unexpected server error.");
        problemDetails.Detail.Should().Be("Test exception");
    }

    [Fact]
    public async Task TryHandleAsync_InDevelopment_ReturnsFullExceptionDetails()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var handler = CreateHandler(Environments.Development);
        HttpContext.Response.Body = new MemoryStream();

        // Act
        await handler.TryHandleAsync(HttpContext, exception, CancellationToken.None);

        // Assert
        HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(HttpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Contain("System.Exception");
        problemDetails.Detail.Should().Contain("Test exception");
    }

    [Fact]
    public async Task TryHandleAsync_InProduction_ReturnsOnlyExceptionMessage()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var handler = CreateHandler(Environments.Production);
        HttpContext.Response.Body = new MemoryStream();

        // Act
        await handler.TryHandleAsync(HttpContext, exception, CancellationToken.None);

        // Assert
        HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(HttpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be("Test exception");
        problemDetails.Detail.Should().NotContain("System.Exception");
    }

    [Fact]
    public async Task TryHandleAsync_SetsInstanceToRequestPath()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var handler = CreateHandler(Environments.Production);
        HttpContext.Request.Path = "/api/test";
        HttpContext.Response.Body = new MemoryStream();

        // Act
        await handler.TryHandleAsync(HttpContext, exception, CancellationToken.None);

        // Assert
        HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(HttpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        problemDetails.Should().NotBeNull();
        problemDetails!.Instance.Should().Be("/api/test");
    }

    [Fact]
    public async Task TryHandleAsync_SetsContentTypeToApplicationJson()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var handler = CreateHandler(Environments.Production);

        // Act
        await handler.TryHandleAsync(HttpContext, exception, CancellationToken.None);

        // Assert
        HttpContext.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task TryHandleAsync_WithNullExceptionMessage_HandlesGracefully()
    {
        // Arrange
        var exception = new Exception();
        var handler = CreateHandler(Environments.Production);
        HttpContext.Response.Body = new MemoryStream();

        // Act
        var result = await handler.TryHandleAsync(HttpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        HttpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task TryHandleAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var handler = CreateHandler(Environments.Production);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await handler.TryHandleAsync(HttpContext, exception, cts.Token));
    }

    [Fact]
    public async Task TryHandleAsync_WithInnerException_LogsCorrectly()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner exception");
        var exception = new Exception("Outer exception", innerException);
        var handler = CreateHandler(Environments.Production);

        // Act
        await handler.TryHandleAsync(HttpContext, exception, CancellationToken.None);

        // Assert
        LoggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(e => e == exception && e.InnerException == innerException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
