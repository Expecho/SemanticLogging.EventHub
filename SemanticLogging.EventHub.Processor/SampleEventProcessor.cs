using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace SemanticLogging.EventHub.SampleProcessor
{
    public class SampleEventProcessor : IEventProcessor
    {
        public Task OpenAsync(PartitionContext context)
        {
            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> events)
        {
            foreach (var eventData in events)
            {
                dynamic data = JsonConvert.DeserializeObject(Encoding.Default.GetString(eventData.GetBytes()));
                // process data
            }
            
            await context.CheckpointAsync();
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }
    }
}
