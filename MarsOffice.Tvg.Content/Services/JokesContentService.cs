using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using MarsOffice.Tvg.Content.Abstractions;
using Microsoft.Azure.Cosmos.Table;

namespace MarsOffice.Tvg.Content.Services
{
    public class JokesContentService : AbstractContentService
    {
        public JokesContentService(HttpClient httpClient, CloudTable usedPostsTable) : base(httpClient, usedPostsTable)
        {
        }

        public override async Task<IEnumerable<Post>> GetPosts(RequestContent request, int no)
        {
            throw new System.NotImplementedException();
        }
    }
}