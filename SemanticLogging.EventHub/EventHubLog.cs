using System;
using System.Diagnostics.Tracing;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.EventHub;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;

namespace Microsoft.Practices.EnterpriseLibrary.SemanticLogging
{
    /// <summary>
    /// Factories and helpers for using the <see cref="EventHubSink"/>.
    /// </summary>
    public static class EventHubLog
    {
        /// <summary>
        /// Subscribes to an <see cref="IObservable{EventEntry}" /> using a <see cref="EventHubSink" />.
        /// </summary>
        /// <param name="eventStream">The event stream. Typically this is an instance of <see cref="ObservableEventListener" />.</param>
        /// <param name="eventHubConnectionString">The connection string for the eventhub.</param>
        /// <param name="EventHubPath">The path of the eventhub.</param>
        /// <param name="bufferingInterval">The buffering interval between each batch publishing. Default value is <see cref="Buffering.DefaultBufferingInterval" />.</param>
        /// <param name="bufferingCount">The number of entries that will trigger a batch publishing.</param>
        /// <param name="onCompletedTimeout">Defines a timeout interval for when flushing the entries after an <see cref="EventHubSink.OnCompleted" /> call is received and before disposing the sink.</param>
        /// <param name="maxBufferSize">The maximum number of entries that can be buffered while it's sending to Azure EventHub before the sink starts dropping entries.
        /// This means that if the timeout period elapses, some event entries will be dropped and not sent to the store. Normally, calling <see cref="IDisposable.Dispose" /> on
        /// the <see cref="System.Diagnostics.Tracing.EventListener" /> will block until all the entries are flushed or the interval elapses.
        /// If <see langword="null" /> is specified, then the call will block indefinitely until the flush operation finishes.</param>
        /// <param name="partitionKey">PartitionKey is optional. If no partition key is supplied the log messages are sent to eventhub 
        /// and distributed to various partitions in a round robin manner.</param>
        /// <returns>
        /// A subscription to the sink that can be disposed to unsubscribe the sink and dispose it, or to get access to the sink instance.
        /// </returns>
        public static SinkSubscription<EventHubSink> LogToEventHub(this IObservable<EventEntry> eventStream, string eventHubConnectionString, string EventHubPath, TimeSpan? bufferingInterval = null, int bufferingCount = Buffering.DefaultBufferingCount, TimeSpan? onCompletedTimeout = null, int maxBufferSize = Buffering.DefaultMaxBufferSize, string partitionKey = null)
        {
            var sink = new EventHubSink(
                eventHubConnectionString,
                EventHubPath,
                bufferingInterval ?? Buffering.DefaultBufferingInterval,
                bufferingCount,
                maxBufferSize,
                onCompletedTimeout ?? Timeout.InfiniteTimeSpan,
                partitionKey);

            var subscription = eventStream.Subscribe(sink);
            return new SinkSubscription<EventHubSink>(subscription, sink);
        }

        /// <summary>
        /// Creates an event listener that logs using a <see cref="EventHubSink" />.
        /// </summary>
        /// <param name="eventHubConnectionString">The connection string for the eventhub.</param>
        /// <param name="EventHubPath">The path of the eventhub.</param>
        /// <param name="bufferingInterval">The buffering interval between each batch publishing.</param>
        /// <param name="bufferingCount">The number of entries that will trigger a batch publishing.</param>
        /// <param name="listenerDisposeTimeout">Defines a timeout interval for the flush operation when the listener is disposed.</param>
        /// <param name="maxBufferSize">The maximum number of entries that can be buffered while it's sending to Azure EventHub before the sink starts dropping entries.
        /// This means that if the timeout period elapses, some event entries will be dropped and not sent to the store. Calling <see cref="IDisposable.Dispose" /> on
        /// the <see cref="EventListener" /> will block until all the entries are flushed or the interval elapses.
        /// If <see langword="null" /> is specified, then the call will block indefinitely until the flush operation finishes.</param>
        /// <param name="partitionKey">PartitionKey is optional. If no partition key is supplied the log messages are sent to eventhub 
        /// and distributed to various partitions in a round robin manner.</param>
        /// <returns>
        /// An event listener that uses <see cref="WindowsAzureTableSink" /> to log events.
        /// </returns>
        public static EventListener CreateListener(string eventHubConnectionString, string EventHubPath, TimeSpan? bufferingInterval = null, int bufferingCount = Buffering.DefaultBufferingCount, TimeSpan? listenerDisposeTimeout = null, int maxBufferSize = Buffering.DefaultMaxBufferSize, string partitionKey = null)
        {
            var listener = new ObservableEventListener();
            listener.LogToEventHub(eventHubConnectionString, EventHubPath, bufferingInterval, bufferingCount, listenerDisposeTimeout, maxBufferSize, partitionKey);
            return listener;
        }
    }
}
