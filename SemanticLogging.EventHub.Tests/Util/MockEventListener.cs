using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Schema;

namespace SemanticLogging.EventHub.Tests.Util
{
    public class MockEventListener : EventListener
    {
        public ConcurrentBag<EventEntry> WrittenEntries = new ConcurrentBag<EventEntry>();

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            WrittenEntries.Add(EventEntry.Create(eventData, EventSourceSchemaCache.Instance.GetSchema(eventData.EventId, eventData.EventSource)));
        }
    }
}