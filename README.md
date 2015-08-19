## SemanticLogging.EventHub
The SemanticLogging.EventHub project provides sinks for the Semantic Logging Application Block that exposes Event Source events to an Azure Event Hub. There is an HTTPS based sink and an AMQP based sink.

This sinks are also available as a Nuget package: https://www.nuget.org/packages/SemanticLogging.EventHub/

Run the SemanticLogging.EventHub.Processor console application for an example of how to process the events.

## AMQP based sink Usage

Minimal setup for the AMQP based sink:
```c#
var listener = new ObservableEventListener();

listener.LogToEventHubUsingAmqp(
    "Endpoint=sb://my-eventhub-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[your key]",
    "my-eventhub"
  );
  
listener.EnableEvents(MyEventSource.Log, EventLevel.LogAlways);
```

a partition key can optionally be provided using the partitionKey parameter. If no partition key is supplied the data is sent to the eventhub and distributed to various partitions in a round robin manner. 

## HTTPS based sink Usage

Minimal setup for the HTTPS based sink:
```c#
var listener = new ObservableEventListener();

listener.LogToEventHubUsingHttp(
    "my-eventhub-ns",
    "my-eventhub",
    "dev01"
    "[your token]"
  );
  
listener.EnableEvents(MyEventSource.Log, EventLevel.LogAlways);
```

You can use this tool to generate a sas token: https://github.com/sandrinodimattia/RedDog/releases/tag/0.2.0.1

## Out-of-process logging

A sample configuration for out-of-process logging using a windows service (see https://msdn.microsoft.com/en-us/library/dn774996(v=pandp.20).aspx):

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration xmlns="http://schemas.microsoft.com/practices/2013/entlib/semanticlogging/etw"
               xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
			   xmlns:etw="http://schemas.microsoft.com/practices/2013/entlib/semanticlogging/etw"
               xsi:schemaLocation="http://schemas.microsoft.com/practices/2013/entlib/semanticlogging/etw SemanticLogging-svc.xsd">
  
  <sinks>
	<eventHubAmqpSink xmlns="urn:dhs.sinks.eventHubAmqpSink" name="eventHubAmqpSink" type ="SemanticLogging.EventHub.EventHubAmqpSink, EnterpriseLibrary.SemanticLogging.EventHub"
		eventHubConnectionString="Endpoint=sb://my-eventhub-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[your key]"
		eventHubName="my-eventhub"
		bufferingIntervalInSeconds="30"
		bufferingCount="100"
		bufferingFlushAllTimeoutInSeconds="5"
		maxBufferSize="3000">
		<etw:sources>
			<etw:eventSource name="DeHeerSoftware-PlanCare2" level="Warning" />
		</etw:sources>
	</eventHubAmqpSink>
	<eventHubHttpSink xmlns="urn:dhs.sinks.eventHubHttpSink" name="eventHubHttpSink" type ="SemanticLogging.EventHub.EventHubHttpSink, EnterpriseLibrary.SemanticLogging.EventHub"
		eventHubNamespace="my-eventhub-ns"
		eventHubName="my-eventhub"
		partitionKey="dev01"
		sasToken="[your token]">
		<etw:sources>
			<etw:eventSource name="DeHeerSoftware-PlanCare2" level="Warning" />
		</etw:sources>
	</eventHubHttpSink>
  </sinks>
</configuration>
```

You can use this tool to generate a sas token: https://github.com/sandrinodimattia/RedDog/releases/tag/0.2.0.1

## Consuming events

One possibility is to create your own EventProcessor, a simplified implementation could like like this:

```c#
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
			
		foreach (var eventData in events)
            	{
                	dynamic data = JsonConvert.DeserializeObject(Encoding.Default.GetString(eventData.GetBytes()));
                	// do something with data
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

