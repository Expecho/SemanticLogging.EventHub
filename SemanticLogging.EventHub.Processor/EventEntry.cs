using System;
using System.Collections.ObjectModel;

namespace SemanticLogging.EventHub.SampleProcessor
{
    internal class EventEntry
    {
        public int EventId { get; set; }
        public int ProcessId { get; set; }
        public int ThreadId { get; set; }
        public string FormattedMessage { get; set; }
        public Schema Schema { get; set; }
        public Guid ProviderId { get; set; }
        public Guid ActivityId { get; set; }
        public Guid RelatedActivityId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public ReadOnlyCollection<object> Payload { get; set; }

        public bool IsValid()
        {
            return Schema != null && ProviderId != Guid.Empty;
        }
    }
}