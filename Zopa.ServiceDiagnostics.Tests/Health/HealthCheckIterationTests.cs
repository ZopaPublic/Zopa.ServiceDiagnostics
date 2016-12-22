using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Zopa.ServiceDiagnostics.Health;

namespace Zopa.ServiceDiagnostics.Tests.Health
{
    public class HealthCheckIterationTests
    {
        [Test]
        public async Task HealthChecks_should_be_executed_in_parallel()
        {
            var simpleWaiters = Enumerable.Repeat("", 10).Select(x => new NoddyHealthCheck(100));
            var descriptiveWaiters = Enumerable.Repeat("", 10).Select(x => new NoddyHealthCheck(100));

            var runner = new HealthCheckRunner(simpleWaiters, descriptiveWaiters);

            var sw = Stopwatch.StartNew();

            await runner.DoAsync();

            var elapsed = sw.ElapsedMilliseconds;
            await TestContext.Out.WriteLineAsync($"Took {elapsed}ms");

            elapsed.Should().BeLessThan(2000); //much less than the 2000 it would take for serial execution
        }

        [Test]
        public async Task The_runner_should_execute_each_simple_check_once()
        {
            var simpleWaiterMocks = Enumerable.Repeat("", 3).Select(x =>
            {
                var waiter = new Mock<IAmAHealthCheck>();
                waiter.Setup(w => w.ExecuteAsync(It.IsAny<Guid>())).Returns(Task.FromResult(0));
                return waiter;
            }).ToArray();

            await new HealthCheckRunner(simpleWaiterMocks.Select(x => x.Object), Enumerable.Empty<IAmADescriptiveHealthCheck>()).DoAsync();

            foreach (var waiterMock in simpleWaiterMocks)
            {
                waiterMock.Verify(x => x.ExecuteAsync(It.IsAny<Guid>()), Times.Once);
            }
        }

        [Test]
        public async Task The_runner_should_execute_each_descriptive_check_once()
        {
            var descriptiveWaiterMocks = Enumerable.Repeat("", 3).Select(x =>
            {
                var waiter = new Mock<IAmADescriptiveHealthCheck>();
                waiter.Setup(w => w.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<Stopwatch>())).ReturnsAsync(HealthCheckResult.Pass("", TimeSpan.Zero));
                return waiter;
            }).ToArray();

            await new HealthCheckRunner(Enumerable.Empty<IAmAHealthCheck>(), descriptiveWaiterMocks.Select(x => x.Object)).DoAsync();

            foreach (var waiterMock in descriptiveWaiterMocks)
            {
                waiterMock.Verify(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<Stopwatch>()), Times.Once);
            }
        }

        [Test]
        public async Task The_runner_should_report_on_each_simple_health_check()
        {
            var first = new NoddyHealthCheck(name: "first");
            var second = new NoddyHealthCheck(name: "second");

            var result = await new HealthCheckRunner(new[] { first, second }, Enumerable.Empty<IAmADescriptiveHealthCheck>()).DoAsync();

            result.Results.Count().Should().Be(2);
            result.Results.Should().Contain(x => x.Name == "first");
            result.Results.Should().Contain(x => x.Name == "second");
        }

        [Test]
        public async Task The_runner_should_report_on_each_descriptive_health_check()
        {
            var first = new NoddyHealthCheck(name: "first");
            var second = new NoddyHealthCheck(name: "second");

            var result = await new HealthCheckRunner(Enumerable.Empty<IAmAHealthCheck>(), new[] { first, second }).DoAsync();

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

            async Task IAmAHealthCheck.ExecuteAsync(Guid correlationId)
            {
                await Task.Delay(_delayMilis);
            }

            async Task<HealthCheckResult> IAmADescriptiveHealthCheck.ExecuteAsync(Guid correlationId, Stopwatch sw)
            {
                await Task.Delay(_delayMilis);
                return HealthCheckResult.Pass(Name, sw.Elapsed);
            }
        }
    }
}