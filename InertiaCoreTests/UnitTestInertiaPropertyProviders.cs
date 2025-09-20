using InertiaCore.Models;
using InertiaCore.Utils;
using Microsoft.AspNetCore.Http;

namespace InertiaCoreTests;

public partial class Tests
{
    [Test]
    [Description("Test if ProvidesInertiaProperties interface works correctly.")]
    public async Task TestInertiaPropertyProvider()
    {
        var provider = new TestPropertyProvider();

        var response = _factory.Render("Test/Page", new
        {
            Test = "Test",
            Provider = provider
        });

        var context = PrepareContext();

        response.SetContext(context);
        await response.ProcessResponse();

        var page = response.GetJson().Value as Page;

        Assert.That(page?.Props, Is.EqualTo(new Dictionary<string, object?>
        {
            { "test", "Test" },
            { "user", "John Doe" },
            { "permissions", new List<string> { "read", "write" } },
            { "component", "Test/Page" },
            { "errors", new Dictionary<string, string>(0) }
        }));
    }

    [Test]
    [Description("Test if multiple ProvidesInertiaProperties objects work together.")]
    public async Task TestMultipleInertiaPropertyProviders()
    {
        var userProvider = new UserPropertyProvider();
        var settingsProvider = new SettingsPropertyProvider();

        var response = _factory.Render("Dashboard/Index", new
        {
            Test = "Test",
            UserProvider = userProvider,
            SettingsProvider = settingsProvider
        });

        var context = PrepareContext();

        response.SetContext(context);
        await response.ProcessResponse();

        var page = response.GetJson().Value as Page;

        Assert.That(page?.Props, Is.EqualTo(new Dictionary<string, object?>
        {
            { "test", "Test" },
            { "user", "Alice" },
            { "role", "admin" },
            { "theme", "dark" },
            { "language", "en" },
            { "errors", new Dictionary<string, string>(0) }
        }));
    }

    [Test]
    [Description("Test if ProvidesInertiaProperties receives correct RenderContext.")]
    public async Task TestInertiaPropertyProviderContext()
    {
        var provider = new ContextAwarePropertyProvider();

        var response = _factory.Render("User/Profile", new
        {
            Provider = provider
        });

        var headers = new HeaderDictionary
        {
            { "X-Custom-Header", "test-value" }
        };

        var context = PrepareContext(headers);

        response.SetContext(context);
        await response.ProcessResponse();

        var page = response.GetJson().Value as Page;

        Assert.That(page?.Props, Is.EqualTo(new Dictionary<string, object?>
        {
            { "component", "User/Profile" },
            { "hasCustomHeader", true },
            { "errors", new Dictionary<string, string>(0) }
        }));
    }
}

// Test implementations of ProvidesInertiaProperties
internal class TestPropertyProvider : ProvidesInertiaProperties
{
    public IEnumerable<KeyValuePair<string, object?>> ToInertiaProperties(RenderContext context)
    {
        yield return new KeyValuePair<string, object?>("user", "John Doe");
        yield return new KeyValuePair<string, object?>("permissions", new List<string> { "read", "write" });
        yield return new KeyValuePair<string, object?>("component", context.Component);
    }
}

internal class UserPropertyProvider : ProvidesInertiaProperties
{
    public IEnumerable<KeyValuePair<string, object?>> ToInertiaProperties(RenderContext context)
    {
        yield return new KeyValuePair<string, object?>("user", "Alice");
        yield return new KeyValuePair<string, object?>("role", "admin");
    }
}

internal class SettingsPropertyProvider : ProvidesInertiaProperties
{
    public IEnumerable<KeyValuePair<string, object?>> ToInertiaProperties(RenderContext context)
    {
        yield return new KeyValuePair<string, object?>("theme", "dark");
        yield return new KeyValuePair<string, object?>("language", "en");
    }
}

internal class ContextAwarePropertyProvider : ProvidesInertiaProperties
{
    public IEnumerable<KeyValuePair<string, object?>> ToInertiaProperties(RenderContext context)
    {
        yield return new KeyValuePair<string, object?>("component", context.Component);
        yield return new KeyValuePair<string, object?>("hasCustomHeader",
            context.Request.Headers.ContainsKey("X-Custom-Header"));
    }
}