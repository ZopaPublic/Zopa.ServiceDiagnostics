using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;

using Zopa.ServiceDiagnostics.Health;

namespace Zopa.ServiceDiagnostics.Tests.Health
{
    public class HealthCheckIterationTests
    {
        private readonly ITestOutputHelper _output;

        public HealthCheckIterationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task HealthChecks_should_be_executed_in_parallel()
        {
            var simpleWaiters = Enumerable.Repeat("", 10).Select(x => new NoddyHealthCheck(100));
            var descriptiveWaiters = Enumerable.Repeat("", 10).Select(x => new NoddyHealthCheck(100));

            var runner = new HealthCheckRunner(simpleWaiters, descriptiveWaiters);

            var sw = Stopwatch.StartNew();

            await runner.DoAsync(CancellationToken.None);

            var elapsed = sw.ElapsedMilliseconds;
            _output.WriteLine($"Took {elapsed}ms");

            elapsed.Should().BeLessThan(2000); //much less than the 2000 it would take for serial execution
        }

        [Fact]
        public async Task The_runner_should_execute_each_simple_check_once()
        {
            var simpleWaiterMocks = Enumerable.Repeat("", 3).Select(x =>
            {
                var waiter = new Mock<IAmAHealthCheck>();
                waiter.Setup(w => w.ExecuteAsync(It.IsAny<Guid>(), CancellationToken.None)).Returns(Task.FromResult(0));
                return waiter;
            }).ToArray();

            await new HealthCheckRunner(simpleWaiterMocks.Select(x => x.Object), Enumerable.Empty<IAmADescriptiveHealthCheck>()).DoAsync(CancellationToken.None);

            foreach (var waiterMock in simpleWaiterMocks)
            {
                waiterMock.Verify(x => x.ExecuteAsync(It.IsAny<Guid>(), CancellationToken.None), Times.Once);
            }
        }

        [Fact]
        public async Task The_runner_should_execute_each_descriptive_check_once()
        {
            var descriptiveWaiterMocks = Enumerable.Repeat("", 3).Select(x =>
            {
                var waiter = new Mock<IAmADescriptiveHealthCheck>();
                waiter.Setup(w => w.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<Stopwatch>(), CancellationToken.None)).ReturnsAsync(HealthCheckResult.Pass("", TimeSpan.Zero));
                return waiter;
            }).ToArray();

            await new HealthCheckRunner(Enumerable.Empty<IAmAHealthCheck>(), descriptiveWaiterMocks.Select(x => x.Object)).DoAsync(CancellationToken.None);

            foreach (var waiterMock in descriptiveWaiterMocks)
            {
                waiterMock.Verify(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<Stopwatch>(), CancellationToken.None), Times.Once);
            }
        }

        [Fact]
        public async Task The_runner_should_report_on_each_simple_health_check()
        {
            var first = new NoddyHealthCheck(name: "first");
            var second = new NoddyHealthCheck(name: "second");

            var result = await new HealthCheckRunner(new[] { first, second }, Enumerable.Empty<IAmADescriptiveHealthCheck>()).DoAsync(CancellationToken.None);

            result.Results.Count().Should().Be(2);
            result.Results.Should().Contain(x => x.Name == "first");
            result.Results.Should().Contain(x => x.Name == "second");
        }

        [Fact]
        public async Task The_runner_should_report_on_each_descriptive_health_check()
        {
            var first = new NoddyHealthCheck(name: "first");
            var second = new NoddyHealthCheck(name: "second");

            var result = await new HealthCheckRunner(Enumerable.Empty<IAmAHealthCheck>(), new[] { first, second }).DoAsync(CancellationToken.None);

            result.Results.Count().Should().Be(2);
            result.Results.Should().Contain(x => x.Name == "first");
            result.Results.Should().Contain(x => x.Name == "second");
        }

        private class NoddyHealthCheck : IAmAHealthCheck, IAmADescriptiveHealthCheck
        {
            public NoddyHealthCheck(int delayMilis = 0, string name = nameof(NoddyHealthCheck))
            {
                _delayMilis = delayMilis;
                Name = name;
            }

            private readonly int _delayMilis;

            public string Name { get; }

            async Task IAmAHealthCheck.ExecuteAsync(Guid correlationId, CancellationToken cancellationToken)
            {
                await Task.Delay(_delayMilis, cancellationToken);
            }

            async Task<HealthCheckResult> IAmADescriptiveHealthCheck.ExecuteAsync(Guid correlationId, Stopwatch sw, CancellationToken cancellationToken)
            {
                await Task.Delay(_delayMilis, cancellationToken);
                return HealthCheckResult.Pass(Name, sw.Elapsed);
            }
        }
    }
}