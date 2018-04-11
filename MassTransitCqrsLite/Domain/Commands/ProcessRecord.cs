using MassTransit;
using System;

namespace MassTransitCqrsLite.Domain.Commands
{
    public interface ProcessRecord : CorrelatedBy<Guid>
    {
        Guid Id { get; }
        int Index { get; }
    }
}
