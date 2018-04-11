using Automatonymous;
using MassTransit;
using MassTransit.MongoDbIntegration.Saga;
using MassTransitCqrsLite.Domain.Commands;
using MassTransitCqrsLite.Domain.Events;
using System;

namespace MassTransitCqrsLite.Domain.Sagas
{
    public class TestState : SagaStateMachineInstance, IVersionedSaga
    {
        public Guid CorrelationId { get; set; }
        public Guid Id { get; set; }
        public string CurrentState { get; set; }
        public int Version { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Guid RecordId { get; set; }
        public int Index { get; set; }
    }

    public class TestStateMachine : MassTransitStateMachine<TestState>
    {
        public TestStateMachine()
        {
            InstanceState(x => x.CurrentState);

            Event(() => ProcessRecord, x => x.CorrelateById(context => context.Message.Id).SelectId(context => NewId.NextGuid()));
            Event(() => RecordCreated, x => x.CorrelateById(context => Guid.Parse(context.Headers.Get<string>("CorrelationId"))));

            Initially(
                When(ProcessRecord)
                    .TransitionTo(Processing)
                    .ThenAsync(async context =>
                    {
                        context.Instance.RecordId = context.Data.Id;
                        context.Instance.Index = context.Data.Index;
                        context.Instance.UpdatedAt = DateTime.Now;

                        await context.CreateConsumeContext().Publish<CreateRecord>(new
                        {
                            Id = context.Instance.RecordId,
                            Index = context.Instance.Index,
                            CorrelationId = context.Instance.CorrelationId
                        });
                    })
            );

            During(Processing,
                When(RecordCreated)
                    .ThenAsync(async context =>
                    {
                        context.Instance.UpdatedAt = DateTime.Now;

                        await context.CreateConsumeContext().Publish<RecordProcessed>(new
                        {
                            Id = context.Instance.RecordId,
                            Index = context.Instance.Index,
                            CorrelationId = context.Instance.CorrelationId
                        });
                    })
                    .Finalize());

            SetCompletedWhenFinalized();
        }

        State Processing { get; set; }

        Event<ProcessRecord> ProcessRecord { get; set; }
        Event<RecordCreated> RecordCreated { get; set; }
    }
}
