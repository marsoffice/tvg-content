using System.Net.Http;
using System.Threading.Tasks;
using MarsOffice.Tvg.Content.Abstractions;
using MarsOffice.Tvg.Content.Entities;

namespace MarsOffice.Tvg.Content.Services
{
    public abstract class AbstractContentService
    {
        protected readonly HttpClient httpClient;
        public AbstractContentService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public abstract Task<ServiceResponse> GetContent(RequestContent request);
    }
}