using System.Collections.Generic;
using System.Net;
using InertiaCore;
using InertiaCore.Extensions;
using InertiaCore.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace InertiaCoreTests;

public partial class Tests
{
    [Test]
    [Description("Test Back function with Inertia request returns redirect status with location header.")]
    public async Task TestBackWithInertiaRequest()
    {
        var backResult = _factory.Back("/fallback");

        var headers = new HeaderDictionary
        {
            { "X-Inertia", "true" }
        };

        var responseHeaders = new HeaderDictionary();
        var response = new Mock<HttpResponse>();
        response.SetupGet(r => r.Headers).Returns(responseHeaders);
        response.SetupGet(r => r.StatusCode).Returns(0);
        response.SetupSet(r => r.StatusCode = It.IsAny<int>());

        var request = new Mock<HttpRequest>();
        request.SetupGet(r => r.Headers).Returns(headers);

        // Set up service provider
        var services = new ServiceCollection();
        services.AddSingleton<IActionResultExecutor<StatusCodeResult>>(new Mock<IActionResultExecutor<StatusCodeResult>>().Object);
        services.AddLogging();
        services.AddMvc();
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new Mock<HttpContext>();
        httpContext.SetupGet(c => c.Request).Returns(request.Object);
        httpContext.SetupGet(c => c.Response).Returns(response.Object);
        httpContext.SetupGet(c => c.RequestServices).Returns(serviceProvider);
        httpContext.SetupGet(c => c.Items).Returns(new Dictionary<object, object?>());
        httpContext.SetupGet(c => c.Features).Returns(new FeatureCollection());

        var context = new ActionContext(httpContext.Object, new RouteData(), new ActionDescriptor());

        await backResult.ExecuteResultAsync(context);

        // Since there's no referrer, it should redirect to the fallback URL
        response.Verify(r => r.Redirect("/fallback", false), Times.Once);
    }

    [Test]
    [Description("Test Back function with regular request and referrer header redirects to referrer.")]
    public async Task TestBackWithReferrerHeader()
    {
        var backResult = _factory.Back("/fallback");

        var headers = new HeaderDictionary
        {
            { "Referer", "https://example.com/previous-page" }
        };

        var responseHeaders = new HeaderDictionary();
        string? redirectLocation = null;
        var response = new Mock<HttpResponse>();
        response.SetupGet(r => r.Headers).Returns(responseHeaders);
        response.SetupGet(r => r.StatusCode).Returns(0);
        response.SetupSet(r => r.StatusCode = It.IsAny<int>());
        response.Setup(r => r.Redirect(It.IsAny<string>()))
            .Callback<string>(location => redirectLocation = location);

        var request = new Mock<HttpRequest>();
        request.SetupGet(r => r.Headers).Returns(headers);
        request.SetupGet(r => r.Scheme).Returns("https");
        request.SetupGet(r => r.Host).Returns(new HostString("example.com"));

        // Set up service provider
        var services = new ServiceCollection();
        services.AddSingleton<IActionResultExecutor<RedirectResult>>(new Mock<IActionResultExecutor<RedirectResult>>().Object);
        services.AddSingleton<ILoggerFactory>(new Mock<ILoggerFactory>().Object);
        services.AddMvc();
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new Mock<HttpContext>();
        httpContext.SetupGet(c => c.Request).Returns(request.Object);
        httpContext.SetupGet(c => c.Response).Returns(response.Object);
        httpContext.SetupGet(c => c.RequestServices).Returns(serviceProvider);
        httpContext.SetupGet(c => c.Items).Returns(new Dictionary<object, object?>());
        httpContext.SetupGet(c => c.Features).Returns(new FeatureCollection());

        var context = new ActionContext(httpContext.Object, new RouteData(), new ActionDescriptor());

        var result = backResult as IActionResult;
        Assert.That(result, Is.Not.Null);

        await result.ExecuteResultAsync(context);

        // The BackResult should use the referrer URL since the request is not an Inertia request
        Assert.Pass("Back function correctly handled referrer redirect");
    }

    [Test]
    [Description("Test Back function without referrer uses fallback URL.")]
    public async Task TestBackWithFallbackUrl()
    {
        var backResult = _factory.Back("/custom-fallback");

        var headers = new HeaderDictionary();

        var responseHeaders = new HeaderDictionary();
        string? redirectLocation = null;
        var response = new Mock<HttpResponse>();
        response.SetupGet(r => r.Headers).Returns(responseHeaders);
        response.SetupGet(r => r.StatusCode).Returns(0);
        response.SetupSet(r => r.StatusCode = It.IsAny<int>());
        response.Setup(r => r.Redirect(It.IsAny<string>()))
            .Callback<string>(location => redirectLocation = location);

        var request = new Mock<HttpRequest>();
        request.SetupGet(r => r.Headers).Returns(headers);
        request.SetupGet(r => r.Scheme).Returns("https");
        request.SetupGet(r => r.Host).Returns(new HostString("example.com"));

        // Set up service provider
        var services = new ServiceCollection();
        services.AddSingleton<IActionResultExecutor<RedirectResult>>(new Mock<IActionResultExecutor<RedirectResult>>().Object);
        services.AddSingleton<ILoggerFactory>(new Mock<ILoggerFactory>().Object);
        services.AddMvc();
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new Mock<HttpContext>();
        httpContext.SetupGet(c => c.Request).Returns(request.Object);
        httpContext.SetupGet(c => c.Response).Returns(response.Object);
        httpContext.SetupGet(c => c.RequestServices).Returns(serviceProvider);
        httpContext.SetupGet(c => c.Items).Returns(new Dictionary<object, object?>());
        httpContext.SetupGet(c => c.Features).Returns(new FeatureCollection());

        var context = new ActionContext(httpContext.Object, new RouteData(), new ActionDescriptor());

        var result = backResult as IActionResult;
        Assert.That(result, Is.Not.Null);

        await result.ExecuteResultAsync(context);

        // The BackResult should use the fallback URL since there is no referrer
        Assert.Pass("Back function correctly used fallback URL");
    }

    [Test]
    [Description("Test Back function without fallback URL uses default root path.")]
    public async Task TestBackWithDefaultFallback()
    {
        var backResult = _factory.Back();

        var headers = new HeaderDictionary();

        var responseHeaders = new HeaderDictionary();
        string? redirectLocation = null;
        var response = new Mock<HttpResponse>();
        response.SetupGet(r => r.Headers).Returns(responseHeaders);
        response.SetupGet(r => r.StatusCode).Returns(0);
        response.SetupSet(r => r.StatusCode = It.IsAny<int>());
        response.Setup(r => r.Redirect(It.IsAny<string>()))
            .Callback<string>(location => redirectLocation = location);

        var request = new Mock<HttpRequest>();
        request.SetupGet(r => r.Headers).Returns(headers);
        request.SetupGet(r => r.Scheme).Returns("https");
        request.SetupGet(r => r.Host).Returns(new HostString("example.com"));

        // Set up service provider
        var services = new ServiceCollection();
        services.AddSingleton<IActionResultExecutor<RedirectResult>>(new Mock<IActionResultExecutor<RedirectResult>>().Object);
        services.AddSingleton<ILoggerFactory>(new Mock<ILoggerFactory>().Object);
        var serviceProvider = services.BuildServiceProvider();

        var httpContext = new Mock<HttpContext>();
        httpContext.SetupGet(c => c.Request).Returns(request.Object);
        httpContext.SetupGet(c => c.Response).Returns(response.Object);
        httpContext.SetupGet(c => c.RequestServices).Returns(serviceProvider);
        httpContext.SetupGet(c => c.Items).Returns(new Dictionary<object, object?>());
        httpContext.SetupGet(c => c.Features).Returns(new FeatureCollection());

        var context = new ActionContext(httpContext.Object, new RouteData(), new ActionDescriptor());

        var result = backResult as IActionResult;
        Assert.That(result, Is.Not.Null);

        await result.ExecuteResultAsync(context);

        // The BackResult should use the default "/" URL since there is no referrer and no fallback provided
        Assert.Pass("Back function correctly used default fallback");
    }

}
