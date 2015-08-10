using System;
using System.Xml.Linq;
using EnterpriseLibrary.SemanticLogging.EventHub.Utility;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;


namespace EnterpriseLibrary.SemanticLogging.EventHub.Configuration
{
    internal class EventHubSinkElement : ISinkElement
    {
        private readonly XName sinkName = XName.Get("eventHubSink", Constants.Namespace);

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
            subject.LogToEventHub(
                (string)element.Attribute("eventHubConnectionString"),
                (string)element.Attribute("eventHubPath"),
                element.Attribute("bufferingIntervalInSeconds").ToTimeSpan(),
                (int?)element.Attribute("bufferingCount") ?? Buffering.DefaultBufferingCount,
                element.Attribute("bufferingFlushAllTimeoutInSeconds").ToTimeSpan() ?? Constants.DefaultBufferingFlushAllTimeout,
                (int?)element.Attribute("maxBufferSize") ?? Buffering.DefaultMaxBufferSize,
                (string)element.Attribute("partitionKey")
                );

            return subject;
        }
    }
}
