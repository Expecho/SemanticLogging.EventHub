using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SemanticLogging.EventHub
{
    internal interface IHttpClient : IDisposable
    {
        HttpRequestHeaders DefaultRequestHeaders { get; }

        Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content);
    }
}
