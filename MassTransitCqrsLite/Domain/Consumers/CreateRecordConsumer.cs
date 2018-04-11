using CQRSlite.Domain;
using MassTransit;
using MassTransitCqrsLite.Domain.Aggregate;
using MassTransitCqrsLite.Domain.Commands;
using Serilog;
using System;
using System.Threading.Tasks;

namespace MassTransitCqrsLite.Domain.Consumers
{
    public class CreateRecordConsumer : IConsumer<CreateRecord>
    {
        private ISession _session;

        public CreateRecordConsumer(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task Consume(ConsumeContext<CreateRecord> context)
        {
            Log.Information($"Create record: Id {context.Message.Id}, Index {context.Message.Index}");

            var record = new Record(context.Message.Id, context.Message.Index);

            await _session.Add(record);

            await _session.Commit();
        }
    }
}
