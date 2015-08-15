using System;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;

namespace EnterpriseLibrary.SemanticLogging.EventHub
{
    public static class EventHubHttpLog
    {
        /// <summary>
        /// Subscribes to an <see cref="IObservable{EventEntry}" /> using a <see cref="EventHubHttpSink" />.
        /// </summary>
        /// <param name="eventHubNamespace">The namespace of the eventhub.</param>
        /// <param name="eventHubName">The name of the eventhub.</param>
        /// <returns>
        /// A subscription to the sink that can be disposed to unsubscribe the sink and dispose it, or to get access to the sink instance.
        /// </returns>
        public static SinkSubscription<EventHubHttpSink> LogToEventHubUsingHttp(this IObservable<EventEntry> eventStream,
            string eventHubNamespace, string eventHubName, string partitionKey = null)
        {
            var sink = new EventHubHttpSink(
                    eventHubNamespace,
                    eventHubName,
                    partitionKey
                );

            var subscription = eventStream.Subscribe(sink);
            return new SinkSubscription<EventHubHttpSink>(subscription, sink);
        }
    }
}
