using InertiaCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Net;
using InertiaCore.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using InertiaCore.Extensions;

namespace InertiaCore;

public class Middleware
{
    private readonly RequestDelegate _next;

    public Middleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.IsInertiaRequest()
            && context.Request.Method == "GET"
            && context.Request.Headers[InertiaHeader.Version] != Inertia.GetVersion())
        {
            await OnVersionChange(context);
            return;
        }
        await _next(context);
    }

    private static async Task OnVersionChange(HttpContext context)
    {
        var tempData = context.RequestServices.GetRequiredService<ITempDataDictionaryFactory>()
            .GetTempData(context);

        if (tempData.Any()) tempData.Keep();

        context.Response.Headers.Override(InertiaHeader.Location, context.RequestedUri());
        context.Response.StatusCode = (int)HttpStatusCode.Conflict;

        await context.Response.CompleteAsync();
    }
}
