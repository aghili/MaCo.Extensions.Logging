using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aghili.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aghili.Logging.Tests
{
    [TestClass()]
    public class LogTests
    {
        [TestMethod()]
        public async Task testLog()
        {
            Log.Instance.WriteNew(LogMesssageType.Information, "Information log detail.", "detail2", "detail3","...");
            Log.Instance.WriteNew(LogMesssageType.Warrning, "Warrning log detail.");
            Log.Instance.WriteNew(LogMesssageType.DataLog, "DataLog log detail.");
            Log.Instance.WriteNew(LogMesssageType.Exception, "Exception log detail.");

            try
            {
                throw new Exception("Error");
            }
            catch (Exception ex)
            {
                Log.Instance.WriteNew(ex);
                Log.Instance.WriteNew(ex, "Detail1", "Detail2", "...");
            }
            Thread.Sleep(5000);
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
            Thread.Sleep(5000);
        }
    }
}