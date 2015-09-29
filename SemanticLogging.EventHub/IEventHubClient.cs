using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace SemanticLogging.EventHub
{
    public interface IEventHubClient
    {
        Task SendBatchAsync(IEnumerable<EventData> eventDataList);
    }
}