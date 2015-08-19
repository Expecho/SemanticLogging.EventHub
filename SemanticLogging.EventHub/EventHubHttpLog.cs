using System;
using System.Diagnostics.Tracing;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;

namespace EnterpriseLibrary.SemanticLogging.EventHub
{
    public static class EventHubHttpLog
    {
        /// <summary>
        /// Subscribes to an <see cref="IObservable{EventEntry}" /> using an <see cref="EventHubHttpSink" />.
        /// </summary>
        /// <param name="eventHubNamespace">The namespace of the eventhub.</param>
        /// <param name="eventHubName">The name of the eventhub.</param>
        /// <param name="publisherId">The id fo the event pbulisher.</param>
        /// <param name="sasToken">The shared access signature token.</param>
        /// <returns>
        /// A subscription to the sink that can be disposed to unsubscribe the sink and dispose it, or to get access to the sink instance.
        /// </returns>
        public static SinkSubscription<EventHubHttpSink> LogToEventHubUsingHttp(this IObservable<EventEntry> eventStream,
            string eventHubNamespace, string eventHubName, string publisherId, string sasToken)
        {
            var sink = new EventHubHttpSink(
                    eventHubNamespace,
                    eventHubName,
                    publisherId,
                    sasToken
                );

            var subscription = eventStream.Subscribe(sink);
            return new SinkSubscription<EventHubHttpSink>(subscription, sink);
        }

        /// <summary>
        /// Creates an event listener that logs using an <see cref="EventHubHttpSink" />.
        /// </summary>
        /// <param name="eventHubNamespace">The namespace of the eventhub.</param>
        /// <param name="eventHubName">The name of the eventhub.</param>
        /// <param name="publisherId">The id fo the event pbulisher.</param>
        /// <param name="sasToken">The shared access signature token.</param>
        /// <returns>
        /// An event listener that uses <see cref="EventHubHttpSink" /> to log events.
        /// </returns>
        public static EventListener CreateListener(string eventHubNamespace, string eventHubName, string publisherId, string sasToken)
        {
            var listener = new ObservableEventListener();
            listener.LogToEventHubUsingHttp(eventHubNamespace, eventHubName, publisherId, sasToken);
            return listener;
        }
    }
}
