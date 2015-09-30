using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace SemanticLogging.EventHub
{
    internal class EventHubClientImp : IEventHubClient
    {
        private readonly EventHubClient client;

        public EventHubClientImp(EventHubClient client)
        {
            this.client = client;
        }

        public Task SendBatchAsync(IEnumerable<EventData> eventDataList)
        {
            return client.SendBatchAsync(eventDataList);
        }
    }
}