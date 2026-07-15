using MaCo.Extensions.Logging;
using MaCo.Extensions.Logging.Classes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace MaCo.Extensions.Logging.Tests
{
    [TestClass()]
    public class LogTests
    {
        [TestMethod()]
        public void testILoggerBridgeFormatsMessage()
        {
            Log.Instance.Settings.Enabled = true;
            Log.Instance.Settings.MessageTypes =
                LogMessageType.Exception | LogMessageType.Warning |
                LogMessageType.Information | LogMessageType.DataLog;
            Log.Instance.writeAdapter.Clear();
            var adapter = new InMemoryAdapter();
            Log.Instance.writeAdapter.Add(adapter);

            var logger = new MaCo.Extensions.Logging.MaCoLogger("test", () => new MaCoLoggerConfiguration());
            logger.Log(LogLevel.Information, 0, "hello {Name}", null, (s, e) => $"FORM{s}");

            Assert.IsTrue(adapter.Entries.Any(x => x.Contains("FORMhello {Name}")),
                $"Expected formatted message, got: {string.Join(" | ", adapter.Entries)}");
        }

        private InMemoryAdapter _adapter = null!;

        [TestInitialize()]
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

        [TestCleanup()]
        public void Cleanup()
        {
            Log.Dispose();
        }

        [TestMethod()]
        public void testLog()
        {
            Log.Instance.WriteNew(LogMessageType.Information, "Information log detail.", "detail2", "detail3", "...");
            Log.Instance.WriteNew(LogMessageType.Warning, "Warning log detail.");
            Log.Instance.WriteNew(LogMessageType.DataLog, "DataLog log detail.");
            Log.Instance.WriteNew(LogMessageType.Exception, "Exception log detail.");

            try
            {
                throw new Exception("Error");
            }
            catch (Exception ex)
            {
                Log.Instance.WriteNew(ex);
                Log.Instance.WriteNew(ex, "Detail1", "Detail2", "...");
            }

            Assert.IsTrue(_adapter.Entries.Count >= 5,
                $"Expected at least 5 entries, got {_adapter.Entries.Count}");
            Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("Information log detail.")));
            Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("detail2") && e.Contains("detail3")));
            Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("Error")));
        }

        [TestMethod()]
        public void testLogSettings()
        {
            Log.Instance.Settings.LogType = LogType.WindowsLogEvent;
            Log.Instance.Settings.LogType = LogType.File;
            Log.Instance.Settings.LogKeepDataOnLimitRichedPercent = 80;
            Log.Instance.Settings.LogRowLimitPerContainer = 10000;

            try
            {
                throw new Exception("Error");
            }
            catch (Exception ex)
            {
                Log.Instance.WriteNew(ex);
                Log.Instance.WriteNew(ex, "Detail1", "Detail2", "...");
            }

            Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("Error")));
        }

        [TestMethod()]
        public void testIEnumerableDetailIsJoined()
        {
            var details = new object[] { "alpha", "beta", "gamma" };
            Log.Instance.WriteNew(LogMessageType.Information, "prefix", details);

            string entry = _adapter.Entries.First(e => e.Contains("prefix"));
            Assert.IsTrue(entry.Contains("prefix=>alpha=>beta=>gamma"));
        }
    }
}
