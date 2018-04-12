using CQRSlite.Domain;
using CQRSlite.Events;
using MassTransit;
using MassTransit.Testing;
using MassTransitCqrsLite.Domain.Commands;
using MassTransitCqrsLite.Domain.Events;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MassTransitCqrsLite.Tests
{
    [CollectionDefinition("Test Harness")]
    public class TestCollection : ICollectionFixture<TestHarness>
    {
    }

    public class Test
    {
        protected Guid UserId { get { return Fixture.UserId; } }
        protected TestHarness Fixture { get; private set; }
        protected IBus Bus { get { return Fixture.BusControl; } }
        protected ISession Session { get { return Fixture.Session; } }
        protected IEventStore EventStore { get { return Fixture.EventStore; } }
        protected BusTestHarness Harness { get { return Fixture.Harness; } }

        public Test(TestHarness fixture, ITestOutputHelper output = null)
        {
            Fixture = fixture;

            if (output != null)
            {
                //Log.Logger = new LoggerConfiguration()
                //    .MinimumLevel.Debug()
                //    .WriteTo
                //    .TestOutput(output, LogEventLevel.Verbose)
                //    .CreateLogger()
                //    .ForContext<OsdrTest>();
            }
        }

        protected async Task<Guid> ProcessRecord(int index)
        {
            var id = NewId.NextGuid();

            await Bus.Publish<ProcessRecord>(new
            {
                Id = id,
                Index = index
            });

            if (!Harness.Consumed.Select<RecordProcessed>(m => m.Context.Message.Id == id).Any())
            {
                throw new TimeoutException();
            }

            return id;
        }
    }
}
