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
    public class WhenADescriptiveHealthCheckThrowsAnException : IClassFixture<WhenADescriptiveHealthCheckThrowsAnException.Scenario>
    {
        public class Scenario : BaseScenario
        {
            internal const string Name = "some_name";
            private readonly Exception _exception = new Exception("cheese");

            private HealthCheckRunner _runner;
            private Mock<IAmADescriptiveHealthCheck> _healthCheck;

            public HealthCheckResults Result;

            protected override void Given()
            {
                _healthCheck = new Mock<IAmADescriptiveHealthCheck>();
                _healthCheck.Setup(x => x.Name).Returns(Name);
                _healthCheck.Setup(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<Stopwatch>(), CancellationToken.None)).Throws(_exception);
                _runner = new HealthCheckRunner(new IAmAHealthCheck[0], new[] { _healthCheck.Object });
            }

            protected override async Task WhenAsync()
            {
                Result = await _runner.DoAsync(CancellationToken.None);
            }
        }

        readonly Scenario _scenario;

        public WhenADescriptiveHealthCheckThrowsAnException(Scenario scenario)
        {
            _scenario = scenario;
        }

        [Fact]
        public void One_result_should_be_returned()
        {
            _scenario.Result.Results.Count().Should().Be(1);
        }

        [Fact]
        public void The_results_name_should_be_that_of_the_health_check()
        {
            _scenario.Result.Results.First().Name.Should().Be(Scenario.Name);
        }

        [Fact]
        public void The_result_should_indicate_failure()
        {
            _scenario.Result.Results.First().Passed.Should().BeFalse();
        }

        [Fact]
        public void The_result_should_contain_an_exception_message()
        {
            _scenario.Result.Results.First().ExceptionMessage.Should().NotBeEmpty();
        }

        [Fact]
        public void The_result_should_contain_the_correct_exception_message()
        {
            _scenario.Result.Results.First().ExceptionMessage?.Should().StartWith("cheese");
        }

        [Fact]
        public void The_result_should_contain_a_correlation_id()
        {
            _scenario.Result.CorrelationId.Should().NotBeEmpty();
        }
    }
}