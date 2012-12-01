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
    public class WebApiRequestRendererTests
    {
        private readonly NLogTraceWriter _writer;
        private readonly DebugTarget _debugTarget;

        public WebApiRequestRendererTests()
        {
            EnsureLayoutRendersSetup.Please();

            // setup writer under test
            _writer = new NLogTraceWriter();

            // setup debug logging target
            _debugTarget = new DebugTarget { Layout = "${message}" };
            SimpleConfigurator.ConfigureForTargetLogging(_debugTarget, LogLevel.Info);
        }

        [Fact]
        public void Must_Render_Method()
        {
            _debugTarget.Layout = "${webapi-request:method=true}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be("GET");
        }

        [Fact]
        public void Must_Render_Uri()
        {
            _debugTarget.Layout = "${webapi-request:uri=true}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be("http://localhost/testing");
        }

        [Fact]
        public void Must_Render_Route()
        {
            _debugTarget.Layout = "${webapi-trace:route=true}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be("");
        }

        [Fact]
        public void Must_Render_Content()
        {
            _debugTarget.Layout = "${webapi-request:content=true}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be("Some Content");
        }

        [Fact]
        public void Must_Not_Render_When_False()
        {
            _debugTarget.Layout = "${webapi-request:content=false}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be(String.Empty);
        }

        [Fact]
        public void Must_Not_Render_When_Invalid_Property()
        {
            _debugTarget.Layout = "${webapi-request:idontknowwhatthisis=false}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be(String.Empty);
        }

        [Fact]
        public void Must_Render_All_Trace_Properties()
        {
            _debugTarget.Layout = "${webapi-request:method=true}-${webapi-request:uri=true}" +
                                  "-${webapi-request:content=true}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be("GET-http://localhost/testing-Some Content");
        }

        [Fact]
        public void Must_Not_Render_When_Property_IsNot_Present()
        {
            _debugTarget.Layout = "${webapi-request}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be(String.Empty);
        }

        private void WriteGeneralTrace()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/testing") { Content = new StringContent("Some Content") };

            _writer.Trace(request, "MyCategory", TraceLevel.Info,
                          record =>
                          {
                              record.Message = "Test message";
                              record.Operation = "DeleteMyThing";
                              record.Operator = "MyApi";
                              record.Status = HttpStatusCode.OK;
                          });
        }
    }

}
