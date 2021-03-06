using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using Polly;

namespace adoStats_core
{
    public class AzureDevWebService : IAzureDevWebService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly AzureDevWebUtilities _azureDevWebUtilities;

        public AzureDevWebService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _azureDevWebUtilities = new AzureDevWebUtilities();
        }



        public async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, bool retry = true)
        {
            if (retry)
            {
                return (await Policy
                    .HandleResult<HttpResponseMessage>(message => !message.IsSuccessStatusCode)
                    .WaitAndRetryForeverAsync(
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        (exception, retry, timespan) =>
                        {
                            Console.WriteLine($"Polly: {exception.Result.StatusCode}:{exception.Result.ReasonPhrase}");
                            Console.WriteLine($"Polly: {exception.Result.RequestMessage.Method} {exception.Result.RequestMessage.RequestUri.AbsoluteUri}");
                        }
                    ).ExecuteAndCaptureAsync(
                            () => _clientFactory.CreateClient().SendAsync(request)
                    )).Result;
            }
            else
            {
                return await _clientFactory.CreateClient().SendAsync(request);
            }
        }


        public HttpRequestMessage CreateDefaultRequest()
        {
            var host = _azureDevWebUtilities.GetADOHost();
            var PAT = _azureDevWebUtilities.GetADOPAT();
            var m = new HttpRequestMessage();
            var b64Pat = AzureDevWebUtilities.Base64Encode($":{PAT}");
            m.Headers.Add("Authorization", $"Basic {b64Pat}");
            m.Headers.Host = host;
            m.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            m.Headers.Add("Upgrade-Insecure-Requests", "1");
            return m;
        }
    }
}
