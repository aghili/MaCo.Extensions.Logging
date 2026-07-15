using MaCo.Extensions.Logging;
using MaCo.Extensions.Logging.Classes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MaCo.Extensions.Logging.Tests;

// ── MaCoLogger ─────────────────────────────────────────────────────────

[TestClass]
public class MaCoLoggerTests
{
    private InMemoryAdapter _adapter = null!;

    [TestInitialize]
    public void Init()
    {
        Log.Instance.Settings.Enabled = true;
        Log.Instance.Settings.MessageTypes =
            LogMessageType.Exception | LogMessageType.Warning |
            LogMessageType.Information | LogMessageType.DataLog;
        Log.Instance.writeAdapter.Clear();
        _adapter = new InMemoryAdapter();
        Log.Instance.writeAdapter.Add(_adapter);
    }

    [TestCleanup]
    public void Cleanup() => Log.Dispose();

    [TestMethod]
    public void Log_Information_FormatsMessage()
    {
        var logger = new MaCoLogger("TestCategory", () => new MaCoLoggerConfiguration());
        logger.Log(LogLevel.Information, 0, "hello {Name}", null, (s, e) => $"FORM{s}");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("FORMhello {Name}")));
    }

    [TestMethod]
    public void Log_IncludesCategoryName()
    {
        var logger = new MaCoLogger("MyCategory", () => new MaCoLoggerConfiguration());
        logger.Log(LogLevel.Information, 0, "test", null, (s, e) => "msg");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("[MyCategory]")));
    }

    [TestMethod]
    public void Log_DisabledLevel_DoesNotWrite()
    {
        var config = new MaCoLoggerConfiguration { LogLevels = new List<LogLevel> { LogLevel.Warning } };
        var logger = new MaCoLogger("test", () => config);
        logger.Log(LogLevel.Information, 0, "should not appear", null, (s, e) => "msg");
        Assert.IsFalse(_adapter.Entries.Any(e => e.Contains("should not appear")));
    }

    [TestMethod]
    public void Log_EnabledLevel_Writes()
    {
        var config = new MaCoLoggerConfiguration { LogLevels = new List<LogLevel> { LogLevel.Warning } };
        var logger = new MaCoLogger("test", () => config);
        logger.Log(LogLevel.Warning, 0, "should appear", null, (s, e) => "msg");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("msg")));
    }

    [TestMethod]
    public void IsEnabled_ReturnsCorrectly()
    {
        var config = new MaCoLoggerConfiguration { LogLevels = new List<LogLevel> { LogLevel.Error } };
        var logger = new MaCoLogger("test", () => config);
        Assert.IsFalse(logger.IsEnabled(LogLevel.Information));
        Assert.IsTrue(logger.IsEnabled(LogLevel.Error));
    }

    [TestMethod]
    public void BeginScope_ReturnsNull()
    {
        var logger = new MaCoLogger("test", () => new MaCoLoggerConfiguration());
        Assert.IsNull(logger.BeginScope("state"));
    }

    [TestMethod]
    public void Log_WithException_IncludesExceptionMessage()
    {
        var logger = new MaCoLogger("test", () => new MaCoLoggerConfiguration());
        var ex = new InvalidOperationException("test error");
        logger.Log(LogLevel.Error, 0, "state", ex, (s, e) => $"ERR:{e!.Message}");
        Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("ERR:test error")));
    }
}

// ── MaCoLoggerProvider ─────────────────────────────────────────────────

[TestClass]
public class MaCoLoggerProviderTests
{
    [TestMethod]
    public void CreateLogger_ReturnsMaCoLogger()
    {
        var provider = new MaCoLoggerProvider(() => new MaCoLoggerConfiguration());
        var logger = provider.CreateLogger("TestCategory");
        Assert.IsInstanceOfType(logger, typeof(MaCoLogger));
    }

    [TestMethod]
    public void CreateLogger_DifferentCategories()
    {
        var provider = new MaCoLoggerProvider(() => new MaCoLoggerConfiguration());
        var logger1 = provider.CreateLogger("Cat1");
        var logger2 = provider.CreateLogger("Cat2");
        Assert.IsNotNull(logger1);
        Assert.IsNotNull(logger2);
    }

    [TestMethod]
    public void Dispose_CallsLogDispose()
    {
        var provider = new MaCoLoggerProvider(() => new MaCoLoggerConfiguration());
        provider.Dispose();
        // Should not throw
    }
}

// ── MaCoLoggingBuilderExtensions ───────────────────────────────────────

[TestClass]
public class MaCoLoggingBuilderExtensionsTests
{
    [TestMethod]
    public void AddMaCoLogging_Parameterless_ReturnsBuilder()
    {
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddMaCoLogging();
        });
        Assert.IsNotNull(factory);
        factory.Dispose();
    }

    [TestMethod]
    public void AddMaCoLogging_WithConfig_ReturnsBuilder()
    {
        var config = new MaCoLoggerConfiguration
        {
            LogLevels = new List<LogLevel> { LogLevel.Warning, LogLevel.Error }
        };
        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddMaCoLogging(config);
        });
        Assert.IsNotNull(factory);
        factory.Dispose();
    }

    [TestMethod]
    public void AddMaCoLogging_WithIConfiguration_ReturnsBuilder()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Logging:MaCo:Enabled"] = "true"
            })
            .Build();

        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddMaCoLogging(config);
        });
        Assert.IsNotNull(factory);
        factory.Dispose();
    }

    [TestMethod]
    public void AddMaCoLogging_CreatesWorkingLogger()
    {
        Log.Instance.Settings.Enabled = true;
        Log.Instance.Settings.MessageTypes = LogMessageType.Information;
        Log.Instance.writeAdapter.Clear();
        var adapter = new InMemoryAdapter();
        Log.Instance.writeAdapter.Add(adapter);

        var factory = LoggerFactory.Create(builder =>
        {
            builder.AddMaCoLogging();
        });

        var logger = factory.CreateLogger("Test");
        logger.LogInformation("builder test");

        Assert.IsTrue(adapter.Entries.Any(e => e.Contains("builder test")));

        factory.Dispose();
        Log.Dispose();
    }
}
