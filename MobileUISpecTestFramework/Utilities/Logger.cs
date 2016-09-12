using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Config;


namespace MobileUISpecTestFramework.Utilities
{
    public enum LogLevel
    {
        DEBUG = 1,
        ERROR,
        FATAL,
        INFO,
        WARN
    }
    public static class Logger
    {
        #region Members
        private static readonly ILog logger = LogManager.GetLogger(typeof(Logger));
        #endregion

        #region Constructor
        static Logger()
        {
            XmlConfigurator.Configure();
        }
        #endregion

        #region Methods
        public static void WriteLog(LogLevel logLevel, string log)
        {
            if (logLevel.Equals(LogLevel.DEBUG))
            {
                logger.Debug(log);
            }
            if (logLevel.Equals(LogLevel.ERROR))
            {
                logger.Error(log);
            }
            if (logLevel.Equals(LogLevel.FATAL))
            {
                logger.Fatal(log);
            }
            if (logLevel.Equals(LogLevel.INFO))
            {
                logger.Info(log);
            }
            if (logLevel.Equals(LogLevel.WARN))
            {
                logger.Warn(log);
            }
        }
        #endregion
    }
}
