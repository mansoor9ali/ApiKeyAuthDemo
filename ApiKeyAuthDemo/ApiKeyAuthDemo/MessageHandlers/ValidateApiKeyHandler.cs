using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;


namespace ApiKeyAuthDemo.MessageHandlers
{
    public class ValidateApiKeyHandler : DelegatingHandler
    {
        private const string APIKeyToCheck = "56756GGJGHJG&&*jJJJ99";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            IEnumerable<string> requestHeaders;
            var checkApiKeyExists = request.Headers.TryGetValues("apikey", out requestHeaders);
            if (!checkApiKeyExists)
            {
                return SendError("You can't use the API without the key.", HttpStatusCode.Forbidden);
            }
            else
            {
                var firstOrDefault = requestHeaders.FirstOrDefault();
                if (firstOrDefault != null && firstOrDefault.Equals(APIKeyToCheck))
                {
                    return base.SendAsync(request, cancellationToken);
                }
                else
                {
                    return SendError("Invalid API key.", HttpStatusCode.Forbidden);
                }

            }
        }
        private Task<HttpResponseMessage> SendError(string error, HttpStatusCode code)
        {
            var response = new HttpResponseMessage();
            response.Content = new StringContent(error);
            response.StatusCode = code;

            return Task<HttpResponseMessage>.Factory.StartNew(() => response);
        }
    }

}