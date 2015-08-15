using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
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
            if (events == null)
            {
                return;
            }

            foreach (var eventData in events)
            {
                dynamic data = JsonConvert.DeserializeObject(Encoding.Default.GetString(eventData.GetBytes()));
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

    public class EventDataConverter
    {
        public static EventEntry ToEventEntry(dynamic eventData)
        {
            return new EventEntry
            (
                (Guid)eventData.sourceId,
                eventData.eventId,
                eventData.formattedMessage,
                null,
                eventData.timestamp,
                eventData.activityId,
                eventData.relatedactivityId,
                null

            );
        }
    }
}
