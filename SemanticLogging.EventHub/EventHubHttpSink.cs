using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
using Newtonsoft.Json;
using SemanticLogging.EventHub.Utility;

namespace SemanticLogging.EventHub
{
    public class EventHubHttpSink : IObserver<EventEntry>, IDisposable
    {
        private readonly string eventHubNamespace;
        private readonly string eventHubName;
        private readonly string publisherId;
        private readonly string sasToken;
        private readonly HttpClient httpClient = new HttpClient();
        private readonly BufferedEventPublisher<EventEntry> bufferedPublisher;
        private readonly TimeSpan onCompletedTimeout;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubHttpSink" /> class.
        /// </summary>
        /// <param name="eventHubNamespace">The namespace of the eventhub.</param>
        /// <param name="eventHubName">The name of the eventhub.</param>
        /// <param name="publisherId">The id fo the event pbulisher.</param>
        /// <param name="sasToken">The shared access signature token.</param>
        /// <param name="bufferingInterval">The buffering interval between each batch publishing.</param>
        /// <param name="bufferingCount">The number of entries that will trigger a batch publishing.</param>
        /// <param name="maxBufferSize">The maximum number of entries that can be buffered while it's sending to the store before the sink starts dropping entries.</param>      
        /// <param name="onCompletedTimeout">Defines a timeout interval for when flushing the entries after an <see cref="OnCompleted"/> call is received and before disposing the sink.
        /// This means that if the timeout period elapses, some event entries will be dropped and not sent to the store. Normally, calling <see cref="IDisposable.Dispose"/> on 
        /// the <see cref="System.Diagnostics.Tracing.EventListener"/> will block until all the entries are flushed or the interval elapses.
        /// If <see langword="null"/> is specified, then the call will block indefinitely until the flush operation finishes.</param>
        public EventHubHttpSink(string eventHubNamespace, string eventHubName, string publisherId, string sasToken, TimeSpan bufferingInterval, int bufferingCount, int maxBufferSize, TimeSpan onCompletedTimeout)
        {
            Guard.ArgumentNotNullOrEmpty(eventHubNamespace, "eventHubConnectionString");
            Guard.ArgumentNotNullOrEmpty(eventHubName, "eventHubName");
            Guard.ArgumentNotNullOrEmpty(publisherId, "partitionKey");
            Guard.ArgumentNotNullOrEmpty(sasToken, "sasToken");

            this.eventHubNamespace = eventHubNamespace;
            this.eventHubName = eventHubName;
            this.publisherId = publisherId;
            this.sasToken = sasToken;
            this.onCompletedTimeout = onCompletedTimeout;

            string sinkId = string.Format(CultureInfo.InvariantCulture, "EventHubHttpSink ({0})", Guid.NewGuid());
            bufferedPublisher = BufferedEventPublisher<EventEntry>.CreateAndStart(sinkId, PublishEventsAsync, bufferingInterval, bufferingCount, maxBufferSize, cancellationTokenSource.Token);

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", sasToken);
        }

        public void OnNext(EventEntry value)
        {
            bufferedPublisher.TryPost(value);
        }

        public void OnError(Exception error)
        {
            FlushSafe();
            Dispose();
        }

        public void OnCompleted()
        {
            FlushSafe();
            Dispose();
        }

        private async Task<int> PublishEventsAsync(IList<EventEntry> collection)
        {
            var publishedEventCount = collection.Count;

            try
            {
                HttpContent content;

                if(collection.Count == 1)
                {
                    content = CreateSingleMessageContent(collection.First());
                }
                else
                {
                    content = CreateBatchMessageContent(collection);
                }

                await PostMessageContentAsync(content);

                return publishedEventCount;
            }
            catch (OperationCanceledException)
            {
                return 0;
            }
            catch (Exception ex)
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return 0;
                }

                SemanticLoggingEventSource.Log.CustomSinkUnhandledFault(ex.ToString());
                throw;
            }
        }

        private async Task PostMessageContentAsync(HttpContent content)
        {
            var url = string.Format("https://{0}.servicebus.windows.net/{1}/publishers/{2}/Messages", eventHubNamespace, eventHubName, publisherId);

            try
            {
                var response = await httpClient.PostAsync(url, content);
                if (!response.IsSuccessStatusCode)
                    SemanticLoggingEventSource.Log.CustomSinkUnhandledFault(string.Format("The request failed with statuscode {0}: {1}", (int)response.StatusCode, response.ReasonPhrase));

            }
            catch (Exception ex)
            {
                SemanticLoggingEventSource.Log.CustomSinkUnhandledFault(ex.ToString());
                throw;
            }
        }

        private HttpContent CreateBatchMessageContent(IList<EventEntry> collection)
        {
            var messages = collection.Select(c => c.ToBatchMessage());

            var sendMessage = new ServiceBusHttpMessage
                                {
                                    Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messages))
                                };


            HttpContent postContent = new ByteArrayContent(sendMessage.Body);
            postContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.microsoft.servicebus.json");

            return postContent;
        }

        private HttpContent CreateSingleMessageContent(EventEntry eventEntry)
        {
            var payload = JsonConvert.SerializeObject(eventEntry);

            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            content.Headers.Add("ContentType", "application/atom+xml;type=entry;charset=utf-8");

            return content;
        }

        private void FlushSafe()
        {
            try
            {
                FlushAsync().Wait(onCompletedTimeout);
            }
            catch (AggregateException ex)
            {
                // Flush operation will already log errors. Never expose this exception to the observable.
                ex.Handle(e => e is FlushFailedException);
            }
        }

        /// <summary>
        /// Flushes the buffer content to <see cref="PublishEventsAsync"/>.
        /// </summary>
        /// <returns>The Task that flushes the buffer.</returns>
        public Task FlushAsync()
        {
            return bufferedPublisher.FlushAsync();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            cancellationTokenSource.Cancel();
            bufferedPublisher.Dispose();
            httpClient.Dispose();
        }
    }
}