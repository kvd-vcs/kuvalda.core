using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Kuvalda.Core;
using Serilog;

namespace Kuvalda.Storage.Http
{
    public class HttpObjectStorage : IObjectStorage
    {
        private readonly HttpClient _client;
        private readonly EndpointOptions _options;
        private readonly ILogger _log;
        

        public HttpObjectStorage(HttpClient client, ILogger log, EndpointOptions options)
        {
            _log = log;
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<bool> Exist(string key)
        {
            var splitKey = $"{key.Substring(0, 2)}/{key.Substring(2)}";
            var uri = _options.GetObjectUri(splitKey);
            var request = new HttpRequestMessage(HttpMethod.Head, uri);

            _log?.Debug("Request http HEAD method for uri: {uri}", uri);
            
            var response = await _client.SendAsync(request);
            
            _log?.Debug("Take response HEAD method for uri: {uri} with status code {code}", uri, response.StatusCode);

            return response.IsSuccessStatusCode;
        }

        public async Task<Stream> Get(string key)
        {
            var splitKey = $"{key.Substring(0, 2)}/{key.Substring(2)}";
            var uri = _options.GetObjectUri(splitKey);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            
            _log?.Debug("Request http GET method for uri: {uri}", uri);
            
            var response = await _client.SendAsync(request);
            
            _log?.Debug("Take response GET method for uri: {uri} with status code {code}", uri, response.StatusCode);
            
            return await response.Content.ReadAsStreamAsync();
        }

        public Task Set(string key, Stream obj)
        {
            throw new NotImplementedException("Write by http not allowed. Protocol is readonly");
        }
    }
}