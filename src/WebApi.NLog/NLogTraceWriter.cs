using System;
using System.Net.Http;
using System.Web.Http.Tracing;
using NLog;
using NLog.Config;

namespace WebApi.NLog
{
    /// <summary>
    ///  NLog implementation of webapi's ITraceWriter tracing extensibility point.
    ///   <remarks>More info about ITraceWriter, please see: "http://blogs.msdn.com/b/roncain/archive/2012/04/12/tracing-in-asp-net-web-api.aspx"</remarks>
    /// </summary>
    public class NLogTraceWriter : ITraceWriter
    {
        // property keys for renderer properties
        public const string RecordKey = "webapi_trace_record";
        public const string RequestKey = "webapi_request";

        public NLogTraceWriter() { }

        public static void ConfigureRenderers()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("webapi-trace", typeof(WebApiTraceRenderer));
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("webapi-request", typeof(WebApiRequestRenderer));
        }

        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            // set nlog logger - note: this category is bypassed; the logger name in the log event info is used instead
            var logger = LogManager.GetLogger(category);

            try
            {
                // only log and trace if enabled
                var logLevel = GetLogLevel(level);
                if (logger.IsEnabled(logLevel))
                {
                    // setup trace record
                    var record = new TraceRecord(request, category, level);

                    // user traces and populates record
                    traceAction(record);

                    // create the log event and log  - this will log to the configured nlog target, rules, etc. specified by the app
                    logger.Log(CreateEvent(record, logLevel));
                }
            }
            catch(Exception e)
            {
                // log exception - this exception usually occurs within the user's trace action code                
                logger.LogException(LogLevel.Error, "Failed to trace message due to an error.", e);
                throw;
            }
        }

        private LogEventInfo CreateEvent(TraceRecord record, LogLevel logLevel)
        {
            // create new log event info based off TraceRecord properties specified by app
            var logEvent = new LogEventInfo
                           {
                               Level = logLevel,
                               Message = record.Message,
                               LoggerName = record.Category,
                               Exception = record.Exception,
                               TimeStamp = record.Timestamp
                           };

            // load properties on the log event, this will be used downstream by the layout renderers
            PopulateProperties(record, logEvent);

            return logEvent;
        }

        private void PopulateProperties(TraceRecord record, LogEventInfo logEvent)
        {
            logEvent.Properties[RecordKey] = record;
            logEvent.Properties[RequestKey] = record.Request;
        }

        private LogLevel GetLogLevel(TraceLevel level)
        {
            // translates webapi log level to nlog log level
            return LogLevel.FromString(level.ToString());
        }


    }
}
