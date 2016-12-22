using System.Threading.Tasks;
using NUnit.Framework;

namespace Zopa.ServiceDiagnostics.Tests
{
    public abstract class AsyncScenario
    {
        [OneTimeSetUp]
        public async Task Setup()
        {
            Given();
            await WhenAsync();
        }

        protected virtual void Given() { }
        protected virtual Task WhenAsync() { return Task.FromResult(0); }
    }
}
