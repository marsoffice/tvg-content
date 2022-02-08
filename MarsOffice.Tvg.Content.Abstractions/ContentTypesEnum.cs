using System.Collections.Generic;

namespace MarsOffice.Tvg.Content.Abstractions
{
    public static class ContentTypesEnum
    {
        public static readonly string Reddit = "Reddit";
        public static readonly string Jokes = "Jokes";

        public static readonly IEnumerable<string> AllContentTypes = new[] { Reddit, Jokes };
    }
}