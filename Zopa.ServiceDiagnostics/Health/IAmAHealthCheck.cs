using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zopa.ServiceDiagnostics.Health
{
    public interface IAmAHealthCheck
    {
        string Name { get; }

        /// <summary>
        /// Runs the actual health check
        /// </summary>
        /// <param name="correlationId">Each unique run of all checks will have an associated correlation id to help tie your logs together (assuming you log such things)</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>Anyhting if the check was successful. A health check's run is marked as unsuccessful if this method throws an exception</returns>
        Task ExecuteAsync(Guid correlationId, CancellationToken cancellationToken);
    }
}