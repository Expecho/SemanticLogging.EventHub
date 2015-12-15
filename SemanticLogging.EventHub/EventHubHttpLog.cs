using System;
using System.Diagnostics.Tracing;
using System.Threading;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;

namespace SemanticLogging.EventHub
{
    public static class EventHubHttpLog
    {
        /// <summary>
        /// Subscribes to an <see cref="IObservable{EventEntry}" /> using an <see cref="EventHubHttpSink" />.
        /// </summary>
        /// <param name="eventStream">The event stream. Typically this is an instance of <see cref="ObservableEventListener" />.</param>
        /// <param name="eventHubNamespace">The namespace of the eventhub.</param>
        /// <param name="eventHubName">The name of the eventhub.</param>
        /// <param name="publisherId">The id of the event publisher.</param>
        /// <param name="sasToken">The shared access signature token.</param>
        /// <param name="bufferingInterval">The buffering interval between each batch publishing.</param>
        /// <param name="bufferingCount">The number of entries that will trigger a batch publishing.</param>
        /// <param name="maxBufferSize">The maximum number of entries that can be buffered while it's sending to the store before the sink starts dropping entries.</param>      
        /// <param name="onCompletedTimeout">Defines a timeout interval for when flushing the entries after an <see cref="OnCompleted"/> call is received and before disposing the sink.
        /// This means that if the timeout period elapses, some event entries will be dropped and not sent to the store. Normally, calling <see cref="IDisposable.Dispose"/> on 
        /// the <see cref="System.Diagnostics.Tracing.EventListener"/> will block until all the entries are flushed or the interval elapses.
        /// If <see langword="null"/> is specified, then the call will block indefinitely until the flush operation finishes.</param>
        /// <returns>
        /// A subscription to the sink that can be disposed to unsubscribe the sink and dispose it, or to get access to the sink instance.
        /// </returns>
        public static SinkSubscription<EventHubHttpSink> LogToEventHubUsingHttp(this IObservable<EventEntry> eventStream,
            string eventHubNamespace, string eventHubName, string publisherId, string sasToken, TimeSpan? bufferingInterval = null, int bufferingCount = Buffering.DefaultBufferingCount, TimeSpan? onCompletedTimeout = null, int maxBufferSize = Buffering.DefaultMaxBufferSize)
        {
            var sink = new EventHubHttpSink(
                    eventHubNamespace,
                    eventHubName,
                    publisherId,
                    sasToken,
                    bufferingInterval ?? Buffering.DefaultBufferingInterval,
                    bufferingCount,
                    maxBufferSize,
                    onCompletedTimeout ?? Timeout.InfiniteTimeSpan
                );

            var subscription = eventStream.Subscribe(sink);
            return new SinkSubscription<EventHubHttpSink>(subscription, sink);
        }

        /// <summary>
        /// Creates an event listener that logs using an <see cref="EventHubHttpSink" />.
        /// </summary>
        /// <param name="eventHubNamespace">The namespace of the eventhub.</param>
        /// <param name="eventHubName">The name of the eventhub.</param>
        /// <param name="publisherId">The id of the event publisher.</param>
        /// <param name="sasToken">The shared access signature token.</param>
        /// <param name="bufferingInterval">The buffering interval between each batch publishing.</param>
        /// <param name="bufferingCount">The number of entries that will trigger a batch publishing.</param>
        /// <param name="maxBufferSize">The maximum number of entries that can be buffered while it's sending to the store before the sink starts dropping entries.</param>      
        /// <param name="onCompletedTimeout">Defines a timeout interval for when flushing the entries after an <see cref="OnCompleted"/> call is received and before disposing the sink.
        /// This means that if the timeout period elapses, some event entries will be dropped and not sent to the store. Normally, calling <see cref="IDisposable.Dispose"/> on 
        /// the <see cref="System.Diagnostics.Tracing.EventListener"/> will block until all the entries are flushed or the interval elapses.
        /// If <see langword="null"/> is specified, then the call will block indefinitely until the flush operation finishes.</param>
        /// <returns>
        /// An event listener that uses <see cref="EventHubHttpSink" /> to log events.
        /// </returns>
        public static EventListener CreateListener(string eventHubNamespace, string eventHubName, string publisherId, string sasToken, TimeSpan? bufferingInterval = null, int bufferingCount = Buffering.DefaultBufferingCount, TimeSpan? onCompletedTimeout = null, int maxBufferSize = Buffering.DefaultMaxBufferSize)
        {
            var listener = new ObservableEventListener();
            listener.LogToEventHubUsingHttp(eventHubNamespace, eventHubName, publisherId, sasToken, bufferingInterval, bufferingCount, onCompletedTimeout, maxBufferSize);
            return listener;
        }
    }
}
