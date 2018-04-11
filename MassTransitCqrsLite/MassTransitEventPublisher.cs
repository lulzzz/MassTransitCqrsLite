using CQRSlite.Events;
using MassTransit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MassTransitCqrsLite
{
    public class MassTransitEventPublisher : IEventPublisher
    {
        private ConsumeContext _context;

        public void SetContext(ConsumeContext context)
        {
            _context = context;
        }

        public async Task Publish<T>(T @event, CancellationToken cancellationToken) where T : class, IEvent
        {
            if (_context == null)
                throw new NullReferenceException(nameof(_context));

            if (_context.CorrelationId != null)
            {
                await _context.Publish(@event, @event.GetType(), c => c.Headers.Set("CorrelationId", _context.CorrelationId.ToString()), cancellationToken);
            }
            else
            {
                await _context.Publish(@event, @event.GetType(), cancellationToken);
            }
        }
    }
}
