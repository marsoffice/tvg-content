using System.Net.Http;
using MarsOffice.Tvg.Content.Abstractions;
using Microsoft.Azure.Cosmos.Table;

namespace MarsOffice.Tvg.Content.Services
{
    public static class ContentServiceFactory
    {
        public static AbstractContentService Create(string contentType, HttpClient httpClient, CloudTable usedPostsTable)
        {
            if (contentType == ContentTypesEnum.Jokes)
            {
                return new JokesContentService(httpClient, usedPostsTable);
            }
            if (contentType == ContentTypesEnum.Reddit)
            {
                return new RedditContentService(httpClient, usedPostsTable);
            }
            throw new System.Exception("Unknown content type");
        }
    }
}