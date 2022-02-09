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
            while (tries < 10)
            {
                var batch = await GetPosts(request, request.ContentMaxPosts ?? 5);
                if (!batch.Any())
                {
                    tries++;
                    continue;
                }
                var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, request.JobId);
                var orFilters = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, batch.First().UniqueId);
                for (var i = 1; i < batch.Count(); i++)
                {
                    orFilters = TableQuery.CombineFilters(
                        orFilters,
                        TableOperators.Or,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, batch.ElementAt(i).UniqueId)
                    );
                }
                var query = new TableQuery<UsedPost>()
                    .Where(
                        TableQuery.CombineFilters(
                            TableQuery.CombineFilters(
                                filter,
                                TableOperators.And,
                                TableQuery.GenerateFilterCondition("ContentType", QueryComparisons.Equal, request.ContentType)
                            ), TableOperators.And, orFilters
                        )
                    );
                var dbResults = await usedPostsTable.ExecuteQuerySegmentedAsync(query, null);
                posts.AddRange(
                    batch.Where(p => !dbResults.Any(dbP => p.UniqueId == dbP.RowKey)).ToList()
                );
                tries++;
            }

            if (!posts.Any())
            {
                throw new System.Exception("Unable to retrieve posts");
            }

            return new ServiceResponse { 
                Category = posts.Where(x => !string.IsNullOrEmpty(x.Category)).FirstOrDefault()?.Category,
                Posts = posts
            };
        }

        public abstract Task<IEnumerable<Post>> GetPosts(RequestContent request, int no);
    }
}