using MassTransit;
using MassTransitCqrsLite.Domain.Events;
using Serilog;
using System.Threading.Tasks;

namespace MassTransitCqrsLite.Domain.Consumers
{
    public class RecordProcessedConsumer : IConsumer<RecordProcessed>
    {
        public async Task Consume(ConsumeContext<RecordProcessed> context)
        {
            Log.Information($"Record with Id {context.Message.Id} and Index {context.Message.Index} successfully processed");

            await Task.CompletedTask;
        }
    }
}
