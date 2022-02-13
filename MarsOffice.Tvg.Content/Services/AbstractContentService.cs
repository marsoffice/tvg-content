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
            while (tries < 1000 && posts.Count < (request.ContentMaxPosts ?? 5))
            {
                var post = await GetOneRandomPost(request);
                if (post == null)
                {
                    tries++;
                    continue;
                }
                if (request.ContentMinChars != null && post.Text.Length < request.ContentMinChars.Value)
                {
                    tries++;
                    continue;
                }
                if (request.ContentMaxChars != null && post.Text.Length > request.ContentMaxChars.Value)
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

                var comments = post.Comments;
                post.Comments = null;
                posts.Add(
                    post
                );

                if (request.ContentNoOfIncludedTopComments != null)
                {
                    if (comments == null || !comments.Any())
                    {
                        posts.Remove(post);
                        tries++;
                        continue;
                    }
                    var commentsQueryable = comments.AsQueryable();
                    commentsQueryable = commentsQueryable.OrderByDescending(x => x.Score);
                    if (request.ContentMinChars != null)
                    {
                        commentsQueryable = commentsQueryable.Where(x => x.Text.Length > request.ContentMinChars.Value);
                    }
                    if (request.ContentMaxChars != null)
                    {
                        commentsQueryable = commentsQueryable.Where(x => x.Text.Length <= request.ContentMaxChars.Value);
                    }
                    commentsQueryable = commentsQueryable.Take(request.ContentNoOfIncludedTopComments.Value);
                    var commentsResult = commentsQueryable.ToList();
                    if (commentsResult.Count < request.ContentNoOfIncludedTopComments.Value)
                    {
                        posts.Remove(post);
                        tries++;
                        continue;
                    }
                    posts.AddRange(commentsResult);
                }

                tries++;
            }

            if (!posts.Any())
            {
                throw new System.Exception("Unable to retrieve posts");
            }


            foreach (var p in posts)
            {
                var insertOp = TableOperation.InsertOrMerge(new UsedPost
                {
                    ContentType = request.ContentType,
                    PartitionKey = request.JobId,
                    RowKey = p.UniqueId,
                    UniqueId = p.UniqueId,
                    JobId = request.JobId,
                    ETag = "*"
                });
                await usedPostsTable.ExecuteAsync(insertOp);
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