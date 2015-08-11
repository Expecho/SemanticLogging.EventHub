## SemanticLogging.EventHub
SemanticLogging.EventHub is a sink for the Semantic Logging Application Block that exposes Event Source events to an Azure Event Hub.

This sink is also available as a Nuget package: https://www.nuget.org/packages/SemanticLogging.EventHub/

Run the SemanticLogging.EventHub.Processor console application for an example of how to process the events.

## Usage

Minimal setup:
```
var listener = new ObservableEventListener();

listener.LogToEventHub(
    "<Connection string to eventhub>",
    "<Eventhub name>"
  );
  
listener.EnableEvents(MyEventSource.Log, EventLevel.LogAlways);
```

## How do I contribute?

Please see [CONTRIBUTE.md](/CONTRIBUTE.md) for more details.

