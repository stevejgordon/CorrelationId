using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CorrelationId.Net48.Tests
{
    public class Net48MvcSampleApiClient
    {
        private System.Net.Http.HttpClient Client { get; }
        public Net48MvcSampleApiClient(System.Net.Http.HttpClient client)
        {
            Client = client;
            Client.BaseAddress = new Uri("http://localhost:31488");
        }

        public async Task<HttpResponseMessage> GetAsync()
        {
            return await Client.GetAsync("/api/correlationId");
        }
    }
}