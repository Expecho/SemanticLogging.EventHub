using System.Collections.Specialized;

namespace SemanticLogging.EventHub.Utility
{
    internal class ServiceBusHttpMessage
    {
        public byte[] Body { get; set; }
        public string Location { get; set; }
        public BrokerProperties BrokerProperties { get; set; }
        public NameValueCollection CustomProperties { get; set; }

        public ServiceBusHttpMessage()
        {
            BrokerProperties = new BrokerProperties();
            CustomProperties = new NameValueCollection();
        }
    }
}