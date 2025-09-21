using System.Net;
using InertiaCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace InertiaCore.Utils;

public class BackResult : IActionResult
{
    private readonly string _fallbackUrl;

    public BackResult(string? fallbackUrl = null) => _fallbackUrl = fallbackUrl ?? "/";

    public async Task ExecuteResultAsync(ActionContext context)
    {
        // Store validation errors in TempData if ModelState has errors
        if (!context.ModelState.IsValid)
        {
            var tempDataFactory = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
            var tempData = tempDataFactory.GetTempData(context.HttpContext);
            tempData.SetValidationErrors(context.ModelState);
        }

        var referrer = context.HttpContext.Request.Headers.Referer.ToString();
        var redirectUrl = !string.IsNullOrEmpty(referrer) ? referrer : _fallbackUrl;

        await new RedirectResult(redirectUrl).ExecuteResultAsync(context);
    }
}
