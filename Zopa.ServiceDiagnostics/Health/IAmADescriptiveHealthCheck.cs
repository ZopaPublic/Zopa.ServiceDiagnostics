using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Zopa.ServiceDiagnostics.Health
{
    public interface IAmADescriptiveHealthCheck
    {
        Task<HealthCheckResult> ExecuteAsync(Guid correlationId, Stopwatch sw);

        string Name { get; }
    }
}