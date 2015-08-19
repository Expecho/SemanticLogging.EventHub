using System;
using System.Xml.Linq;
using EnterpriseLibrary.SemanticLogging.EventHub.Utility;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Configuration;

namespace EnterpriseLibrary.SemanticLogging.EventHub.Configuration
{
    internal class EventHubHttpSinkElement : ISinkElement
    {
        private readonly XName sinkName = XName.Get("eventHubHttpSink", "urn:dhs.sinks.eventHubHttpSink");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated with Guard class")]
        public bool CanCreateSink(XElement element)
        {
            Guard.ArgumentNotNull(element, "element");

            return element.Name == sinkName;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated with Guard class")]
        public IObserver<EventEntry> CreateSink(XElement element)
        {
            Guard.ArgumentNotNull(element, "element");

            var subject = new EventEntrySubject();
            subject.LogToEventHubUsingHttp(
                (string)element.Attribute("eventHubNamespace"),
                (string)element.Attribute("eventHubName"),
                (string)element.Attribute("partitionKey"),
                (string)element.Attribute("sasToken")
                );

            return subject;
        }
    }
}