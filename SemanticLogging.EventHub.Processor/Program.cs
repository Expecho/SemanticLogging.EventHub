using System;
using Microsoft.ServiceBus.Messaging;

namespace SemanticLogging.EventHub.SampleProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = CreateEventProcessorHost();
            host.RegisterEventProcessorAsync<SampleEventProcessor>().Wait();

            Console.WriteLine("Press any key to stop processing...");
            Console.ReadKey();

            host.UnregisterEventProcessorAsync().Wait();
        }

        public static EventProcessorHost CreateEventProcessorHost()
        {
            const string eventHubConnectionString = "<eventhub connection string>";
            var eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString, "<eventhub name>");

            var eventProcessorHost = new EventProcessorHost(Guid.NewGuid().ToString(),
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

            return eventProcessorHost;
        }
    }

    
}
