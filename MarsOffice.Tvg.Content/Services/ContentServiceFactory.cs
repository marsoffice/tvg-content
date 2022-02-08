using System.Net.Http;
using MarsOffice.Tvg.Content.Abstractions;

namespace MarsOffice.Tvg.Content.Services
{
    public static class ContentServiceFactory
    {
        public static AbstractContentService Create(string contentType, HttpClient httpClient)
        {
            if (contentType == ContentTypesEnum.Jokes)
            {
                return null;
            }
            if (contentType == ContentTypesEnum.Reddit)
            {
                return null;
            }
            throw new System.Exception("Unknown content type");
        }
    }
}