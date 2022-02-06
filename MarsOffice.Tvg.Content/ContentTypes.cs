using System;
using System.Threading.Tasks;
using MarsOffice.Microfunction;
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

        [FunctionName("GetAllContentTypes")]
        public async Task<IActionResult> GetAllSpeechTypes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/content/getAllContentTypes")] HttpRequest req,
            ILogger log
            )
        {
            try
            {
                var principal = MarsOfficePrincipal.Parse(req);
                var userId = principal.FindFirst("id").Value;

                return new OkObjectResult(new[] { 
                    "Reddit",
                    "Jokes"
                });
            }
            catch (Exception e)
            {
                log.LogError(e, "Exception occured in function");
                return new BadRequestObjectResult(Errors.Extract(e));
            }
        }
    }
}