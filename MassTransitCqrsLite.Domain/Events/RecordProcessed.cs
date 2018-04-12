using MassTransit;
using System;

namespace MassTransitCqrsLite.Domain.Events
{
    public interface RecordProcessed : CorrelatedBy<Guid>
    {
        Guid Id { get; }
        int Index { get; }
    }
}
