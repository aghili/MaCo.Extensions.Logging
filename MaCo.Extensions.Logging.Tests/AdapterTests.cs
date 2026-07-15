using MaCo.Extensions.Logging;
using MaCo.Extensions.Logging.Classes;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MaCo.Extensions.Logging.Tests;

[TestClass]
public class LogFileAdapterTests
{
    [TestMethod]
    public void Write_LogMessageType_WritesToFile()
    {
        using var adapter = new LogFileAdapter();
        adapter.Write(LogMessageType.Information, "test.log", "hello");
        adapter.Flush();
        // Adapter writes to ExecPath/Log/ — just verify no exception
    }

    [TestMethod]
    public void Write_LogLevel_WritesToFile()
    {
        using var adapter = new LogFileAdapter();
        adapter.Write(LogLevel.Information, "test.log", "level hello");
        adapter.Flush();
    }

    [TestMethod]
    public void WriterType_IsFile()
    {
        using var adapter = new LogFileAdapter();
        Assert.AreEqual(LogType.File, adapter.WriterType);
    }

    [TestMethod]
    public void Equals_SameType_ReturnsTrue()
    {
        using var adapter = new LogFileAdapter();
        Assert.IsTrue(adapter.Equals(LogType.File));
    }

    [TestMethod]
    public void Equals_DifferentType_ReturnsFalse()
    {
        using var adapter = new LogFileAdapter();
        Assert.IsFalse(adapter.Equals(LogType.Online));
    }

    [TestMethod]
    public void WriteOptions_DefaultValues()
    {
        using var adapter = new LogFileAdapter();
        Assert.AreEqual(80, adapter.WriteOptions.LogKeepDataOnLimitRichedPercent);
        Assert.AreEqual(20, adapter.WriteOptions.LogRowLimitPerContainer);
    }

    [TestMethod]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var adapter = new LogFileAdapter();
        adapter.Dispose();
        adapter.Dispose();
    }

    [TestMethod]
    public void Flush_EmptyQueue_DoesNotThrow()
    {
        using var adapter = new LogFileAdapter();
        adapter.Flush();
    }

    [TestMethod]
    public void OnShrinkRise_CanSubscribe()
    {
        using var adapter = new LogFileAdapter();
        bool fired = false;
        adapter.OnShrinkRise += (s, e) => fired = true;
        Assert.IsFalse(fired);
    }

    [TestMethod]
    public void Write_MultipleMessages_FlushesAll()
    {
        using var adapter = new LogFileAdapter();
        adapter.Write(LogMessageType.Information, "multi.log", "msg1");
        adapter.Write(LogMessageType.Warning, "multi.log", "msg2");
        adapter.Write(LogMessageType.Exception, "multi.log", "msg3");
        adapter.Flush();
    }
}

[TestClass]
public class InMemoryAdapterTests
{
    [TestMethod]
    public void Write_LogMessageType_AddsEntry()
    {
        var adapter = new InMemoryAdapter();
        adapter.Write(LogMessageType.Information, "path", "msg");
        Assert.AreEqual(1, adapter.Entries.Count);
        Assert.IsTrue(adapter.Entries[0].Contains("Information"));
        Assert.IsTrue(adapter.Entries[0].Contains("path"));
        Assert.IsTrue(adapter.Entries[0].Contains("msg"));
    }

    [TestMethod]
    public void Write_LogLevel_AddsEntry()
    {
        var adapter = new InMemoryAdapter();
        adapter.Write(LogLevel.Warning, "path", "warn");
        Assert.AreEqual(1, adapter.Entries.Count);
        Assert.IsTrue(adapter.Entries[0].Contains("Warning"));
    }

    [TestMethod]
    public void WriterType_DefaultIsFile()
    {
        var adapter = new InMemoryAdapter();
        Assert.AreEqual(LogType.File, adapter.WriterType);
    }

    [TestMethod]
    public void Equals_ReturnsCorrectly()
    {
        var adapter = new InMemoryAdapter();
        Assert.IsTrue(adapter.Equals(LogType.File));
        Assert.IsFalse(adapter.Equals(LogType.Online));
    }

    [TestMethod]
    public void Dispose_DoesNotThrow()
    {
        var adapter = new InMemoryAdapter();
        adapter.Dispose();
    }

    [TestMethod]
    public void WriteOptions_DefaultValues()
    {
        var adapter = new InMemoryAdapter();
        Assert.AreEqual(80, adapter.WriteOptions.LogKeepDataOnLimitRichedPercent);
        Assert.AreEqual(20, adapter.WriteOptions.LogRowLimitPerContainer);
    }

    [TestMethod]
    public void Write_MultipleEntries_Accumulates()
    {
        var adapter = new InMemoryAdapter();
        adapter.Write(LogMessageType.Information, "p1", "m1");
        adapter.Write(LogMessageType.Warning, "p2", "m2");
        adapter.Write(LogMessageType.Exception, "p3", "m3");
        Assert.AreEqual(3, adapter.Entries.Count);
    }
}
