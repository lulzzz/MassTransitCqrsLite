using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MassTransitCqrsLite.Tests
{
    [Collection("Test Harness")]
    public class ProcessTwoRecordsTest : Test
    {
        private Guid FirstRecordId { get; set; }
        private Guid SecondRecordId { get; set; }

        public ProcessTwoRecordsTest(TestHarness fixture, ITestOutputHelper output) : base(fixture, output)
        {
            FirstRecordId = ProcessRecord(0).Result;
            SecondRecordId = ProcessRecord(1).Result;
        }

        [Fact]
        public async Task FirstRecordSuccessfullyProcessed()
        {
            var record = await Session.Get<Domain.Aggregate.Record>(FirstRecordId);

            Assert.Equal(FirstRecordId, record.Id);
            Assert.Equal(0, record.Index);
        }

        [Fact]
        public async Task SecondRecordSuccessfullyProcessed()
        {
            var record = await Session.Get<Domain.Aggregate.Record>(SecondRecordId);

            Assert.Equal(SecondRecordId, record.Id);
            Assert.Equal(1, record.Index);
        }
    }
}
