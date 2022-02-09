using System.Collections.Generic;

namespace MarsOffice.Tvg.Content.Abstractions
{
    public class ContentResponse
    {
        public string VideoId { get; set; }
        public string UserId { get; set; }
        public string JobId { get; set; }
        public string ContentType { get; set; }
        public string Category { get; set; }
        public IEnumerable<Post> Posts { get; set; }
        public bool Success { get; set; } = true;
        public string Error { get; set; }
    }
}