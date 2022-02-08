using MarsOffice.Tvg.Content.Abstractions;
using Microsoft.Azure.Cosmos.Table;

namespace MarsOffice.Tvg.Content.Entities
{
    public class UsedPost : TableEntity
    {
        public string UniqueId { get; set; }
        public string ContentType { get; set; }
        public string UserId { get; set; }
    }
}