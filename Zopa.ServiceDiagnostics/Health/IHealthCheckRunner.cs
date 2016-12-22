using System.Threading.Tasks;

namespace Zopa.ServiceDiagnostics.Health
{
    public interface IHealthCheckRunner
    {
        Task<HealthCheckResults> DoAsync();
    }
}