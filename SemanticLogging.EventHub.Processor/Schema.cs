using System;
using System.Diagnostics.Tracing;

namespace SemanticLogging.EventHub.SampleProcessor
{
    internal class Schema
    {
        public string EventName { get; set; }
        public int Version { get; set; }
        public EventLevel Level { get; set; }
        public EventKeywords KeyWords { get; set; }
        public string KeyWordsDescription { get; set; }
        public EventTask Task { get; set; }
        public string TaskName { get; set; }
        public EventOpcode OpCode { get; set; }
        public string OpCodeName { get; set; }
        public string[] Payload { get; set; }
        public int Id { get; set; }
        public Guid ProviderId { get; set; }
        public string ProviderName { get; set; }
    }
}