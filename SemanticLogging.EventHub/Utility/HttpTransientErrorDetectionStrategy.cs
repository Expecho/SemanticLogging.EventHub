using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace SemanticLogging.EventHub.Utility
{
    public class HttpTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        private readonly List<HttpStatusCode> statusCodes = new List<HttpStatusCode>
        {
            HttpStatusCode.GatewayTimeout,
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.ServiceUnavailable
        };

        public bool IsTransient(Exception ex)
        {
            var we = ex as WebException;
            if (we == null)
                return false;

            var response = we.Response as HttpWebResponse;

            var isTransient = response != null && statusCodes.Contains(response.StatusCode);
            return isTransient;
        }

    }
}
