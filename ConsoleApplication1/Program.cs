using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.ServiceBus;
using SemanticLogging.EventHub;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {


            var listener = new ObservableEventListener();
            var errors = new ObservableEventListener();

            errors.LogToConsole();

            //listener.LogToEventHubUsingAmqp
            //    (
            //        //"Endpoint=sb://plancare2.servicebus.windows.net/;SharedAccessKeyName=eventsource-sender;SharedAccessKey=3q+yg0o5YV9YpJAWdCU+Hf5ruBddCmos27TIzFOF/fU=",
            //        "Endpoint=sb://expecho-eventhub-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=m2bvokk8pHBDdSHRuTmbVhoLRpzIfqOkAaZwnu8TyZQ=",
            //        "expecho-eventhub",
            //        bufferingCount: 0
            //    );


            var serviceUri = ServiceBusEnvironment.CreateServiceUri("https", "expecho-eventhub-ns", "expecho-eventhub").ToString().Trim('/');
            string generatedSaS = SharedAccessSignatureTokenProvider.GetSharedAccessSignature("my-sender",
                "pVNik0g41sEqIAW3N8QIgjBy7IlCIUrdEaQDTFN+WwU=", serviceUri, TimeSpan.FromDays(1));

            listener.LogToEventHubUsingHttp
                (
                    //"Endpoint=sb://plancare2.servicebus.windows.net/;SharedAccessKeyName=eventsource-sender;SharedAccessKey=3q+yg0o5YV9YpJAWdCU+Hf5ruBddCmos27TIzFOF/fU=",
                    "expecho-eventhub-ns",
                    "expecho-eventhub",
                    "d2",
                    generatedSaS,
                    bufferingCount: 5
                );

            errors.EnableEvents(SemanticLoggingEventSource.Log, EventLevel.LogAlways, Keywords.All);
            listener.EnableEvents(MyEventSource.Log, EventLevel.LogAlways, Keywords.All);

            var tries = 200;
            for (int i = 0; i < tries; i++)
            {
                MyEventSource.Log.WriteMyTraceEvent(new string('*', 500), i);
            }

            listener.Dispose();
            errors.Dispose();
        }
    }

    [EventSource(Name = "DeHeerSoftware-PlanCare2")]
    public sealed class MyEventSource : EventSource
    {
        public class Keywords
        {
            public const EventKeywords Database = (EventKeywords)1;
            public const EventKeywords ExternalApi = (EventKeywords)2;
        }

        public class Tasks
        {
            public const EventTask Timing = (EventTask)1;
        }

        public static MyEventSource Log = new MyEventSource();

        [Event(1, Message = "{0}", Keywords = Keywords.Database | Keywords.ExternalApi, Task = Tasks.Timing)]
        public void WriteMyTraceEvent(string eventName, long index)
        {
            WriteEvent(1, eventName, index);
        }
    }
}
    