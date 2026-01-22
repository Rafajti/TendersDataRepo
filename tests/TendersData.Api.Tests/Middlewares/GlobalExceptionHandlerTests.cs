using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;
using TendersData.Api.Middlewares;

namespace TendersData.Api.Tests.Middlewares;

public class GlobalExceptionHandlerTests
{
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;
    private readonly DefaultHttpContext _httpContext;

    private IHostEnvironment CreateEnvironment(string environmentName)
    {
        var env = new Mock<IHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns(environmentName);
        env.Setup(e => e.ApplicationName).Returns("TestApp");
        env.Setup(e => e.ContentRootPath).Returns("/");
        return env.Object;
    }

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _httpContext = new DefaultHttpContext();
    }

    [Fact]
    public async Task TryHandleAsync_WithGenericException_Returns500StatusCode()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var env = CreateEnvironment(Environments.Production);
        var handler = new GlobalExceptionHandler(_loggerMock.Object, env);

        // Act
        var result = await handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task TryHandleAsync_WithGenericException_LogsError()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var env = CreateEnvironment(Environments.Production);
        var handler = new GlobalExceptionHandler(_loggerMock.Object, env);

        // Act
        await handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
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
        var env = CreateEnvironment(Environments.Production);
        var handler = new GlobalExceptionHandler(_loggerMock.Object, env);
        _httpContext.Response.Body = new MemoryStream();

        // Act
        await handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(_httpContext.Response.Body);
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
        var env = CreateEnvironment(Environments.Development);
        var handler = new GlobalExceptionHandler(_loggerMock.Object, env);
        _httpContext.Response.Body = new MemoryStream();

        // Act
        await handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(_httpContext.Response.Body);
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
        var env = CreateEnvironment(Environments.Production);
        var handler = new GlobalExceptionHandler(_loggerMock.Object, env);
        _httpContext.Response.Body = new MemoryStream();

        // Act
        await handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(_httpContext.Response.Body);
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
    public async Task TryHandleAsync_AddsTraceIdToProblemDetails()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var env = CreateEnvironment(Environments.Production);
        var handler = new GlobalExceptionHandler(_loggerMock.Object, env);
        _httpContext.Response.Body = new MemoryStream();
        _httpContext.TraceIdentifier = "test-trace-id";

        // Act
        await handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(_httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        var jsonDoc = JsonDocument.Parse(responseBody);

        jsonDoc.RootElement.TryGetProperty("traceId", out var traceIdElement).Should().BeTrue();
        traceIdElement.GetString().Should().Be("test-trace-id");
    }

    [Fact]
    public async Task TryHandleAsync_SetsInstanceToRequestPath()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var env = CreateEnvironment(Environments.Production);
        var handler = new GlobalExceptionHandler(_loggerMock.Object, env);
        _httpContext.Request.Path = "/api/test";
        _httpContext.Response.Body = new MemoryStream();

        // Act
        await handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(_httpContext.Response.Body);
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
        var env = CreateEnvironment(Environments.Production);
        var handler = new GlobalExceptionHandler(_loggerMock.Object, env);

        // Act
        await handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _httpContext.Response.ContentType.Should().StartWith("application/json");
    }

    [Fact]
    public async Task TryHandleAsync_WithNullExceptionMessage_HandlesGracefully()
    {
        // Arrange
        var exception = new Exception();
        var env = CreateEnvironment(Environments.Production);
        var handler = new GlobalExceptionHandler(_loggerMock.Object, env);
        _httpContext.Response.Body = new MemoryStream();

        // Act
        var result = await handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _httpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task TryHandleAsync_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var env = CreateEnvironment(Environments.Production);
        var handler = new GlobalExceptionHandler(_loggerMock.Object, env);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await handler.TryHandleAsync(_httpContext, exception, cts.Token));
    }

    [Fact]
    public async Task TryHandleAsync_WithInnerException_LogsCorrectly()
    {
        // Arrange
        var innerException = new InvalidOperationException("Inner exception");
        var exception = new Exception("Outer exception", innerException);
        var env = CreateEnvironment(Environments.Production);
        var handler = new GlobalExceptionHandler(_loggerMock.Object, env);

        // Act
        await handler.TryHandleAsync(_httpContext, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.Is<Exception>(e => e == exception && e.InnerException == innerException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
