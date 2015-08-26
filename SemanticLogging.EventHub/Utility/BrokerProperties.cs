using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.ServiceBus.Messaging;

namespace SemanticLogging.EventHub.Utility
{
    [DataContract]
    internal class BrokerProperties
    {
        [DataMember(EmitDefaultValue = false)]
        public string CorrelationId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string SessionId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? DeliveryCount { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public Guid? LockToken { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string MessageId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Label { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ReplyTo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long? SequenceNumber { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string To { get; set; }

        public DateTime? LockedUntilUtcDateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string LockedUntilUtc
        {
            get
            {
                return LockedUntilUtcDateTime.HasValue ? LockedUntilUtcDateTime.Value.ToString("R", CultureInfo.InvariantCulture) : null;
            }
            set
            {
                DateTime lockedUntilUtcDateTime;
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind,
                    out lockedUntilUtcDateTime))
                {
                    LockedUntilUtcDateTime = lockedUntilUtcDateTime;
                }
            }
        }

        public DateTime? ScheduledEnqueueTimeUtcDateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ScheduledEnqueueTimeUtc
        {
            get
            {
                return ScheduledEnqueueTimeUtcDateTime.HasValue ? ScheduledEnqueueTimeUtcDateTime.Value.ToString("R", CultureInfo.InvariantCulture) : null;
            }
            set
            {
                DateTime scheduledEnqueueTimeUtcDateTime;
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind,
                    out scheduledEnqueueTimeUtcDateTime))
                {
                    ScheduledEnqueueTimeUtcDateTime = scheduledEnqueueTimeUtcDateTime;
                }
            }
        }

        public TimeSpan? TimeToLiveTimeSpan { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public double TimeToLive
        {
            get
            {
                return TimeToLiveTimeSpan.HasValue ? TimeToLiveTimeSpan.Value.TotalSeconds : 0;
            }
            set
            {
                TimeToLiveTimeSpan = TimeSpan.MaxValue.TotalSeconds == value ? TimeSpan.MaxValue : TimeSpan.FromSeconds(value);
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public string ReplyToSessionId { get; set; }

        public MessageState StateEnum { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string State
        {
            get { return StateEnum.ToString(); }

            internal set { StateEnum = (MessageState)Enum.Parse(typeof(MessageState), value); }
        }

        [DataMember(EmitDefaultValue = false)]
        public long? EnqueuedSequenceNumber { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string PartitionKey { get; set; }

        public DateTime? EnqueuedTimeUtcDateTime { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string EnqueuedTimeUtc
        {
            get
            {
                return EnqueuedTimeUtcDateTime.HasValue
                    ? EnqueuedTimeUtcDateTime.Value.ToString("R", CultureInfo.InvariantCulture)
                    : null;
            }
            set
            {
                DateTime enqueuedTimeUtcDateTime;
                if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind,
                    out enqueuedTimeUtcDateTime))
                {
                    EnqueuedTimeUtcDateTime = enqueuedTimeUtcDateTime;
                }
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public string ViaPartitionKey { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool? ForcePersistence { get; set; }
    }
}
