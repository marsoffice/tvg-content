using System.Collections.Generic;

namespace MarsOffice.Tvg.Content.Abstractions
{
    public class ContentRetrieved
    {
        public string VideoId { get; set; }
        public string JobId { get; set; }
        public string ContentType { get; set; }
        public IEnumerable<Post> Posts { get; set; }
    }
}