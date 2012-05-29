using System;
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
    public class  NLogTracerWriterTests
    {
        private readonly NLogTraceWriter _writer;
        private readonly DebugTarget _debugTarget;

        public NLogTracerWriterTests()
        {
            // setup writer under test
            _writer = new NLogTraceWriter();

            // setup debug logging target
            _debugTarget = new DebugTarget { Layout = "${message}" };
            SimpleConfigurator.ConfigureForTargetLogging(_debugTarget, LogLevel.Info);
        }

        [Fact]
        public void Must_Call_TraceRecord_Action()
        {
            var called= false;
            var action = new Action<TraceRecord>(record => called = true);
            _writer.Trace(new HttpRequestMessage(), String.Empty, TraceLevel.Warn, action);
            called.Should().BeTrue();
        }

        [Fact]
        public void Must_Have_NonNull_TraceRecord()
        {
            TraceRecord record = null;
            var action = new Action<TraceRecord>(tr =>  record = tr);
            _writer.Trace(new HttpRequestMessage(), String.Empty, TraceLevel.Info, action);
            record.Should().NotBeNull();
        }

        [Fact]
        public void Must_Trace_To_Target()
        {
            WriteGeneralTrace();
            _debugTarget.Counter.Should().Be(1);
        }

        [Fact]
        public void Must_Trace_With_Correct_Message()
        {
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be("Test message");
        }

        [Fact]
        public void Must_Trace_With_Correct_Log_Level()
        {
            _debugTarget.Layout = "${level}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be("Info");
        }

        [Fact]
        public void Must_Trace_With_Correct_Category()
        {
            _debugTarget.Layout = "${logger}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be("MyCategory");
        }

        [Fact]
        public void Must_Not_Trace_If_Level_IsNot_Enabled()
        {
            _writer.Trace(new HttpRequestMessage(), String.Empty, TraceLevel.Debug,
                  record => { record.Message = "Test message"; });
            _debugTarget.Counter.Should().Be(0);
        }

        [Fact]
        public void Must_Not_Trace_If_Level_IsOff()
        {
            SimpleConfigurator.ConfigureForTargetLogging(_debugTarget, LogLevel.Off);
            _writer.Trace(new HttpRequestMessage(), String.Empty, TraceLevel.Fatal, null);
            _debugTarget.Counter.Should().Be(0);
        }

        [Fact]
        public void Must_Not_Trace_Exception()
        {
            _debugTarget.Layout = "${exception}";
            WriteGeneralTrace();
            _debugTarget.LastMessage.Should().Be(String.Empty);
        }

        [Fact]
        public void Must_Trace_Exception()
        {
            _debugTarget.Layout = "${exception}";
            _writer.Trace(new HttpRequestMessage(), String.Empty, TraceLevel.Fatal, record =>
            {
                record.Exception = new Exception("Oh noes");
            });

            _debugTarget.LastMessage.Should().Be("Oh noes");
        }

        [Fact]
        public void Must_Configure_Coupled_Renderers()
        {
            NLogTraceWriter.ConfigureRenderers();
            Type renderer = null;
            ConfigurationItemFactory.Default.LayoutRenderers.TryGetDefinition("webapi-trace", out renderer);
            renderer.Should().Be(typeof (WebApiTraceRenderer));

            ConfigurationItemFactory.Default.LayoutRenderers.TryGetDefinition("webapi-request", out renderer);
            renderer.Should().Be(typeof (WebApiRequestRenderer));
        }

        [Fact]
        public void Must_Throw_Exception_If_Exception_Thrown_In_User_Code()
        {
            Action act = () =>
            {
                _writer.Trace(new HttpRequestMessage(), String.Empty, TraceLevel.Fatal, record =>
                {
                    throw new Exception("I suck at tracing.");
                });
            };

            act.ShouldThrow<Exception>().WithMessage("I suck at tracing.");
            _debugTarget.Counter.Should().Be(1);
        }

        private void WriteGeneralTrace()
        {
            _writer.Trace(new HttpRequestMessage(), "MyCategory", TraceLevel.Info,
                          record => { record.Message = "Test message"; });
        }
    }
}
