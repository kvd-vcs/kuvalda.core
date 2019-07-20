using System;
using System.Net.Http;
using System.Threading.Tasks;
using Kuvalda.Core;
using Serilog;

namespace Kuvalda.Storage.Http
{
    public class HttpRefsService : IRemoteRefsService
    {
        private readonly HttpClient _client;
        private readonly EndpointOptions _options;
        private readonly ILogger _log;

        public HttpRefsService(HttpClient client, EndpointOptions options, ILogger log)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _log = log;
        }

        public async Task<bool> Exists(string name)
        {
            var uri = _options.GetRefsUri(name);
            var request = new HttpRequestMessage(HttpMethod.Head, uri);

            _log?.Debug("Request http HEAD method for uri: {uri}", uri);
            
            var response = await _client.SendAsync(request);
            
            _log?.Debug("Take response HEAD method for uri: {uri} with status code {code}", uri, response.StatusCode);

            return response.IsSuccessStatusCode;
        }

        public Task<Reference> Get(string name)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetCommit(string name)
        {
            throw new NotImplementedException();
        }

        public Task Store(string name, Reference reference)
        {
            throw new NotImplementedException();
        }

        public Task<Reference> GetHead()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetHeadCommit()
        {
            throw new NotImplementedException();
        }

        public Task SetHead(Reference value)
        {
            throw new NotImplementedException();
        }

        public Task<string[]> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}