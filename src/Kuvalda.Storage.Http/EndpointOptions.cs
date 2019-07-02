using System;

namespace Kuvalda.Storage.Http
{
    public class EndpointOptions
    {
        public string RepoUrl { get; set; }
        public string ObjectsFormat { get; set; }
        public string TagsFormat { get; set; }

        public Uri GetObjectUri(string key)
            => new Uri(new Uri(RepoUrl), string.Format(ObjectsFormat, key));
        
        public Uri GetTagUri(string key)
            => new Uri(new Uri(RepoUrl), string.Format(TagsFormat, key));
    }
}