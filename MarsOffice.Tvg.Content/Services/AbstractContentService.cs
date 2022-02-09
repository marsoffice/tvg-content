using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MarsOffice.Tvg.Content.Abstractions;
using MarsOffice.Tvg.Content.Entities;
using Microsoft.Azure.Cosmos.Table;

namespace MarsOffice.Tvg.Content.Services
{
    public abstract class AbstractContentService
    {
        protected readonly HttpClient httpClient;
        protected readonly CloudTable usedPostsTable;
        public AbstractContentService(HttpClient httpClient, CloudTable usedPostsTable)
        {
            this.httpClient = httpClient;
            this.usedPostsTable = usedPostsTable;
        }

        public async Task<ServiceResponse> Execute(RequestContent request)
        {
            var tries = 0;
            var posts = new List<Post>();
            while (tries < 100 && posts.Count < (request.ContentMaxPosts ?? 5))
            {
                var post = await GetOneRandomPost(request);
                if (post == null)
                {
                    tries++;
                    continue;
                }
                var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, request.JobId);
                var orFilter = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, post.UniqueId);

                var query = new TableQuery<UsedPost>()
                    .Where(
                        TableQuery.CombineFilters(
                            TableQuery.CombineFilters(
                                filter,
                                TableOperators.And,
                                TableQuery.GenerateFilterCondition("ContentType", QueryComparisons.Equal, request.ContentType)
                            ), TableOperators.And, orFilter
                        )
                    ).Take(1);
                var dbResults = await usedPostsTable.ExecuteQuerySegmentedAsync(query, null);
                if (dbResults.Any())
                {
                    tries++;
                    continue;
                }
                posts.Add(
                    post
                );
                tries++;
            }

            if (!posts.Any())
            {
                throw new System.Exception("Unable to retrieve posts");
            }

            return new ServiceResponse
            {
                Category = posts.Where(x => !string.IsNullOrEmpty(x.Category)).FirstOrDefault()?.Category,
                Posts = posts
            };
        }

        public abstract Task<Post> GetOneRandomPost(RequestContent request);
    }
}