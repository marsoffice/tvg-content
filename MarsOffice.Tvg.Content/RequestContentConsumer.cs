using System;
using System.Net.Http;
using System.Threading.Tasks;
using MarsOffice.Tvg.Content.Abstractions;
using MarsOffice.Tvg.Content.Services;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage.Queue.Protocol;
using Microsoft.Azure.WebJobs;
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
            [QueueTrigger("request-content", Connection = "localsaconnectionstring")] QueueMessage message,
            [Queue("content-response", Connection = "localsaconnectionstring")] IAsyncCollector<ContentResponse> contentResponseQueue,
            [Table("UsedPosts", Connection = "localsaconnectionstring")] CloudTable usedPostsTable,
            ILogger log)
        {
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<RequestContent>(message.Text,
                    new Newtonsoft.Json.JsonSerializerSettings
                    {
                        ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                    });
            try
            {
                var service = ContentServiceFactory.Create(request.ContentType, _httpClient, usedPostsTable);

                var reply = await service.Execute(request);


                await contentResponseQueue.AddAsync(new ContentResponse
                {
                    Success = true,
                    Category = reply.Category,
                    Posts = reply.Posts,
                    ContentType = request.ContentType,
                    JobId = request.JobId,
                    VideoId = request.VideoId,
                    UserId = request.UserId,
                    UserEmail = request.UserEmail
                });
                await contentResponseQueue.FlushAsync();
            }
            catch (Exception e)
            {
                log.LogError(e, "Function threw an exception");
                if (message.DequeueCount >= 5)
                {
                    await contentResponseQueue.AddAsync(new ContentResponse
                    {
                        Success = false,
                        Error = "ContentService: " + e.Message,
                        JobId = request.JobId,
                        VideoId = request.VideoId
                    });
                    await contentResponseQueue.FlushAsync();
                }
                throw;
            }
        }
    }
}
