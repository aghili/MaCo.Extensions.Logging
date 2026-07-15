using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

// =============================================================================
// Tests using the DEPRECATED Aghili.Logging namespace.
// These verify backward compatibility with v1.x API surface.
// =============================================================================

#pragma warning disable CS0618 // Type or member is obsolete

namespace Aghili.Logging.Tests
{
    [TestClass()]
    public class DeprecatedApiTests
    {
        private MaCo.Extensions.Logging.Tests.InMemoryAdapter _adapter = null!;

        [TestInitialize()]
        public void Init()
        {
            MaCo.Extensions.Logging.Log.Instance.Settings.Enabled = true;
            MaCo.Extensions.Logging.Log.Instance.Settings.MessageTypes =
                MaCo.Extensions.Logging.LogMessageType.Exception |
                MaCo.Extensions.Logging.LogMessageType.Warning |
                MaCo.Extensions.Logging.LogMessageType.Information |
                MaCo.Extensions.Logging.LogMessageType.DataLog;
            MaCo.Extensions.Logging.Log.Instance.writeAdapter.Clear();
            _adapter = new MaCo.Extensions.Logging.Tests.InMemoryAdapter();
            MaCo.Extensions.Logging.Log.Instance.writeAdapter.Add(_adapter);
        }

        [TestCleanup()]
        public void Cleanup()
        {
            MaCo.Extensions.Logging.Log.Dispose();
        }

        [TestMethod()]
        public void TestDeprecatedLogMesssageType()
        {
            // Use old enum name LogMesssageType
            LogMesssageType type = LogMesssageType.Exception;
            Assert.AreEqual(1, (int)type);

            LogMesssageType warning = LogMesssageType.Warrning;
            Assert.AreEqual(2, (int)warning);

            LogMesssageType info = LogMesssageType.Information;
            Assert.AreEqual(4, (int)info);

            LogMesssageType dataLog = LogMesssageType.DataLog;
            Assert.AreEqual(8, (int)dataLog);
        }

        [TestMethod()]
        public void TestDeprecatedLogMesssageTypeConversion()
        {
            // Test explicit conversion from old to new
            LogMesssageType oldType = LogMesssageType.Exception;
            MaCo.Extensions.Logging.LogMessageType newType = (MaCo.Extensions.Logging.LogMessageType)(int)oldType;
            Assert.AreEqual(MaCo.Extensions.Logging.LogMessageType.Exception, newType);

            // Test explicit conversion from new to old
            MaCo.Extensions.Logging.LogMessageType newType2 = MaCo.Extensions.Logging.LogMessageType.Warning;
            LogMesssageType oldType2 = (LogMesssageType)(int)newType2;
            Assert.AreEqual(LogMesssageType.Warrning, oldType2);
        }

        [TestMethod()]
        public void TestDeprecatedLogInstance()
        {
            // Use old Aghili.Logging.Log.Instance
            var instance = Log.Instance;
            Assert.IsNotNull(instance);
            Assert.IsTrue(instance.Settings.Enabled);
        }

        [TestMethod()]
        public void TestDeprecatedWriteNew()
        {
            // Use old LogMesssageType enum with WriteNew (explicit cast)
            Log.Instance.WriteNew((MaCo.Extensions.Logging.LogMessageType)(int)LogMesssageType.Information, "Deprecated info message");
            Log.Instance.WriteNew((MaCo.Extensions.Logging.LogMessageType)(int)LogMesssageType.Warrning, "Deprecated warning message");
            Log.Instance.WriteNew((MaCo.Extensions.Logging.LogMessageType)(int)LogMesssageType.DataLog, "Deprecated data log");
            Log.Instance.WriteNew((MaCo.Extensions.Logging.LogMessageType)(int)LogMesssageType.Exception, "Deprecated exception");

            Assert.IsTrue(_adapter.Entries.Count >= 4,
                $"Expected at least 4 entries, got {_adapter.Entries.Count}");
            Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("Deprecated info message")));
            Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("Deprecated warning message")));
            Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("Deprecated data log")));
            Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("Deprecated exception")));
        }

        [TestMethod()]
        public void TestDeprecatedExceptionOverload()
        {
            try
            {
                throw new InvalidOperationException("Test error");
            }
            catch (Exception ex)
            {
                Log.Instance.WriteNew(ex, "Additional context");
            }

            Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("Test error")));
            Assert.IsTrue(_adapter.Entries.Any(e => e.Contains("Additional context")));
        }

        [TestMethod()]
        public void TestDeprecatedSettingsMesssageTypes()
        {
            // Use old MesssageTypes property via extension method
            var settings = Log.Instance.Settings;
            LogMesssageType current = settings.GetMesssageTypes();
            Assert.IsTrue(current.HasFlag(LogMesssageType.Exception));
            Assert.IsTrue(current.HasFlag(LogMesssageType.Warrning));

            // Set using old property name via extension method
            settings.SetMesssageTypes(LogMesssageType.Information | LogMesssageType.DataLog);
            LogMesssageType updated = settings.GetMesssageTypes();
            Assert.IsTrue(updated.HasFlag(LogMesssageType.Information));
            Assert.IsTrue(updated.HasFlag(LogMesssageType.DataLog));
            Assert.IsFalse(updated.HasFlag(LogMesssageType.Exception));
        }

        [TestMethod()]
        public void TestDeprecatedShirinkEventArgs()
        {
            // Use old ShirinkEventArgs class
            var args = new Aghili.Logging.Classes.ShirinkEventArgs();
            args.Type = MaCo.Extensions.Logging.Classes.ShrinkType.Backup;
            args.RecordCount = 100;
            args.NewRecordCount = 0;

            Assert.AreEqual(MaCo.Extensions.Logging.Classes.ShrinkType.Backup, args.Type);
            Assert.AreEqual(100, args.RecordCount);
        }

        [TestMethod()]
        public void TestDeprecatedShirinkType()
        {
            // Use old ShirinkType enum
            Aghili.Logging.Classes.ShirinkType backup = Aghili.Logging.Classes.ShirinkType.Backup;
            Aghili.Logging.Classes.ShirinkType resize = Aghili.Logging.Classes.ShirinkType.Resize;

            Assert.AreEqual(0, (int)backup);
            Assert.AreEqual(1, (int)resize);
        }

        [TestMethod()]
        public void TestDeprecatedShirinkTypeConversion()
        {
            // Test explicit conversion from old to new
            Aghili.Logging.Classes.ShirinkType oldType = Aghili.Logging.Classes.ShirinkType.Backup;
            MaCo.Extensions.Logging.Classes.ShrinkType newType = (MaCo.Extensions.Logging.Classes.ShrinkType)(int)oldType;
            Assert.AreEqual(MaCo.Extensions.Logging.Classes.ShrinkType.Backup, newType);

            // Test explicit conversion from new to old
            MaCo.Extensions.Logging.Classes.ShrinkType newType2 = MaCo.Extensions.Logging.Classes.ShrinkType.Resize;
            Aghili.Logging.Classes.ShirinkType oldType2 = (Aghili.Logging.Classes.ShirinkType)(int)newType2;
            Assert.AreEqual(Aghili.Logging.Classes.ShirinkType.Resize, oldType2);
        }

        [TestMethod()]
        public void TestDeprecatedLogDispose()
        {
            // Use old Log.Dispose()
            Log.Dispose();
            // Should not throw
        }

        [TestMethod()]
        public void TestDeprecatedEnumFlags()
        {
            // Test flag combinations with old enum
            LogMesssageType combined = LogMesssageType.Exception | LogMesssageType.Warrning | LogMesssageType.Information;
            Assert.IsTrue(combined.HasFlag(LogMesssageType.Exception));
            Assert.IsTrue(combined.HasFlag(LogMesssageType.Warrning));
            Assert.IsTrue(combined.HasFlag(LogMesssageType.Information));
            Assert.IsFalse(combined.HasFlag(LogMesssageType.DataLog));
        }

        [TestMethod()]
        public void TestDeprecatedEnumFlagsWithConversion()
        {
            // Test flag operations work through explicit conversion
            LogMesssageType oldCombined = LogMesssageType.Exception | LogMesssageType.Warrning;
            MaCo.Extensions.Logging.LogMessageType newType = (MaCo.Extensions.Logging.LogMessageType)(int)oldCombined;
            Assert.IsTrue(newType.HasFlag(MaCo.Extensions.Logging.LogMessageType.Exception));
            Assert.IsTrue(newType.HasFlag(MaCo.Extensions.Logging.LogMessageType.Warning));
        }
    }
}

#pragma warning restore CS0618 // Type or member is obsolete
