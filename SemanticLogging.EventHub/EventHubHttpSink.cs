using System;
using System.Net.Http;
using System.Text;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Newtonsoft.Json;
using SemanticLogging.EventHub.Utility;

namespace SemanticLogging.EventHub
{
    public class EventHubHttpSink : IObserver<EventEntry>
    {
        private readonly string eventHubNamespace;
        private readonly string eventHubName;
        private readonly string publisherId;
        private readonly string sasToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubHttpSink" /> class.
        /// </summary>
        /// <param name="eventHubNamespace">The namespace of the eventhub.</param>
        /// <param name="eventHubName">The name of the eventhub.</param>
        /// <param name="publisherId">The id fo the event pbulisher.</param>
        /// <param name="sasToken">The shared access signature token.</param>
        public EventHubHttpSink(string eventHubNamespace, string eventHubName, string publisherId, string sasToken)
        {
            Guard.ArgumentNotNullOrEmpty(eventHubNamespace, "eventHubConnectionString");
            Guard.ArgumentNotNullOrEmpty(eventHubName, "eventHubName");
            Guard.ArgumentNotNullOrEmpty(publisherId, "partitionKey");
            Guard.ArgumentNotNullOrEmpty(sasToken, "sasToken");

            this.eventHubNamespace = eventHubNamespace;
            this.eventHubName = eventHubName;
            this.publisherId = publisherId;
            this.sasToken = sasToken;
        }

        public async void OnNext(EventEntry value)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", sasToken);

                var payload = JsonConvert.SerializeObject(value);

                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                content.Headers.Add("ContentType", "application/atom+xml;type=entry;charset=utf-8");
                var url = string.Format("https://{0}.servicebus.windows.net/{1}/publishers/{2}/Messages", eventHubNamespace, eventHubName, publisherId);

                try
                {
                    await httpClient.PostAsync(url, content);
                }
                catch (Exception ex)
                {
                    SemanticLoggingEventSource.Log.CustomSinkUnhandledFault(ex.ToString());
                    throw;
                }
            }
        }

        public void OnError(Exception error)
        {

        }

        public void OnCompleted()
        {

        }
    }
}