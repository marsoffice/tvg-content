using System.Collections.Generic;
using MarsOffice.Tvg.Content.Abstractions;

namespace MarsOffice.Tvg.Content.Entities
{
    public class ServiceResponse
    {
        public string Category { get; set; }
        public IEnumerable<Post> Posts { get; set; }
    }
}