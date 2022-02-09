using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MarsOffice.Tvg.Content.Abstractions;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Polly;

namespace MarsOffice.Tvg.Content.Services
{
    class RedditCommentData
    {
        public string Body { get; set; }
        public int Score { get; set; }
        public string Id { get; set; }
        public string Author { get; set; }
        public double created_utc { get; set; }
    }
    class RedditComment
    {
        public RedditCommentData Data { get; set; }
        public string Kind { get; set; }
    }

    class RedditPostData
    {
        public string Selftext { get; set; }
        public string Title { get; set; }
        public int Score { get; set; }
        public string Id { get; set; }
        public int num_comments { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public double created_utc { get; set; }
    }
    class RedditPost
    {
        public RedditPostData Data { get; set; }
        public string Kind { get; set; }
    }
    class RedditListingData<T> where T : class
    {
        public IEnumerable<T> Children { get; set; }
    }
    class RedditListing<T> where T : class
    {
        public RedditListingData<T> Data { get; set; }
        public string Kind { get; set; }
    }
    public class RedditContentService : AbstractContentService
    {
        public RedditContentService(HttpClient httpClient, CloudTable usedPostsTable) : base(httpClient, usedPostsTable)
        {
        }

        public override async Task<Post> GetOneRandomPost(RequestContent request)
        {
            var response = await Policy
                                .Handle<Exception>()
                                .WaitAndRetryAsync(new[] { TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(1000), TimeSpan.FromMilliseconds(1500) })
                                .ExecuteAsync(async () =>
                                {
                                    var r = await httpClient.GetAsync($"https://www.reddit.com/r/{request.ContentTopic}/random.json");
                                    r.EnsureSuccessStatusCode();
                                    var json = await r.Content.ReadAsStringAsync();
                                    return JsonConvert.DeserializeObject<JArray>(json, new JsonSerializerSettings {
                                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                                        NullValueHandling = NullValueHandling.Ignore
                                    });
                                });

            var postListing = response[0].ToObject<RedditListing<RedditPost>>();
            var commentsListing = response[1].ToObject<RedditListing<RedditComment>>();
            return new Post
            {
                Author = postListing.Data.Children.First().Data.Author,
                Category = postListing.Data.Children.First().Data.Category,
                Text = string.IsNullOrEmpty(postListing.Data.Children.First().Data.Selftext) ? postListing.Data.Children.First().Data.Title : postListing.Data.Children.First().Data.Selftext,
                UniqueId = postListing.Data.Children.First().Data.Id,
                Topic = request.ContentTopic,
                PostedDate = DateTimeOffset.FromUnixTimeSeconds((long)Math.Round(postListing.Data.Children.First().Data.created_utc)),
                Comments =
                    commentsListing.Data.Children.Select(x => x.Data).Select(c => new Post
                    {
                        Author = c.Author,
                        Comments = null,
                        Text = c.Body,
                        Topic = request.ContentTopic,
                        UniqueId = c.Id,
                        Score = c.Score,
                        PostedDate = DateTimeOffset.FromUnixTimeSeconds((long)Math.Round(c.created_utc))
                    }).ToList()
            };
        }
    }
}