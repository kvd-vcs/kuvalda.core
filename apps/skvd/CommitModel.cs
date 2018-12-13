using System.Collections.Generic;
using Newtonsoft.Json;

namespace SimpleKvd.CLI
{
    public class CommitModel
    {
        [JsonProperty(Order = 0)]
        public string TreeHash;
        
        [JsonProperty(Order = 1)]
        public string HashesAddress;
        
        [JsonProperty(Order = 2)]
        public IDictionary<string, string> Labels;
        
        [JsonProperty(Order = 3)]
        public ICollection<string> Parents;
    }
}