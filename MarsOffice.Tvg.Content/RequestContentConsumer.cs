using System;
using System.Net.Http;
using System.Threading.Tasks;
using MarsOffice.Tvg.Content.Abstractions;
using MarsOffice.Tvg.Content.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Polly;

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
            [QueueTrigger("request-content", Connection = "localsaconnectionstring")] RequestContent request,
            [Queue("content-response", Connection = "localsaconnectionstring")] IAsyncCollector<ContentResponse> contentResponseQueue,
            ILogger log)
        {
            try
            {
                var service = ContentServiceFactory.Create(request.ContentType, _httpClient);

                var reply = await Policy
                    .Handle<Exception>()
                    .WaitAndRetryAsync(new[] { TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1500) })
                    .ExecuteAsync(async () => await service.GetContent(request));

                await contentResponseQueue.AddAsync(new ContentResponse
                {
                    Success = true,
                    Category = reply.Category,
                    Posts = reply.Posts,
                    ContentType = request.ContentType,
                    JobId = request.JobId,
                    VideoId = request.VideoId
            });
                await contentResponseQueue.FlushAsync();
            }
            catch (Exception e)
            {
                await contentResponseQueue.AddAsync(new ContentResponse
                {
                    Success = false,
                    Error = e.Message,
                    JobId = request.JobId,
                    VideoId = request.VideoId
                });
                await contentResponseQueue.FlushAsync();
            }
        }
    }
}
