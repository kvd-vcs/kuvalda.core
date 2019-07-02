using System.IO.Abstractions;
using System.Threading.Tasks;
using Serilog;

namespace Kuvalda.Core
{
    public class RepositoryInitializeService : IRepositoryInitializeService
    {
        private readonly RepositoryOptions _options;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;
        private readonly ICommitCreateService _commitCreateService;
        private readonly ICommitStoreService _commitStoreService;
        private readonly IRefsService _refsService;

        public RepositoryInitializeService(RepositoryOptions options, IFileSystem fileSystem, ILogger logger,
            ICommitCreateService commitCreateService, ICommitStoreService commitStoreService, IRefsService refsService)
        {
            _options = options;
            _fileSystem = fileSystem;
            _logger = logger;
            _commitCreateService = commitCreateService;
            _commitStoreService = commitStoreService;
            _refsService = refsService;
        }

        public bool IsInitialized(string path)
        {
            return _fileSystem.File.Exists(_options.SystemFolderPath);
        }
        
        public async Task Initialize(string path)
        {
            if (_fileSystem.Directory.Exists(_options.SystemFolderPath))
            {
                _logger.Warning("Try initialize alredy initialized repository at path {RepositoryPath}", path);
                return;
            }

            _fileSystem.Directory.CreateDirectory(_options.SystemFolderPath);
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(_options.SystemFolderPath, "refs"));
            _fileSystem.File.Create(_fileSystem.Path.Combine(_options.SystemFolderPath, "refs", _options.HeadFileName)).Close();
            
            _logger.Information("Create repository system folder at {RepositoryPath}", _options.SystemFolderPath);

            var initCommit = await _commitCreateService.CreateCommit();
            var initChash = await _commitStoreService.StoreCommit(initCommit);
            
            _logger.Information("Create init commit with chash {InitChash}", initChash);

            _refsService.Store(_options.DefaultBranchName, new CommitReference(initChash));
            _refsService.SetHead(new PointerReference(_options.DefaultBranchName));
            
            _logger.Information("Write init chash to default ref with name {DefaultBranchName}", _options.DefaultBranchName);
        }
    }
}