using System;
using System.IO;
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
        private readonly RepositoryOptions _repoOptions;
        private readonly IReferenceFactory _referenceFactory;
        private readonly ILogger _log;

        public HttpRefsService(HttpClient client, EndpointOptions options, RepositoryOptions repoOptions,
            IReferenceFactory referenceFactory, ILogger log)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _referenceFactory = referenceFactory ?? throw new ArgumentNullException(nameof(referenceFactory));
            _repoOptions = repoOptions ?? throw new ArgumentNullException(nameof(repoOptions));
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

        public async Task<Reference> Get(string name)
        {
            return await GetInternal(name);
        }

        public async Task<string> GetCommit(string name)
        {
            var reference = await GetInternal(name);
            
            switch (reference)
            {
                case EmptyReference _:
                    throw new InvalidDataException($"Reference {name} is empty");
                
                case CommitReference _:
                    return reference.Value;
                
                case PointerReference _:
                    return await GetCommit(reference.Value);
                
                default:
                    return reference.Value;
            }
        }

        public async Task Store(string name, Reference reference)
        {
            var uri = _options.GetPushRefsUri(name);

            var response = await _client.PostAsync(uri, new StringContent(reference.Value));
            
            _log?.Debug("Take response POST method for uri: {uri} with status code {code}", uri, response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpSendErrorException();
            }
        }

        public async Task<Reference> GetHead()
        {
            return await GetInternal(_repoOptions.HeadFileName);
        }

        public async Task<string> GetHeadCommit()
        {
            return await GetCommit(_repoOptions.HeadFileName);
        }

        public async Task SetHead(Reference value)
        {
            await Store(_repoOptions.HeadFileName, value);
        }

        public Task<string[]> GetAll()
        {
            throw new NotSupportedException("This feature is not yet implemented");
        }

        private async Task<Reference> GetInternal(string name)
        {
            var uri = _options.GetRefsUri(name);
            
            var response = await _client.GetAsync(uri);
            
            _log?.Debug("Take response GET method for uri: {uri} with status code {code}", uri, response.StatusCode);

            return _referenceFactory.Create(await response.Content.ReadAsStringAsync());
        }
    }
}