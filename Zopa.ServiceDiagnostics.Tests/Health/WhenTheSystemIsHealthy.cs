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
    public class WhenTheSystemIsHealthy : AsyncScenario
    {
        private const string SimpleCheckName = "simple_name";
        private const string DescriptiveCheckName = "descriptive_name";
        private const string DescriptiveCheckAdditionalMessage = "descriptive_additional";

        private HealthCheckRunner _runner;
        private Mock<IAmAHealthCheck> _simpleHealthCheck;
        private Mock<IAmADescriptiveHealthCheck> _descriptiveHealthCheck;
        
        private HealthCheckResults _result;

        protected override void Given()
        {
            _simpleHealthCheck = new Mock<IAmAHealthCheck>();
            _simpleHealthCheck.Setup(x => x.Name).Returns(SimpleCheckName);

            _descriptiveHealthCheck = new Mock<IAmADescriptiveHealthCheck>();
            _descriptiveHealthCheck.Setup(x => x.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<Stopwatch>())).ReturnsAsync(HealthCheckResult.Pass(DescriptiveCheckName, TimeSpan.FromSeconds(123), DescriptiveCheckAdditionalMessage));

            _runner = new HealthCheckRunner(new[] { _simpleHealthCheck.Object }, new[] { _descriptiveHealthCheck.Object });
        }

        protected override async Task WhenAsync()
        {
            _result = await _runner.DoAsync();
        }

        [Test]
        public void Two_results_should_be_returned()
        {
            _result.Results.Count().Should().Be(2);
        }

        [Test]
        public void The_name_of_simple_checks_should_be_populated()
        {
            _result.Results.Should().Contain(x => x.Name == SimpleCheckName);
        }

        [Test]
        public void The_name_of_descriptive_checks_should_be_kept()
        {
            _result.Results.Should().Contain(x => x.Name == DescriptiveCheckName);
        }

        [Test]
        public void The_execution_time_of_descriptive_checks_should_be_kept()
        {
            _result.Results.Should().Contain(x => x.ExecutionTime == TimeSpan.FromSeconds(123));
        }

        [Test]
        public void The_results_should_indicate_success()
        {
            var allPassed = _result.Results.All(x => x.Passed);
            allPassed.Should().BeTrue();
        }

        [Test]
        public void The_result_should_not_contain_an_exception()
        {
            var noExceptions = _result.Results.All(x => x.ExceptionMessage == null);
            noExceptions.Should().BeTrue();
        }

        [Test]
        public void The_result_should_contain_a_correlation_id()
        {
            _result.CorrelationId.Should().NotBeEmpty();
        }
    }
}