using System;

namespace MarsOffice.Tvg.Content.Abstractions
{
    public class Comment
    {
        public string Author { get; set; }
        public string AuthorAvatar { get; set; }
        public string Text { get; set; }
        public DateTimeOffset PostedDate { get; set; }
    }
}