using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Schema;
using Newtonsoft.Json;

namespace SemanticLogging.EventHub.Utility
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

        internal static BatchMessage ToBatchMessage(this EventEntry entry)
        {
            return new BatchMessage { Body = JsonConvert.SerializeObject(entry) };
        }
    }
}
