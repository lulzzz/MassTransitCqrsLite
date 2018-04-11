using CQRSlite.Domain;
using CQRSlite.Events;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.MongoDbIntegration.Saga;
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

            services.AddMassTransit(x =>
            {
                x.AddConsumer<CreateRecordConsumer>();
                x.AddConsumer<RecordProcessedConsumer>();
                //x.AddSaga<TestState>();
            });

            services.AddSingleton<TestStateMachine>();

            var repository = new MongoDbSagaRepository<TestState>("mongodb://localhost:27017/test", "sagas");
            services.AddSingleton<ISagaRepository<TestState>>(repository);

            services.AddTransient(context => Bus.Factory.CreateUsingRabbitMq(x =>
            {
                IRabbitMqHost host = x.Host(new Uri("rabbitmq://guest:guest@localhost:5672/test"), h => { });

                x.ReceiveEndpoint(host, $"receiver_saga_queue", e =>
                {
                    e.UseInMemoryOutbox();
                    e.StateMachineSaga(container.GetService<TestStateMachine>(), repository);
                });

                x.ReceiveEndpoint(host, $"receiver_queue", e =>
                {
                    var scopeProvider = new DependencyInjectionConsumerScopeProvider(container);

                    e.Consumer(new ScopeConsumerFactory<CreateRecordConsumer>(scopeProvider), cfg => cfg.UseCqrsLite());
                    e.Consumer(container.GetRequiredService<RecordProcessedConsumer>);
                });

                x.UseSerilog();
            }));

            container = services.BuildServiceProvider();

            var busControl = container.GetRequiredService<IBusControl>();

            busControl.Start();

            Log.Information("Receiver started...");

            for (var i = 0; i < 2; i++)
            {
                busControl.Publish<ProcessRecord>(new
                {
                    Id = NewId.NextGuid(),
                    Index = i
                });
            }
        }
    }
}