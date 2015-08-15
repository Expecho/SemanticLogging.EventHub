using System;
using System.Net.Http;
using System.Text;
using EnterpriseLibrary.SemanticLogging.EventHub.Utility;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Newtonsoft.Json;

namespace EnterpriseLibrary.SemanticLogging.EventHub
{
    public class EventHubHttpSink : IObserver<EventEntry>
    {
        private readonly string eventHubNamespace;
        private readonly string eventHubName;
        private readonly string partitionKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubHttpSink" /> class.
        /// </summary>
        /// <param name="eventHubNamespace">The namespace of the eventhub.</param>
        /// <param name="eventHubName">The name of the eventhub.</param>
        public EventHubHttpSink(string eventHubNamespace, string eventHubName, string partitionKey)
        {
            Guard.ArgumentNotNullOrEmpty(eventHubNamespace, "eventHubConnectionString");
            Guard.ArgumentNotNullOrEmpty(eventHubName, "eventHubName");

            this.eventHubNamespace = eventHubNamespace;
            this.eventHubName = eventHubName;
            this.partitionKey = partitionKey;
        }

        public void OnNext(EventEntry value)
        {
            using (var httpClient = new HttpClient())
            {
                var payload = JsonConvert.SerializeObject(value);

                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var url = string.Format("https://{0}.servicebus.windows.net/{1}/Messages", eventHubNamespace, eventHubName);
                
                httpClient.PostAsync(url, content);
            }

        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}