using CQRSlite.Events;
using System;

namespace MassTransitCqrsLite.Domain.Events
{
    public class RecordCreated : IEvent
    {
        public int Index { get; }

        public RecordCreated(Guid id, int index)
        {
            Index = index;
            Id = id;
        }

        public Guid Id { get; set; }

        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.UtcNow;

        public int Version { get; set; }
    }
}
