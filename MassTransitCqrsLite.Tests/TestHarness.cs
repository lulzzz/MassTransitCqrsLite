using CQRSlite.Domain;
using CQRSlite.Events;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.Saga;
using MassTransit.Scoping;
using MassTransit.Testing;
using MassTransitCqrsLite.Domain.Consumers;
using MassTransitCqrsLite.Domain.Events;
using MassTransitCqrsLite.Domain.Sagas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MassTransitCqrsLite.Tests
{
    public class TestHarness : IDisposable
    {
        private BusTestHarness _harness;
        protected IServiceProvider _serviceProvider;
        public Guid UserId { get; private set; }

        public ISession Session { get { return _serviceProvider.GetService<ISession>(); } }
        public IEventStore EventStore { get { return _serviceProvider.GetService<IEventStore>(); } }

        public IBusControl BusControl { get { return _serviceProvider.GetService<IBusControl>(); } }
        public BusTestHarness Harness { get { return _harness; } }

        public TestHarness()
        {
            var builder = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.bddtests.json", true, true);

            var configuration = builder.Build();

            var services = new ServiceCollection();

            UserId = NewId.NextGuid();

            _harness = new InMemoryTestHarness();

            services.AddOptions();

            services.AddSingleton<IEventStore, InMemoryEventStore>();
            services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
            services.AddScoped<ISession, Session>();
            services.AddScoped<IRepository, Repository>();

            services.AddScoped<CreateRecordConsumer>();
            services.AddScoped<RecordProcessedConsumer>();

            services.AddSingleton<IConsumerScopeProvider, DependencyInjectionConsumerScopeProvider>();

            services.AddSingleton<TestStateMachine>();

            services.AddSingleton<ISagaRepository<TestState>>(new InMemorySagaRepository<TestState>());

            services.AddSingleton((ctx) =>
            {
                return _harness.Bus as IBusControl;
            });

            _harness.OnConfigureBus += cfg =>
            {
                cfg.UseInMemoryOutbox();

                cfg.ReceiveEndpoint($"receiver_saga_queue", e =>
                {
                    e.StateMachineSaga(_serviceProvider.GetRequiredService<TestStateMachine>(), _serviceProvider.GetRequiredService<ISagaRepository<TestState>>());
                });

                cfg.ReceiveEndpoint($"receiver_queue", e =>
                {
                    e.ScopedConsumer<CreateRecordConsumer>(_serviceProvider, c => c.UseCqrsLite());
                    e.Consumer(_serviceProvider.GetRequiredService<RecordProcessedConsumer>);
                });
            };

            _harness.Handler<RecordProcessed>(async context =>
            {
                await Task.CompletedTask;
            });

            _serviceProvider = services.BuildServiceProvider();

            _harness.Start().Wait();
        }

        public virtual void Dispose()
        {
            _harness.Stop().Wait();
        }
    }
}
