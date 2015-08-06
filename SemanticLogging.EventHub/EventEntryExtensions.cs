using System;
using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Schema;
using Microsoft.ServiceBus.Messaging;

namespace Microsoft.Practices.EnterpriseLibrary.SemanticLogging
{
    public static class EventEntryExtensions
    {
        private static IReadOnlyDictionary<string, object> InitializePayload(IList<object> payload, EventSchema schema)
        {
            var payloadDictionary = new Dictionary<string, object>(payload.Count);

            for (int i = 0; i < payload.Count; i++)
            {
                payloadDictionary.Add(string.Format("Payload_{0}", schema.Payload[i]), payload[i]);
            }

            return payloadDictionary;
        }

        public static EventData ToEventData(this EventEntry entry)
        {
            var eventData = new EventData();

            eventData.Properties.Add("EventId", entry.EventId);
            eventData.Properties.Add("EventDate", entry.Timestamp.UtcDateTime);
            eventData.Properties.Add("Keywords", (long)entry.Schema.Keywords);
            eventData.Properties.Add("ProviderId", entry.ProviderId);
            eventData.Properties.Add("ProviderName", entry.Schema.ProviderName);
            eventData.Properties.Add("Level", (long)entry.Schema.Level);

            if (entry.FormattedMessage != null)
            {
                eventData.Properties.Add("Message", entry.FormattedMessage);
            }

            eventData.Properties.Add("Opcode", (long)entry.Schema.Opcode);
            eventData.Properties.Add("Task", (long)entry.Schema.Task);
            eventData.Properties.Add("Version", entry.Schema.Version);

            eventData.Properties.Add("ProcessId", entry.ProcessId);
            eventData.Properties.Add("ThreadId", entry.ThreadId);

            if (entry.ActivityId != Guid.Empty)
            {
                eventData.Properties.Add("ActivityId", entry.ActivityId);
            }

            if (entry.RelatedActivityId != Guid.Empty)
            {
                eventData.Properties.Add("RelatedActivityId", entry.RelatedActivityId);
            }

            var payload = InitializePayload(entry.Payload, entry.Schema);

            foreach (var value in payload)
            {
                eventData.Properties.Add(value.Key, value.Value);
            }

            return eventData;
        }
    }
}
