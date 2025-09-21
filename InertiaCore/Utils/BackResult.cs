using System.Net;
using InertiaCore.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace InertiaCore.Utils;

public class BackResult : IActionResult
{
    private readonly string _fallbackUrl;
    private readonly HttpStatusCode _statusCode;

    public BackResult(string? fallbackUrl = null, HttpStatusCode statusCode = HttpStatusCode.SeeOther) =>
        (_fallbackUrl, _statusCode) = (fallbackUrl ?? "/", statusCode);

    public Task ExecuteResultAsync(ActionContext context)
    {
        var referrer = context.HttpContext.Request.Headers.Referer.ToString();
        var redirectUrl = !string.IsNullOrEmpty(referrer) ? referrer : _fallbackUrl;

        context.HttpContext.Response.StatusCode = (int)_statusCode;
        context.HttpContext.Response.Headers.Location = redirectUrl;

        return Task.CompletedTask;
    }
}
