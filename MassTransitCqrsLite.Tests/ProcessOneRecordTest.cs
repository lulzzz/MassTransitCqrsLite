using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MassTransitCqrsLite.Tests
{
    [Collection("Test Harness")]
    public class ProcessOneRecordTest : Test
    {
        private Guid RecordId { get; set; }

        public ProcessOneRecordTest(TestHarness fixture, ITestOutputHelper output) : base(fixture, output)
        {
            RecordId = ProcessRecord(0).Result;
        }

        [Fact]
        public async Task RecordSuccessfullyProcessed()
        {
            var record = await Session.Get<Domain.Aggregate.Record>(RecordId);

            Assert.Equal(RecordId, record.Id);
            Assert.Equal(0, record.Index);
        }
    }
}
