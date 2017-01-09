using System;
using System.Collections.Generic;
using System.Linq;
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
            var eventEntries = events
                    .Select(b => ToEventEntry(Encoding.Default.GetString(b.GetBytes())));

            foreach (var eventEntry in eventEntries)
            {
                // Process eventEntry
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

        private static EventEntry ToEventEntry(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<EventEntry>(json);
            }
            catch (Exception)
            {
                return new EventEntry();
            }

        }
    }
}
