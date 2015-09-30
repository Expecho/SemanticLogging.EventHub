using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;
using SemanticLogging.EventHub.Tests.Util;
using SemanticLogging.EventHub.Utility;

namespace SemanticLogging.EventHub.Tests
{
    [TestClass]
    public class EventHubHttpSinkTests
    {
        [TestMethod]
        public void ShouldFailForNullNamespace()
        {
            AssertEx.Throws<ArgumentNullException>(() => new EventHubHttpSink(null, "eventhubName", "pubId", "token", Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero));
        }

        [TestMethod]
        public void ShouldFailForNullName()
        {
            AssertEx.Throws<ArgumentNullException>(() => new EventHubHttpSink("eventHubNameNs", null, "pubId", "token", Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero));
        }

        [TestMethod]
        public void ShouldFailForNullPublisherId()
        {
            AssertEx.Throws<ArgumentNullException>(() => new EventHubHttpSink("eventHubNameNs", "eventhubName", null, "token", Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero));
        }

        [TestMethod]
        public void ShouldFailForNullSasToken()
        {
            AssertEx.Throws<ArgumentNullException>(() => new EventHubHttpSink("eventHubNameNs", "eventhubName", "pubId", null, Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero));
        }

        [TestMethod]
        public void ShouldNotStallOrThrowWhenHttpClientFails()
        {
            var httpClient = HttpClientTestHelper.Create();
            httpClient.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>()).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

            using (var sink = new EventHubHttpSink(httpClient, "eventHubNameNs", "eventhubName", "pubId", "token", Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero))
            using (var collectErrorsListener = new MockEventListener())
            {
                collectErrorsListener.EnableEvents(SemanticLoggingEventSource.Log, EventLevel.Error, Keywords.All);
                
                sink.OnNext(EventEntryTestHelper.Create());

                Assert.IsTrue(Task.Run(() => sink.OnCompleted()).Wait(TimeSpan.FromSeconds(5)));
                Assert.IsTrue(collectErrorsListener.WrittenEntries.Any(x => x.EventId == 1));
            }
        }

        [TestMethod]
        public void ShouldRaiseFlushFailedOnFlushAsyncWhenHttpClientFails()
        {
            var httpClient = HttpClientTestHelper.Create();
            httpClient.When(client => client.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>())).Do(action => { throw new Exception(); });

            using (var sink = new EventHubHttpSink(httpClient, "eventHubNameNs", "eventhubName", "pubId", "token", Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero))
            using (var collectErrorsListener = new MockEventListener())
            {
                collectErrorsListener.EnableEvents(SemanticLoggingEventSource.Log, EventLevel.Error, Keywords.All);
                
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
            var httpClient = HttpClientTestHelper.Create();
            httpClient.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>()).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            using (var sink = new EventHubHttpSink(httpClient, "eventHubNameNs", "eventhubName", "pubId", "token", Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero))
            {
                sink.OnNext(EventEntryTestHelper.Create());
                sink.OnNext(EventEntryTestHelper.Create());

                await sink.FlushAsync();
            }

            await httpClient.Received().PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>());
        }

        [TestMethod]
        public async Task ShouldWritePropertiesForSingleMessage()
        {
            var httpClient = HttpClientTestHelper.Create();
            httpClient.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>()).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            var entry = EventEntryTestHelper.Create();

            using (var sink = new EventHubHttpSink(httpClient, "eventHubNameNs", "eventhubName", "pubId", "token", Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero))
            {
                sink.OnNext(entry);

                await sink.FlushAsync();
            }

            var byteRepresentation = Encoding.Default.GetBytes(JsonConvert.SerializeObject(entry));
            await httpClient.Received().PostAsync(Arg.Any<string>(), Arg.Is<HttpContent>(c => byteRepresentation.SequenceEqual(c.ReadAsByteArrayAsync().Result)));
        }

        [TestMethod]
        public async Task ShouldNotUseBatchMessagesWhenBatchCountSetToOne()
        {
            var httpClient = HttpClientTestHelper.Create();
            httpClient.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>()).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            var entry = EventEntryTestHelper.Create();

            using (var sink = new EventHubHttpSink(httpClient, "eventHubNameNs", "eventhubName", "pubId", "token", Buffering.DefaultBufferingInterval, 1, 500, TimeSpan.Zero))
            {
                sink.OnNext(entry);
                sink.OnNext(entry);

                await sink.FlushAsync();
            }

            var byteRepresentation = Encoding.Default.GetBytes(JsonConvert.SerializeObject(entry));
            await httpClient.Received(2).PostAsync(Arg.Any<string>(), Arg.Is<HttpContent>(c => byteRepresentation.SequenceEqual(c.ReadAsByteArrayAsync().Result)));
        }

        [TestMethod]
        public async Task ShouldWritePropertiesForBatchMessage()
        {
            var httpClient = HttpClientTestHelper.Create();
            httpClient.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>()).Returns(Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            var entry = EventEntryTestHelper.Create();
            
            using (var sink = new EventHubHttpSink(httpClient, "eventHubNameNs", "eventhubName", "pubId", "token", Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero))
            {
                sink.OnNext(entry);
                sink.OnNext(entry);

                await sink.FlushAsync();
            }

            IList<EventEntry> entries = new List<EventEntry> { entry, entry };
            var messages = entries.Select(c => c.ToBatchMessage());
            var sendMessage = new ServiceBusHttpMessage
            {
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messages))
            };

            var byteRepresentation = sendMessage.Body;

            await httpClient.Received().PostAsync(Arg.Any<string>(), Arg.Is<HttpContent>(c => byteRepresentation.SequenceEqual(c.ReadAsByteArrayAsync().Result)));
        }

        [TestMethod]
        public void ShouldAddSasTokenToHeaders()
        {
            var httpClient = new HttpClientImp();
            const string authorizationScheme = "SharedAccessSignature";
            const string authorizationParameter = "sr=sasToken";
            var sasToken = string.Format("{0} {1}", authorizationScheme, authorizationParameter);

            using (new EventHubHttpSink(httpClient, "eventHubNameNs", "eventhubName", "pubId", sasToken, Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, TimeSpan.Zero))
            {
                Assert.AreEqual(authorizationScheme, httpClient.DefaultRequestHeaders.Authorization.Scheme);
                Assert.AreEqual(authorizationParameter, httpClient.DefaultRequestHeaders.Authorization.Parameter);
            }
        }
    }
}
