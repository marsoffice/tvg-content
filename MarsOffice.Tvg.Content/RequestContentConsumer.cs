using System;
using System.Net.Http;
using System.Threading.Tasks;
using MarsOffice.Tvg.Content.Abstractions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace MarsOffice.Tvg.Content
{
    public class RequestContentConsumer
    {
        private readonly HttpClient _httpClient;
        
        public RequestContentConsumer(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [FunctionName("RequestContentConsumer")]
        public async Task Run(
            [QueueTrigger("request-content", Connection = "localsaconnectionstring")]RequestContent myQueueItem,
            [Queue("content-response", Connection = "localsaconnectionstring")] IAsyncCollector<ContentResponse> contentResponseQueue,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
        }
    }
}
