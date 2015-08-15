using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace EnterpriseLibrary.SemanticLogging.EventHub
{
    public class EventHubSink : IObserver<EventEntry>, IDisposable
    {
        private readonly EventHubClient eventHubClient;
        private readonly string partitionKey;
        private readonly BufferedEventPublisher<EventEntry> bufferedPublisher;
        private readonly TimeSpan onCompletedTimeout;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubSink" /> class.
        /// </summary>
        /// <param name="eventHubConnectionString">The connection string for the eventhub.</param>
        /// <param name="eventHubPath">The path of the eventhub.</param>
        /// <param name="bufferingInterval">The buffering interval between each batch publishing.</param>
        /// <param name="bufferingCount">The number of entries that will trigger a batch publishing.</param>
        /// <param name="maxBufferSize">The maximum number of entries that can be buffered while it's sending to the store before the sink starts dropping entries.</param>      
        /// <param name="onCompletedTimeout">Defines a timeout interval for when flushing the entries after an <see cref="OnCompleted"/> call is received and before disposing the sink.
        /// This means that if the timeout period elapses, some event entries will be dropped and not sent to the store. Normally, calling <see cref="IDisposable.Dispose"/> on 
        /// the <see cref="System.Diagnostics.Tracing.EventListener"/> will block until all the entries are flushed or the interval elapses.
        /// If <see langword="null"/> is specified, then the call will block indefinitely until the flush operation finishes.</param>
        /// <param name="partitionKey">PartitionKey is optional. If no partition key is supplied the log messages are sent to eventhub 
        /// and distributed to various partitions in a round robin manner.</param>
        public EventHubSink(string eventHubConnectionString, string eventHubPath, TimeSpan bufferingInterval, int bufferingCount, int maxBufferSize, TimeSpan onCompletedTimeout, string partitionKey = null)
        {
            var factory = MessagingFactory.CreateFromConnectionString(eventHubConnectionString + ";TransportType=Amqp");
            eventHubClient = factory.CreateEventHubClient(eventHubPath);

            this.partitionKey = partitionKey;
            this.onCompletedTimeout = onCompletedTimeout;
            
            string sinkId = string.Format(CultureInfo.InvariantCulture, "EventHubSink ({0})", Guid.NewGuid());
            bufferedPublisher = BufferedEventPublisher<EventEntry>.CreateAndStart(sinkId, PublishEventsAsync, bufferingInterval, bufferingCount, maxBufferSize, cancellationTokenSource.Token);
        }

        public void OnNext(EventEntry value)
        {
            bufferedPublisher.TryPost(value);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">A value indicating whether or not the class is disposing.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "cancellationTokenSource", Justification = "Token is cancelled")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                cancellationTokenSource.Cancel();
                bufferedPublisher.Dispose();
            }
        }

        public void OnCompleted()
        {
            FlushSafe();
            Dispose();
        }

        public void OnError(Exception error)
        {
            FlushSafe();
            Dispose();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="EventHubSink"/> class.
        /// </summary>
        ~EventHubSink()
        {
            Dispose(false);
        }

        /// <summary>
        /// Flushes the buffer content to <see cref="PublishEventsAsync"/>.
        /// </summary>
        /// <returns>The Task that flushes the buffer.</returns>
        public Task FlushAsync()
        {
            return bufferedPublisher.FlushAsync();
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="EventHubSink"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private async Task<int> PublishEventsAsync(IList<EventEntry> collection)
        {
            var publishedEventCount = collection.Count;

            try
            {
                var events = collection.Select(entry => 
                    new EventData(Encoding.Default.GetBytes(JsonConvert.SerializeObject(entry)))
                {
                    PartitionKey = partitionKey
                });

                await eventHubClient.SendBatchAsync(events);

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
    }
}