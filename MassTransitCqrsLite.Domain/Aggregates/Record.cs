using CQRSlite.Domain;
using MassTransitCqrsLite.Domain.Events;
using System;

namespace MassTransitCqrsLite.Domain.Aggregate
{
    public class Record : AggregateRoot
    {
        public int Index { get; private set; }

        public DateTimeOffset CreatedDateTime { get; private set; }

        private void Apply(RecordCreated e)
        {
            Index = e.Index;
            CreatedDateTime = e.TimeStamp;
        }

        protected Record()
        {
        }

        public Record(Guid id, int index)
        {
            Id = id;
            ApplyChange(new RecordCreated(Id, index));
        }
    }
}
