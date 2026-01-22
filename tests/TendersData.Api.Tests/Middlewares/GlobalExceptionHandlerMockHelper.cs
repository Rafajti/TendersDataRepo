using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using TendersData.Api.Middlewares;

namespace TendersData.Api.Tests.Middlewares;

public abstract class GlobalExceptionHandlerMockHelper
{
    protected Mock<ILogger<GlobalExceptionHandler>> LoggerMock { get; }
    protected DefaultHttpContext HttpContext { get; }

    protected GlobalExceptionHandlerMockHelper()
    {
        LoggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        HttpContext = new DefaultHttpContext();
    }

    protected static IHostEnvironment CreateEnvironment(string environmentName)
    {
        var env = new Mock<IHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns(environmentName);
        env.Setup(e => e.ApplicationName).Returns("TestApp");
        env.Setup(e => e.ContentRootPath).Returns("/");
        return env.Object;
    }

    protected GlobalExceptionHandler CreateHandler(string environmentName = "Production")
    {
        var env = CreateEnvironment(environmentName);
        return new GlobalExceptionHandler(LoggerMock.Object, env);
    }
}
