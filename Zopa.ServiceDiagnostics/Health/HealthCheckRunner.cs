using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Zopa.ServiceDiagnostics.Health
{
    public class HealthCheckRunner : IHealthCheckRunner
    {
        private readonly IEnumerable<IAmAHealthCheck> _healthChecks;
        private readonly IEnumerable<IAmADescriptiveHealthCheck> _descriptiveHealthChecks;

        public HealthCheckRunner(IEnumerable<IAmAHealthCheck> healthChecks, IEnumerable<IAmADescriptiveHealthCheck> descriptiveHealthChecks)
        {
            _healthChecks = healthChecks;
            _descriptiveHealthChecks = descriptiveHealthChecks;
        }

        public async Task<HealthCheckResults> DoAsync(CancellationToken cancellationToken)
        {
            var tasks = new List<Task<HealthCheckResult>>();

            Guid correlationId = Guid.NewGuid();

            foreach (var check in _healthChecks)
            {
                tasks.Add(RunStandardAsync(check, correlationId, cancellationToken));
            }

            foreach (var check in _descriptiveHealthChecks)
            {
                tasks.Add(RunDescriptiveAsync(check, correlationId, cancellationToken));
            }

            var results = await Task.WhenAll(tasks);

            return new HealthCheckResults(correlationId, results);
        }

        private async Task<HealthCheckResult> RunStandardAsync(IAmAHealthCheck healthCheck, Guid correlationId, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                
                await healthCheck.ExecuteAsync(correlationId, cancellationToken);
                return HealthCheckResult.Pass(healthCheck.Name, sw.Elapsed);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Fail(healthCheck.Name, ex, sw.Elapsed);
            }
        }

        private async Task<HealthCheckResult> RunDescriptiveAsync(IAmADescriptiveHealthCheck healthCheck, Guid correlationId, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                return await healthCheck.ExecuteAsync(correlationId, sw, cancellationToken);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Fail(healthCheck.Name, ex, sw.Elapsed);
            }
        }
    }
}