using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Tracing;
using FluentAssertions;
using NLog.Config;
using NLog.Targets;
using NLog;
using Xunit;
using TraceLevel = System.Web.Http.Tracing.TraceLevel;

namespace WebApi.NLog.Tests
{
    public class  WebApiTraceRendererTests
    {
        private readonly NLogTraceWriter _writer;
        private readonly DebugTarget _debugTarget;

        public WebApiTraceRendererTests()
        {
            // setup writer under test
            _writer = new NLogTraceWriter();

            // setup debug logging target
            _debugTarget = new DebugTarget { Layout = "${message}" };
            SimpleConfigurator.ConfigureForTargetLogging(_debugTarget, LogLevel.Info);
        }

        [Fact]
        public void Must_Render_Operation()
        {
           _debugTarget.Layout = "${webapi-trace:operation=true}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be("DeleteMyThing");
        }

        [Fact]
        public void Must_Render_Operator()
        {
            _debugTarget.Layout = "${webapi-trace:operator=true}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be("MyApi");
        }

        [Fact]
        public void Must_Render_Status()
        {
            _debugTarget.Layout = "${webapi-trace:status=true}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be("OK");
        }

        [Fact]
        public void Must_Render_Status_Code()
        {
            _debugTarget.Layout = "${webapi-trace:statuscode=true}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be("200");
        }

        [Fact]
        public void Must_Render_Kind()
        {
            _debugTarget.Layout = "${webapi-trace:kind=true}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be("Trace");
        }

        [Fact]
        public void Must_Render_RequestId()
        {
            _debugTarget.Layout = "${webapi-trace:requestid=true}";
            var id = WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be(id.ToString());
        }

        [Fact]
        public void Must_Not_Render_When_False()
        {
            _debugTarget.Layout = "${webapi-trace:kind=false}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be(String.Empty);
        }

        [Fact]
        public void Must_Not_Render_When_Invalid_Property()
        {
            _debugTarget.Layout = "${webapi-trace:idontknowwhatthisis=false}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be(String.Empty);
        }

        [Fact]
        public void Must_Render_All_Trace_Properties()
        {
            _debugTarget.Layout = "${webapi-trace:operation=true}-${webapi-trace:operator=true}" +
                                  "-${webapi-trace:status=true:statuscode=true}-${webapi-trace:kind=true}|${webapi-trace:requestid=true}|";
            var id = WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be("DeleteMyThing-MyApi-OK200-Trace|" + id.ToString() + "|");
        }

        [Fact]
        public void Must_Not_Render_When_Property_IsNot_Present()
        {
            _debugTarget.Layout = "${webapi-trace}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be(String.Empty);
        }

        private Guid WriteGeneralTrace()
        {
            var requestId = new Guid();
            _writer.Trace(new HttpRequestMessage(), "MyCategory", TraceLevel.Info,
                          record =>
                          {
                              record.Message = "Test message";
                              record.Operation = "DeleteMyThing";
                              record.Operator = "MyApi";
                              record.Status = HttpStatusCode.OK;
                              requestId = record.RequestId;
                          });
            return requestId;
        }
    }
}
