using System.Net.Http;
using NSubstitute;

namespace SemanticLogging.EventHub.Tests.Util
{
    internal static class HttpClientTestHelper
    {
        internal static IHttpClient Create()
        {
            var client = Substitute.For<IHttpClient>();
            client.DefaultRequestHeaders.Returns(new HttpClient().DefaultRequestHeaders);

            return client;
        }
    }
}
