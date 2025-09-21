using System.Text.Json;
using InertiaCore.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.IO.Hashing;

namespace InertiaCore.Extensions;

internal static class InertiaExtensions
{
    internal static bool IsInertiaPartialComponent(this ActionContext context, string component) =>
        context.HttpContext.Request.Headers[InertiaHeader.PartialComponent] == component;

    internal static string RequestedUri(this HttpContext context) =>
        Uri.UnescapeDataString(context.Request.GetEncodedPathAndQuery());

    internal static string RequestedUri(this ActionContext context) => context.HttpContext.RequestedUri();

    internal static bool IsInertiaRequest(this HttpContext context) =>
        bool.TryParse(context.Request.Headers[InertiaHeader.Inertia], out _);

    internal static bool IsInertiaRequest(this ActionContext context) => context.HttpContext.IsInertiaRequest();

    internal static string ToCamelCase(this string s) => JsonNamingPolicy.CamelCase.ConvertName(s);

    internal static bool Override<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.TryAdd(key, value)) return false;
        dictionary[key] = value;

        return true;
    }

    internal static Task<object?> ResolveAsync(this Func<object?> func)
    {
        var rt = func.Method.ReturnType;

        if (!rt.IsGenericType || rt.GetGenericTypeDefinition() != typeof(Task<>))
            return Task.Run(func.Invoke);

        var task = func.DynamicInvoke() as Task;
        return task!.ResolveResult();
    }

    internal static async Task<object?> ResolveResult(this Task task)
    {
        await task.ConfigureAwait(false);
        var result = task.GetType().GetProperty("Result");

        return result?.GetValue(task);
    }

    internal static string XXH128(this string s)
    {
        var inputBytes = Encoding.UTF8.GetBytes(s);
        var hashBytes = XxHash128.Hash(inputBytes);

        var sb = new StringBuilder();
        foreach (var t in hashBytes)
            sb.Append(t.ToString("x2"));

        return sb.ToString();
    }

    /// <summary>
    /// Gets the TempData dictionary for the current HTTP context.
    /// </summary>
    internal static ITempDataDictionary? GetTempData(this HttpContext context)
    {
        try
        {
            var tempDataFactory = context.RequestServices?.GetRequiredService<ITempDataDictionaryFactory>();
            return tempDataFactory?.GetTempData(context);
        }
        catch (InvalidOperationException)
        {
            // Service provider not available, return null
            return null;
        }
    }

    /// <summary>
    /// Sets validation errors in TempData for the specified error bag.
    /// </summary>
    public static void SetValidationErrors(this ITempDataDictionary tempData, Dictionary<string, string> errors, string bagName = "default")
    {
        var errorBags = tempData["__ValidationErrors"] as Dictionary<string, Dictionary<string, string>>
                       ?? new Dictionary<string, Dictionary<string, string>>();

        errorBags[bagName] = errors;
        tempData["__ValidationErrors"] = errorBags;
    }

    /// <summary>
    /// Sets validation errors in TempData from ModelState for the specified error bag.
    /// </summary>
    public static void SetValidationErrors(this ITempDataDictionary tempData, ModelStateDictionary modelState, string bagName = "default")
    {
        var errors = modelState.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value?.Errors.FirstOrDefault()?.ErrorMessage ?? ""
        );
        tempData.SetValidationErrors(errors, bagName);
    }
}
