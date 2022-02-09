using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MarsOffice.Tvg.Content.Abstractions;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;

namespace MarsOffice.Tvg.Content.Services
{
    class DadJoke {
        public string Id { get; set; }
        public string Joke { get; set; }
    }
    public class JokesContentService : AbstractContentService
    {
        public JokesContentService(HttpClient httpClient, CloudTable usedPostsTable) : base(httpClient, usedPostsTable)
        {
        }

        public override async Task<Post> GetOneRandomPost(RequestContent request)
        {
            var response = await Policy
                                .Handle<Exception>()
                                .WaitAndRetryAsync(new[] { TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1500) })
                                .ExecuteAsync(async () =>
                                {
                                    var req = new HttpRequestMessage(HttpMethod.Get, $"https://icanhazdadjoke.com/");
                                    req.Headers.TryAddWithoutValidation("Accept", "application/json");
                                    var r = await httpClient.SendAsync(req);
                                    r.EnsureSuccessStatusCode();
                                    var json = await r.Content.ReadAsStringAsync();
                                    return JsonConvert.DeserializeObject<DadJoke>(json, new JsonSerializerSettings
                                    {
                                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                        NullValueHandling = NullValueHandling.Ignore
                                    });
                                });
            return new Post { 
                Author = "icanhazdadjoke.com",
                Category = "Funny",
                PostedDate = DateTimeOffset.UtcNow,
                Text = response.Joke,
                UniqueId = response.Id,
                Score = 10,
                Topic = "icanhazdadjoke.com"
            };
        }
    }
}