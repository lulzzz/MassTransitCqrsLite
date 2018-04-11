using CQRSlite.Domain;
using CQRSlite.Events;
using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.RabbitMqTransport;
using MassTransit.Saga;
using MassTransit.Scoping;
using MassTransitCqrsLite.Domain.Commands;
using MassTransitCqrsLite.Domain.Consumers;
using MassTransitCqrsLite.Domain.Sagas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

namespace MassTransitCqrsLite
{
    class Program
    {
        static void Main(string[] args)
        {
            IServiceProvider container = null;

            var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", true, true)
                    .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            Log.Information("Starting Receiver...");

            var services = new ServiceCollection();

            services.AddSingleton<IEventStore, InMemoryEventStore>();
            services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
            services.AddScoped<ISession, Session>();
            services.AddScoped<IRepository, Repository>();

            services.AddScoped<CreateRecordConsumer>();
            services.AddScoped<RecordProcessedConsumer>();

            services.AddSingleton<IConsumerScopeProvider, DependencyInjectionConsumerScopeProvider>();

            services.AddSingleton<TestStateMachine>();

            services.AddSingleton<ISagaRepository<TestState>>(new InMemorySagaRepository<TestState>());

            services.AddTransient(context => Bus.Factory.CreateUsingRabbitMq(x =>
            {
                IRabbitMqHost host = x.Host(new Uri("rabbitmq://guest:guest@localhost:5672/test"), h => { });

                x.ReceiveEndpoint(host, $"receiver_saga_queue", e =>
                {
                    e.StateMachineSaga(container.GetRequiredService<TestStateMachine>(), container.GetRequiredService<ISagaRepository<TestState>>());
                });

                x.ReceiveEndpoint(host, $"receiver_queue", e =>
                {
                    e.ScopedConsumer<CreateRecordConsumer>(container, cfg => cfg.UseCqrsLite());
                    e.Consumer(container.GetRequiredService<RecordProcessedConsumer>);
                });

                x.UseSerilog();
            }));

            container = services.BuildServiceProvider();

            var busControl = container.GetRequiredService<IBusControl>();

            busControl.Start();

            Log.Information("Receiver started...");

            for (var i = 0; i < 10; i++)
            {
                busControl.Publish<ProcessRecord>(new
                {
                    Id = NewId.NextGuid(),
                    Index = i
                });
            }
        }
    }

    public static class ConsumerExtensions
    {
        public static void ScopedConsumer<TConsumer>(this IReceiveEndpointConfigurator configurator, IServiceProvider container, Action<IConsumerConfigurator<TConsumer>> configure = null) where TConsumer : class, IConsumer
        {
            configurator.Consumer(new ScopeConsumerFactory<TConsumer>(container.GetRequiredService<IConsumerScopeProvider>()), configure);
        }
    }
}