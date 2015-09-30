using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SemanticLogging.EventHub
{
    internal sealed class HttpClientImp : IHttpClient
    {
        private readonly HttpClient httpClient;

        public HttpClientImp()
        {
            httpClient = new HttpClient();
        }

        public HttpRequestHeaders DefaultRequestHeaders
        {
            get { return httpClient.DefaultRequestHeaders; }
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return httpClient.PostAsync(requestUri, content);
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}