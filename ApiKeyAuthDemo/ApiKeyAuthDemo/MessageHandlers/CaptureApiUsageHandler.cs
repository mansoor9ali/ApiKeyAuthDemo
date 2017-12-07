using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApiKeyAuthDemo.Models;

namespace ApiKeyAuthDemo.MessageHandlers
{
    public class CaptureApiUsageHandler : DelegatingHandler
    {
        private static readonly IApiUsageRepository _repo = new ApiUsageRepository();

        private Task<HttpResponseMessage> SendError(string error, HttpStatusCode code)
        {
            var response = new HttpResponseMessage();
            response.Content = new StringContent(error);
            response.StatusCode = code;

            return Task<HttpResponseMessage>.Factory.StartNew(() => response);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            IEnumerable<string> requestHeaders;
            var checkApiKeyExists = request.Headers.TryGetValues("apikey", out requestHeaders);
            if (!checkApiKeyExists)
            {
                return SendError("You can't use the API without the key.", HttpStatusCode.Forbidden);
            }
            else
            {
                var apikey = requestHeaders.FirstOrDefault();
                var apiRequest = new WebApiUsageRequest(request, apikey);
                request.Content.ReadAsStringAsync().ContinueWith(t =>
                {
                    apiRequest.Content = t.Result;
                    _repo.Add(apiRequest);
                }, cancellationToken);

                // ************ No Need to capture out Respose  ********************

                //return base.SendAsync(request, cancellationToken).ContinueWith(
                //    task =>
                //    {
                //        var apiResponse = new WebApiUsageResponse(task.Result, apikey);
                //        apiResponse.Content = task.Result.Content.ReadAsStringAsync().Result;
                //        _repo.Add(apiResponse);
                //        return task.Result;
                //    }, cancellationToken);


                return base.SendAsync(request, cancellationToken);


            }



        }
    }
}