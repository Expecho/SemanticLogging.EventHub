## SemanticLogging.EventHub
The SemanticLogging.EventHub project provides sinks for the Semantic Logging Application Block that exposes Event Source events to an Azure Event Hub. There is an HTTPS based sink and an AMQP based sink.

This sinks are also available as a Nuget package: https://www.nuget.org/packages/SemanticLogging.EventHub/

Run the SemanticLogging.EventHub.Processor console application for an example of how to process the events.

## AMQP based sink Usage

Minimal setup for the AMQP based sink:
```
var listener = new ObservableEventListener();

listener.LogToEventHubUsingAmqp(
    "<Connection string to eventhub>",
    "<Eventhub name>"
  );
  
listener.EnableEvents(MyEventSource.Log, EventLevel.LogAlways);
```

a partition key can optionally be provided using the partitionKey parameter. If no partition key is supplied the data is sent to the eventhub and distributed to various partitions in a round robin manner. 

## HTTPS based sink Usage

Minimal setup for the HTTPS based sink:
```
var listener = new ObservableEventListener();

listener.LogToEventHubUsingHttp(
    "<Eventhub namespace>",
    "<Eventhub name>",
    "<Publisher id>"
    "<sas token>"
  );
  
listener.EnableEvents(MyEventSource.Log, EventLevel.LogAlways);
```

a partition key can optionally be provided using the partitionKey parameter. If no partition key is supplied the data is sent to the eventhub and distributed to various partitions in a round robin manner. 

## Consuming events

One possibility is to create your own EventProcessor, a simplified implementation could like like this:

```
    public class EventProcessor : IEventProcessor
    {
        public Task OpenAsync(PartitionContext context)
        {
            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> events)
        {
                if (events == null)
                {
                    return;
                }
                var eventDataList = events as IList<EventData> ?? events.ToList();
			
				foreach(var eventData in eventDataList)
				{
				    // process events here
                	string.Join(", ", eventData.Properties.Select(p => string.Format("{0}: {1}", p.Key, p.Value))).Dump();
				}
				
                await context.CheckpointAsync();
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
                if (reason == CloseReason.Shutdown)
                {
                    await context.CheckpointAsync();
                }
        }
    }
```


## How do I contribute?

Please see [CONTRIBUTE.md](/CONTRIBUTE.md) for more details.

