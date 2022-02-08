using System;
using System.Collections.Generic;

namespace MarsOffice.Tvg.Content.Abstractions
{
    public class Post
    {
        public string Topic { get; set; }
        public string UniqueId { get; set; }
        public string Author { get; set; }
        public string AuthorAvatar { get; set; }
        public DateTimeOffset PostedDate { get; set; }
        public string Text { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
    }
}