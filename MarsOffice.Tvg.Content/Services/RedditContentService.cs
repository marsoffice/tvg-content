using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MarsOffice.Tvg.Content.Abstractions;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Polly;

namespace MarsOffice.Tvg.Content.Services
{
    public class RedditContentService : AbstractContentService
    {
        public RedditContentService(HttpClient httpClient, CloudTable usedPostsTable) : base(httpClient, usedPostsTable)
        {
        }

        public override async Task<IEnumerable<Post>> GetPosts(RequestContent request, int no)
        {
            var response = await Policy
                                .Handle<Exception>()
                                .WaitAndRetryAsync(new[] { TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1500) })
                                .ExecuteAsync(async () => {
                                    var r = await httpClient.GetAsync($"https://www.reddit.com/r/{request.ContentTopic}/random.json");
                                    r.EnsureSuccessStatusCode();
                                    var json = await r.Content.ReadAsStringAsync();
                                    return JsonConvert.DeserializeObject<dynamic>(json);
                                });

            return response;
        }
    }
}