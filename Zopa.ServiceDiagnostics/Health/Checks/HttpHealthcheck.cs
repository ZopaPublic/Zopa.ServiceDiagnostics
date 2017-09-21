using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Zopa.ServiceDiagnostics.Health.Checks
{
    public class HttpHealthcheck : IAmAHealthCheck
    {
        private readonly string _uri;

        public HttpHealthcheck(string uri)
        {
            _uri = uri;
        }

        public virtual string Name => $"Http healthcheck against {_uri}";

        public async Task ExecuteAsync(Guid correlationId, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _uri);
            request.Headers.Add("X-Correlation-Id", correlationId.ToString());

            var response = await new HttpClient().SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Could not connect to {_uri}.  Received response code {response.StatusCode} with reason {response.ReasonPhrase}");
            }
        }
    }
}