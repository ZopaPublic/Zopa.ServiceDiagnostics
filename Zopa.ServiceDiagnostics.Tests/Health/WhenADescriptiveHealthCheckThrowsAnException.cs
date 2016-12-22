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
    public class WhenADescriptiveHealthCheckThrowsAnException : AsyncScenario
    {
        private const string Name = "some_name";
        private readonly Exception _exception = new Exception("cheese");

        private HealthCheckRunner _runner;
        private Mock<IAmADescriptiveHealthCheck> _healthCheck;

        private HealthCheckResults _result;

        protected override void Given()
        {
            _healthCheck = new Mock<IAmADescriptiveHealthCheck>();
            _healthCheck.Setup(x => x.Name).Returns(Name);
            _healthCheck.Setup(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<Stopwatch>())).Throws(_exception);
            _runner = new HealthCheckRunner(new IAmAHealthCheck[0], new[] { _healthCheck.Object });
        }

        protected override async Task WhenAsync()
        {
            _result = await _runner.DoAsync();
        }

        [Test]
        public void One_result_should_be_returned()
        {
            _result.Results.Count().Should().Be(1);
        }

        [Test]
        public void The_results_name_should_be_that_of_the_health_check()
        {
            _result.Results.First().Name.Should().Be(Name);
        }

        [Test]
        public void The_result_should_indicate_failure()
        {
            _result.Results.First().Passed.Should().BeFalse();
        }

        [Test]
        public void The_result_should_contain_an_exception_message()
        {
            _result.Results.First().ExceptionMessage.Should().NotBeEmpty();
        }

        [Test]
        public void The_result_should_contain_the_correct_exception_message()
        {
            _result.Results.First().ExceptionMessage?.Should().StartWith("cheese");
        }

        [Test]
        public void The_result_should_contain_a_correlation_id()
        {
            _result.CorrelationId.Should().NotBeEmpty();
        }
    }
}