using System;

namespace Kuvalda.Storage.Http
{
    public class EndpointOptions
    {
        public string RepoUrl { get; set; }
        public string ObjectsFormat { get; set; }
        public string TagsFormat { get; set; }
        public string PushTagsFormat { get; set; }
        public string PushObjectFormat { get; set; }
        public string RefsFormat { get; set; }
        public string PushRefsFormat { get; set; }

        public Uri GetObjectUri(string key)
            => new Uri(new Uri(RepoUrl), string.Format(ObjectsFormat, key));
        
        public Uri GetPushObjectUri(string key)
            => new Uri(new Uri(RepoUrl), string.Format(PushObjectFormat, key));
        
        public Uri GetTagUri(string key)
            => new Uri(new Uri(RepoUrl), string.Format(TagsFormat, key));
        
        public Uri GetPushTagUri(string key)
            => new Uri(new Uri(RepoUrl), string.Format(PushTagsFormat, key));
        
        public Uri GetPushRefsUri(string key)
            => new Uri(new Uri(RepoUrl), string.Format(PushRefsFormat, key));
        
        public Uri GetRefsUri(string key)
            => new Uri(new Uri(RepoUrl), string.Format(RefsFormat, key));

    }
}