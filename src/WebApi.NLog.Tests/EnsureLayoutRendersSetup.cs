using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using System.Web.Http.Tracing;
using FluentAssertions;
using NLog.Config;
using NLog.Targets;
using NLog;
using Xunit;
using TraceLevel = System.Web.Http.Tracing.TraceLevel;

namespace WebApi.NLog.Tests
{
    public static class EnsureLayoutRendersSetup
    {
        static EnsureLayoutRendersSetup()
        {
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition( "webapi-trace", typeof( WebApiTraceRenderer ) );
            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition( "webapi-request", typeof( WebApiRequestRenderer ) );
        }

        public static void Please()
        {
            // just force the static constructor to be run once...
        }
    }
}
