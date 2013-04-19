using System;
using System.Text;
using System.Web.Http.Tracing;
using NLog;
using NLog.LayoutRenderers;

namespace WebApi.NLog
{
    /// <summary>
    /// Layout renderer for asp.net's web api trace record.
    /// </summary>
    [LayoutRenderer( "webapi-trace" )]
    public class WebApiTraceRenderer : LayoutRenderer
    {
        /// <summary>
        ///  Name of the operation being performed during the trace. (e.g., Method)
        /// </summary>
        /// <example>${webapi-trace:operation:true}</example>
        public bool Operation { get; set; }

        /// <summary>
        ///  Http Status Code (e.g, 200) 
        /// </summary>
        /// <example>${webapi-trace:statuscode:true}</example>
        public bool StatusCode { get; set; }

        /// <summary>
        /// Http Status Code Description (e.g., OK)
        /// </summary>
        /// <example>${webapi-trace:status:true}</example>
        public bool Status { get; set; }

        /// <summary>
        ///  Trace Request Guid; used for correlation. 
        /// </summary>
        /// <example>${webapi-trace:requestid:true}</example>
        public bool RequestId { get; set; }

        /// <summary>
        ///  Name of the Operator of the Trace (e.g, MyApiController) 
        /// </summary>
        /// <example>${webapi-trace:operator:true}</example>
        public bool Operator { get; set; }

        /// <summary>
        ///  Type of Trace (i.e, Begin, End, Trace)
        /// </summary>
        /// <example>${webapi-trace:kind:true}</example>
        public bool Kind { get; set; }

        protected override void Append( StringBuilder builder, LogEventInfo logEvent )
        {
            object value;
            if (!logEvent.Properties.TryGetValue( NLogTraceWriter.RecordKey, out value ))
                return;

            var record = value as TraceRecord;
            if (record == null)
                return;


            if (Operation)
                builder.Append( record.Operation );

            if (Status)
                builder.Append( record.Status.ToString() );

            if (StatusCode)
                builder.Append( Convert.ToInt32( record.Status ) );

            if (RequestId)
                builder.Append( record.RequestId );

            if (Operator)
                builder.Append( record.Operator );

            if (Kind)
                builder.Append( record.Kind.ToString() );

        }
    }
}