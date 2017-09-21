using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using Zopa.ServiceDiagnostics.Health;

namespace Zopa.ServiceDiagnostics.Tests.Health
{
    public class WhenTheSystemIsHealthy : IClassFixture<WhenTheSystemIsHealthy.Scenario>
    {
        public class Scenario : BaseScenario
        {
            public const string SimpleCheckName = "simple_name";
            public const string DescriptiveCheckName = "descriptive_name";
            public const string DescriptiveCheckAdditionalMessage = "descriptive_additional";

            private HealthCheckRunner _runner;
            private Mock<IAmAHealthCheck> _simpleHealthCheck;
            private Mock<IAmADescriptiveHealthCheck> _descriptiveHealthCheck;

            public HealthCheckResults Result;

            protected override void Given()
            {
                _simpleHealthCheck = new Mock<IAmAHealthCheck>();
                _simpleHealthCheck.Setup(x => x.Name).Returns(Scenario.SimpleCheckName);

                _descriptiveHealthCheck = new Mock<IAmADescriptiveHealthCheck>();
                _descriptiveHealthCheck
                    .Setup(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<Stopwatch>(), CancellationToken.None))
                    .ReturnsAsync(
                        HealthCheckResult.Pass(
                            Scenario.DescriptiveCheckName,
                            TimeSpan.FromSeconds(123),
                            DescriptiveCheckAdditionalMessage));

                _runner = new HealthCheckRunner(new[] { _simpleHealthCheck.Object }, new[] { _descriptiveHealthCheck.Object });
            }

            protected override async Task WhenAsync()
            {
                Result = await _runner.DoAsync(CancellationToken.None);
            }
        }

        private readonly Scenario _scenario;

        public WhenTheSystemIsHealthy(Scenario scenario)
        {
            _scenario = scenario;
        }
        
        [Fact]
        public void Two_results_should_be_returned()
        {
            _scenario.Result.Results.Count().Should().Be(2);
        }

        [Fact]
        public void The_name_of_simple_checks_should_be_populated()
        {
            _scenario.Result.Results.Should().Contain(x => x.Name == Scenario.SimpleCheckName);
        }

        [Fact]
        public void The_name_of_descriptive_checks_should_be_kept()
        {
            _scenario.Result.Results.Should().Contain(x => x.Name == Scenario.DescriptiveCheckName);
        }

        [Fact]
        public void The_execution_time_of_descriptive_checks_should_be_kept()
        {
            _scenario.Result.Results.Should().Contain(x => x.ExecutionTime == TimeSpan.FromSeconds(123));
        }

        [Fact]
        public void The_results_should_indicate_success()
        {
            var allPassed = _scenario.Result.Results.All(x => x.Passed);
            allPassed.Should().BeTrue();
        }

        [Fact]
        public void The_result_should_not_contain_an_exception()
        {
            var noExceptions = _scenario.Result.Results.All(x => x.ExceptionMessage == null);
            noExceptions.Should().BeTrue();
        }

        [Fact]
        public void The_result_should_contain_a_correlation_id()
        {
            _scenario.Result.CorrelationId.Should().NotBeEmpty();
        }
    }
}