using System.Threading.Tasks;

namespace Zopa.ServiceDiagnostics.Tests
{
    public abstract class BaseScenario
    {
        protected BaseScenario()
        {
            Given();
            WhenAsync().Wait();
        }

        protected virtual void Given() { }
        protected virtual Task WhenAsync() { return Task.FromResult(0); }
    }
}
