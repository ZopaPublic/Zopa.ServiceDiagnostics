using System;
using System.Collections.Generic;

namespace Zopa.ServiceDiagnostics.Health
{
    public class HealthCheckResults
    {
        public HealthCheckResults(Guid correlationId, IEnumerable<HealthCheckResult> results)
        {
            Results = results;
            CorrelationId = correlationId;
        }

        public Guid CorrelationId { get; }

        public IEnumerable<HealthCheckResult> Results { get; }
    }
}