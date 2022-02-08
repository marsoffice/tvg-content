using System;
using System.Threading.Tasks;
using MarsOffice.Microfunction;
using MarsOffice.Tvg.Content.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace MarsOffice.Tvg.Content
{
    public class ContentTypes
    {

        public ContentTypes()
        {
        }

        // https://www.reddit.com/r/AskReddit/random.json
        [FunctionName("GetAllContentTypes")]
        public async Task<IActionResult> GetAllContentTypes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/content/getAllContentTypes")] HttpRequest req,
            ILogger log
            )
        {
            try
            {
                await Task.CompletedTask;
                return new OkObjectResult(ContentTypesEnum.AllContentTypes);
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception occured in function");
                return new BadRequestObjectResult(Errors.Extract(e));
            }
        }
    }
}