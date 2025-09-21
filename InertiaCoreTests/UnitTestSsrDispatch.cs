using InertiaCore.Models;
using InertiaCore.Ssr;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Moq;

namespace InertiaCoreTests;

public partial class Tests
{
    [Test]
    [Description("Test SSR dispatch should not dispatch by default when no bundle exists")]
    public void TestSsrDispatchDefaultBehaviorWithoutBundle()
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(x => x.ContentRootPath).Returns(Path.GetTempPath());

        var options = new Mock<IOptions<InertiaOptions>>();
        options.SetupGet(x => x.Value).Returns(new InertiaOptions { SsrDispatchWithoutBundle = false });

        var gateway = new Gateway(httpClientFactory.Object, options.Object, environment.Object);

        Assert.That(gateway.ShouldDispatch(), Is.False);
    }

    [Test]
    [Description("Test SSR dispatch should dispatch when SsrDispatchWithoutBundle is enabled")]
    public void TestSsrDispatchWithoutBundleEnabled()
    {
        var httpClientFactory = new Mock<IHttpClientFactory>();
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(x => x.ContentRootPath).Returns(Path.GetTempPath());

        var options = new Mock<IOptions<InertiaOptions>>();
        options.SetupGet(x => x.Value).Returns(new InertiaOptions { SsrDispatchWithoutBundle = true });

        var gateway = new Gateway(httpClientFactory.Object, options.Object, environment.Object);

        Assert.That(gateway.ShouldDispatch(), Is.True);
    }

    [Test]
    [Description("Test SSR dispatch should dispatch when bundle exists")]
    public void TestSsrDispatchWithBundleExists()
    {
        var tempDir = Path.GetTempPath();
        var bundleDir = Path.Combine(tempDir, "wwwroot", "js");
        Directory.CreateDirectory(bundleDir);

        var bundlePath = Path.Combine(bundleDir, "ssr.js");
        File.WriteAllText(bundlePath, "// SSR bundle");

        try
        {
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(x => x.ContentRootPath).Returns(tempDir);

            var options = new Mock<IOptions<InertiaOptions>>();
            options.SetupGet(x => x.Value).Returns(new InertiaOptions { SsrDispatchWithoutBundle = false });

            var gateway = new Gateway(httpClientFactory.Object, options.Object, environment.Object);

            Assert.That(gateway.ShouldDispatch(), Is.True);
        }
        finally
        {
            if (File.Exists(bundlePath))
                File.Delete(bundlePath);
            if (Directory.Exists(bundleDir))
                Directory.Delete(bundleDir, true);
        }
    }

    [Test]
    [Description("Test SSR dispatch should dispatch when either bundle exists or dispatch without bundle is enabled")]
    public void TestSsrDispatchWithBundleAndDispatchWithoutBundleEnabled()
    {
        var tempDir = Path.GetTempPath();
        var bundleDir = Path.Combine(tempDir, "build");
        Directory.CreateDirectory(bundleDir);

        var bundlePath = Path.Combine(bundleDir, "ssr.js");
        File.WriteAllText(bundlePath, "// SSR bundle");

        try
        {
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(x => x.ContentRootPath).Returns(tempDir);

            var options = new Mock<IOptions<InertiaOptions>>();
            options.SetupGet(x => x.Value).Returns(new InertiaOptions { SsrDispatchWithoutBundle = true });

            var gateway = new Gateway(httpClientFactory.Object, options.Object, environment.Object);

            Assert.That(gateway.ShouldDispatch(), Is.True);
        }
        finally
        {
            if (File.Exists(bundlePath))
                File.Delete(bundlePath);
            if (Directory.Exists(bundleDir))
                Directory.Delete(bundleDir, true);
        }
    }

    [Test]
    [Description("Test SSR dispatch checks multiple common bundle paths")]
    public void TestSsrDispatchChecksMultipleBundlePaths()
    {
        var tempDir = Path.GetTempPath();
        var bundleDir = Path.Combine(tempDir, "dist");
        Directory.CreateDirectory(bundleDir);

        var bundlePath = Path.Combine(bundleDir, "ssr.js");
        File.WriteAllText(bundlePath, "// SSR bundle in dist");

        try
        {
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(x => x.ContentRootPath).Returns(tempDir);

            var options = new Mock<IOptions<InertiaOptions>>();
            options.SetupGet(x => x.Value).Returns(new InertiaOptions { SsrDispatchWithoutBundle = false });

            var gateway = new Gateway(httpClientFactory.Object, options.Object, environment.Object);

            Assert.That(gateway.ShouldDispatch(), Is.True);
        }
        finally
        {
            if (File.Exists(bundlePath))
                File.Delete(bundlePath);
            if (Directory.Exists(bundleDir))
                Directory.Delete(bundleDir, true);
        }
    }
}