using InertiaCore;
using InertiaCore.Extensions;
using InertiaCore.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System.Net;
using InertiaCore.Models;
using InertiaCore.Ssr;

namespace InertiaCoreTests;

// Test implementation of middleware for testing purposes
public class TestMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApplicationBuilder _app;

    public TestMiddleware(RequestDelegate next, IApplicationBuilder app)
    {
        _next = next;
        _app = app;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Simple check for Inertia request
        var isInertia = context.Request.Headers.ContainsKey(InertiaHeader.Inertia);
        var requestVersion = context.Request.Headers[InertiaHeader.Version].FirstOrDefault();
        var currentVersion = Inertia.GetVersion();

        if (isInertia && context.Request.Method == "GET" && requestVersion != currentVersion)
        {
            await OnVersionChange(context, _app);
            return;
        }
        await _next(context);
    }

    private static async Task OnVersionChange(HttpContext context, IApplicationBuilder app)
    {
        var tempData = app.ApplicationServices.GetRequiredService<ITempDataDictionaryFactory>()
            .GetTempData(context);

        if (tempData.Count > 0) tempData.Keep();

        var requestUri = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
        context.Response.Headers[InertiaHeader.Location] = requestUri;
        context.Response.StatusCode = (int)HttpStatusCode.Conflict;

        // Mock the CompleteAsync for testing
        await Task.CompletedTask;
    }
}

[TestFixture]
public class UnitTestMiddleware
{
    private TestMiddleware _middleware = null!;
    private Mock<RequestDelegate> _nextMock = null!;
    private Mock<IApplicationBuilder> _appMock = null!;
    private Mock<IServiceProvider> _serviceProviderMock = null!;
    private Mock<ITempDataDictionaryFactory> _tempDataFactoryMock = null!;
    private Mock<ITempDataDictionary> _tempDataMock = null!;
    private IResponseFactory _factory = null!;

    [SetUp]
    public void Setup()
    {
        _nextMock = new Mock<RequestDelegate>();
        _appMock = new Mock<IApplicationBuilder>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _tempDataFactoryMock = new Mock<ITempDataDictionaryFactory>();
        _tempDataMock = new Mock<ITempDataDictionary>();

        _tempDataFactoryMock.Setup(f => f.GetTempData(It.IsAny<HttpContext>()))
            .Returns(_tempDataMock.Object);

        _serviceProviderMock.Setup(s => s.GetService(typeof(ITempDataDictionaryFactory)))
            .Returns(_tempDataFactoryMock.Object);

        _appMock.Setup(a => a.ApplicationServices).Returns(_serviceProviderMock.Object);

        // Set up Inertia factory
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var gateway = new Gateway(httpClientFactory.Object);
        var options = new Mock<IOptions<InertiaOptions>>();
        options.SetupGet(x => x.Value).Returns(new InertiaOptions());

        _factory = new ResponseFactory(contextAccessor.Object, gateway, options.Object);
        Inertia.UseFactory(_factory);

        _middleware = new TestMiddleware(_nextMock.Object, _appMock.Object);
    }

    [Test]
    public async Task InvokeAsync_NonInertiaRequest_CallsNext()
    {
        // Arrange
        var context = CreateHttpContext(isInertia: false);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Test]
    public async Task InvokeAsync_InertiaPostRequest_CallsNext()
    {
        // Arrange
        var context = CreateHttpContext(
            isInertia: true,
            method: "POST",
            version: "test-version"
        );
        Inertia.Version("test-version");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Test]
    public async Task InvokeAsync_InertiaGetRequestWithSameVersion_CallsNext()
    {
        // Arrange
        var version = "v1.0.0";
        Inertia.Version(version);
        var context = CreateHttpContext(
            isInertia: true,
            method: "GET",
            version: version
        );

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
    }

    [Test]
    public async Task InvokeAsync_InertiaGetRequestWithDifferentVersion_ReturnsConflict()
    {
        // Arrange
        var currentVersion = "v2.0.0";
        var requestVersion = "v1.0.0";
        Inertia.Version(currentVersion);

        var context = CreateHttpContext(
            isInertia: true,
            method: "GET",
            version: requestVersion,
            requestUri: "https://example.com/test"
        );

        // Setup ITempDataDictionary to indicate no temp data
        _tempDataMock.Setup(t => t.Count).Returns(0);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.Conflict));
        Assert.That(context.Response.Headers[InertiaHeader.Location], Is.EqualTo("https://example.com/test"));
        _nextMock.Verify(next => next(It.IsAny<HttpContext>()), Times.Never);
    }

    [Test]
    public async Task InvokeAsync_VersionChangeWithTempData_KeepsTempData()
    {
        // Arrange
        var currentVersion = "v2.0.0";
        var requestVersion = "v1.0.0";
        Inertia.Version(currentVersion);

        var context = CreateHttpContext(
            isInertia: true,
            method: "GET",
            version: requestVersion,
            requestUri: "https://example.com/test"
        );

        // Setup ITempDataDictionary to indicate it has temp data
        _tempDataMock.Setup(t => t.Count).Returns(1);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _tempDataMock.Verify(t => t.Keep(), Times.Once);
        Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.Conflict));
    }

    [Test]
    public async Task InvokeAsync_VersionChangeWithoutTempData_DoesNotKeepTempData()
    {
        // Arrange
        var currentVersion = "v2.0.0";
        var requestVersion = "v1.0.0";
        Inertia.Version(currentVersion);

        var context = CreateHttpContext(
            isInertia: true,
            method: "GET",
            version: requestVersion,
            requestUri: "https://example.com/test"
        );

        // Setup ITempDataDictionary to indicate no temp data
        _tempDataMock.Setup(t => t.Count).Returns(0);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _tempDataMock.Verify(t => t.Keep(), Times.Never);
        Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.Conflict));
    }

    [Test]
    public async Task InvokeAsync_InertiaGetRequestWithNoVersionHeader_CallsNext()
    {
        // Arrange
        var context = CreateHttpContext(
            isInertia: true,
            method: "GET",
            version: null
        );

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
    }

    private static HttpContext CreateHttpContext(
        bool isInertia = false,
        string method = "GET",
        string? version = null,
        string requestUri = "https://example.com")
    {
        var requestHeaders = new HeaderDictionary();
        if (isInertia)
        {
            requestHeaders[InertiaHeader.Inertia] = "true";
        }
        if (version != null)
        {
            requestHeaders[InertiaHeader.Version] = version;
        }

        var requestMock = new Mock<HttpRequest>();
        requestMock.SetupGet(r => r.Method).Returns(method);
        requestMock.SetupGet(r => r.Scheme).Returns("https");
        requestMock.SetupGet(r => r.Host).Returns(new HostString("example.com"));
        requestMock.SetupGet(r => r.Path).Returns(new Uri(requestUri).AbsolutePath);
        requestMock.SetupGet(r => r.QueryString).Returns(new QueryString(new Uri(requestUri).Query));
        requestMock.SetupGet(r => r.Headers).Returns(requestHeaders);

        var responseHeaders = new HeaderDictionary();
        var responseMock = new Mock<HttpResponse>();
        responseMock.SetupGet(r => r.Headers).Returns(responseHeaders);
        responseMock.SetupProperty(r => r.StatusCode);

        var contextMock = new Mock<HttpContext>();
        contextMock.SetupGet(c => c.Request).Returns(requestMock.Object);
        contextMock.SetupGet(c => c.Response).Returns(responseMock.Object);

        return contextMock.Object;
    }
}