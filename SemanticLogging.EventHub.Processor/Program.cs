using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace SemanticLogging.EventHub.SampleProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            ProcessAsync().Wait();
        }

        public static async Task ProcessAsync()
        {
            var eventHubConnectionString = "eventhub connection string>";
            var eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString, "<eventhub name>");

            var eventProcessorHost = new EventProcessorHost(Environment.MachineName,
                                                            eventHubClient.Path.ToLower(),
                                                            EventHubConsumerGroup.DefaultGroupName,
                                                            eventHubConnectionString,
                                                            "<Storage account connection string>")
            {
                PartitionManagerOptions = new PartitionManagerOptions
                {
                    AcquireInterval = TimeSpan.FromSeconds(10), // Default is 10 seconds 
                    RenewInterval = TimeSpan.FromSeconds(10),   // Default is 10 seconds 
                    LeaseInterval = TimeSpan.FromSeconds(30)    // Default value is 30 seconds 
                }
            };

            await eventProcessorHost.RegisterEventProcessorAsync<SampleEventProcessor>();
        }
    }

    
}
