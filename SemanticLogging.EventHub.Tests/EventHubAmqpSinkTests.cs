using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using SemanticLogging.EventHub.Tests.Util;

namespace SemanticLogging.EventHub.Tests
{
    [TestClass]
    public class EventHubAmqpSinkTests
    {
        [TestMethod]
        public void ShouldFailForNullConnectionString()
        {
            AssertEx.Throws<ArgumentNullException>(() => new EventHubAmqpSink(null, "eventHubName", Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero));
        }

        [TestMethod]
        public void ShouldFailForNullEventHubName()
        {
            AssertEx.Throws<ArgumentNullException>(() => new EventHubAmqpSink("connString", null, Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero));
        }

        [TestMethod]
        public void ShouldNotStallOrThrowWhenEventHubClientFails()
        {
            var eventHubClient = Substitute.For<IEventHubClient>();
            using (var sink = new EventHubAmqpSink(eventHubClient, Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero))
            using (var collectErrorsListener = new MockEventListener())
            {
                collectErrorsListener.EnableEvents(SemanticLoggingEventSource.Log, EventLevel.Error, Keywords.All);
                eventHubClient.When(client => client.SendBatchAsync(Arg.Any<IEnumerable<EventData>>()))
                    .Do(action => { throw new Exception(); });

                sink.OnNext(EventEntryTestHelper.Create());

                Assert.IsTrue(Task.Run(() => sink.OnCompleted()).Wait(TimeSpan.FromSeconds(5)));
                Assert.IsTrue(collectErrorsListener.WrittenEntries.Any(x => x.EventId == 1));
            }
        }
    }
}
