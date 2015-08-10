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
            const string eventHubConnectionString = "<eventhub connection string>";
            var eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString, "<eventhub name>");

            var eventProcessorHost = new EventProcessorHost(Environment.MachineName,
                                                            eventHubClient.Path.ToLower(),
                                                            EventHubConsumerGroup.DefaultGroupName,
                                                            eventHubConnectionString,
                                                            "<Storage account connection string>")
            {
                PartitionManagerOptions = new PartitionManagerOptions
                {
                    AcquireInterval = TimeSpan.FromSeconds(10), 
                    RenewInterval = TimeSpan.FromSeconds(10),   
                    LeaseInterval = TimeSpan.FromSeconds(30)
                }
            };

            await eventProcessorHost.RegisterEventProcessorAsync<SampleEventProcessor>();
        }
    }

    
}
