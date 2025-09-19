using System.Net;
using InertiaCore.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace InertiaCore.Utils;

public class BackResult : IActionResult
{
    private readonly string _fallbackUrl;

    public BackResult(string? fallbackUrl = null) => _fallbackUrl = fallbackUrl ?? "/";

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var referrer = context.HttpContext.Request.Headers.Referer.ToString();
        var redirectUrl = !string.IsNullOrEmpty(referrer) ? referrer : _fallbackUrl;

        if (context.IsInertiaRequest())
        {
            context.HttpContext.Response.Headers.Override(InertiaHeader.Location, redirectUrl);
            await new StatusCodeResult((int)HttpStatusCode.Conflict).ExecuteResultAsync(context);
            return;
        }

        await new RedirectResult(redirectUrl).ExecuteResultAsync(context);
    }
}
