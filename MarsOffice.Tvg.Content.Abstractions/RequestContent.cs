using System;

namespace MarsOffice.Tvg.Content.Abstractions
{
    public class RequestContent
    {
        public string VideoId { get; set; }
        public string JobId { get; set; }
        public string ContentType { get; set; }
        public string ContentTopic { get; set; }
        public bool? ContentGetLatestPosts { get; set; }
        public DateTimeOffset? ContentStartDate { get; set; }
        public int? ContentMinChars { get; set; }
        public int? ContentMaxChars { get; set; }
        public string ContentTranslateFromLanguage { get; set; }
        public string ContentTranslateToLanguage { get; set; }
        public int? ContentNoOfIncludedTopComments { get; set; }
        public bool? ContentIncludeLinks { get; set; }
        public int? ContentMinPosts { get; set; }
        public int? ContentMaxPosts { get; set; }
    }
}