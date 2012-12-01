using System.Net.Http;
using System.Text;
using NLog;
using NLog.LayoutRenderers;

namespace WebApi.NLog
{
    /// <summary>
    /// Layout renderer for asp.net's web api traced http request message.
    /// </summary>
    [LayoutRenderer( "webapi-request" )]
    public class WebApiRequestRenderer : LayoutRenderer
    {

        /// <summary>
        ///  Request's route template 
        /// </summary>
        /// <example>${webapi-request:route=true}</example>
        public bool Route { get; set; }

        /// <summary>
        ///  Http method for request
        /// </summary>
        /// <example>${webapi-request:method=true}</example>
        public bool Method { get; set; }

        /// <summary>
        /// Absolute request Uri 
        /// </summary>
        /// <example>${webapi-request:uri=true}</example>
        public bool Uri { get; set; }

        /// <summary>
        ///  Request's Http Content
        /// </summary>
        /// <example>${webapi-request:content=true}</example>
        public bool Content { get; set; }

        protected override void Append( StringBuilder builder, LogEventInfo logEvent )
        {
            object value;
            if (!logEvent.Properties.TryGetValue( NLogTraceWriter.RequestKey, out value ))
                return;

            var request = value as HttpRequestMessage; ;
            if (request == null)
                return;

            if (request != null)
            {
                if (Route)
                    builder.Append( request.GetRouteData().Route.RouteTemplate );

                if (Method)
                    builder.Append( request.Method.Method );

                if (Uri)
                    builder.Append( request.RequestUri.AbsoluteUri );

                if (Content)
                    builder.Append( request.Content.ReadAsStringAsync().Result );

            }
        }
    }
}