using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
using Microsoft.ServiceBus.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
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

        [TestMethod]
        public void ShouldRaiseFlushFailedOnFlushAsyncWhenEventHubClientFails()
        {
            var eventHubClient = Substitute.For<IEventHubClient>();
            using (var sink = new EventHubAmqpSink(eventHubClient, Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero))
            using (var collectErrorsListener = new MockEventListener())
            {
                collectErrorsListener.EnableEvents(SemanticLoggingEventSource.Log, EventLevel.Error, Keywords.All);
                eventHubClient.When(client => client.SendBatchAsync(Arg.Any<IEnumerable<EventData>>()))
                    .Do(action => { throw new Exception(); });

                sink.OnNext(EventEntryTestHelper.Create());

                try
                {
                    sink.FlushAsync().Wait();
                    Assert.Fail("AggregateException should be thrown.");
                }
                catch (AggregateException ex)
                {
                    Assert.IsInstanceOfType(ex.InnerException, typeof(FlushFailedException));
                }
                
                Assert.IsTrue(collectErrorsListener.WrittenEntries.Any(x => x.EventId == 1));
            }
        }

        [TestMethod]
        public async Task ShouldWriteEntriesOnFlushAsync()
        {
            var eventHubClient = Substitute.For<IEventHubClient>();
            using (var sink = new EventHubAmqpSink(eventHubClient, Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero))
            {
                sink.OnNext(EventEntryTestHelper.Create());
                sink.OnNext(EventEntryTestHelper.Create());

                await sink.FlushAsync();
            }

            eventHubClient.Received().SendBatchAsync(Arg.Is<IEnumerable<EventData>>(l => l.Count() == 2)).Wait();
        }

        [TestMethod]
        public async Task ShouldWriteProperties()
        {
            var eventHubClient = Substitute.For<IEventHubClient>();
            var entry = EventEntryTestHelper.Create();
            using (var sink = new EventHubAmqpSink(eventHubClient, Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero))
            {
                sink.OnNext(entry);
                
                await sink.FlushAsync();
            }

            var byteRepresentation = Encoding.Default.GetBytes(JsonConvert.SerializeObject(entry));

            eventHubClient.Received().SendBatchAsync(Arg.Is<IEnumerable<EventData>>(l => byteRepresentation.SequenceEqual(l.First().GetBytes()))).Wait();
        }

        [TestMethod]
        public async Task ShouldWritePartitionKey()
        {
            var eventHubClient = Substitute.For<IEventHubClient>();
            var entry = EventEntryTestHelper.Create();
            const string partitionKey = "PartitionKey";

            using (var sink = new EventHubAmqpSink(eventHubClient, Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero, partitionKey))
            {
                sink.OnNext(entry);

                await sink.FlushAsync();
            }

            eventHubClient.Received().SendBatchAsync(Arg.Is<IEnumerable<EventData>>(l => l.First().PartitionKey == partitionKey)).Wait();
        }
    }
}
