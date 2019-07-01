using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Net.Http;
using Kuvalda.Core;
using Kuvalda.Core.Checkout;
using Kuvalda.Core.Status;
using Kuvalda.FastRsyncNet;
using Kuvalda.Storage.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Kuvalda.Cli
{
    partial class Program
    {
        private const string TEMP_FOLDER_NAME = "temp";
        private const string OBJECTS_FOLDER_NAME = "objects";

        private const string KVD_CONFIG_FILE_NAME = "config.json";
        private const string KVD_ENVIRONMENT_VARIABLE_PREFIX = "KVD_";
        private const string KVD_REPO_SYSTEM_FOLDER_NAME = ".kvd";
        private const string KVD_REPO_HEAD_FILE_NAME = "HEAD";
        private const string KVD_REPO_DEFAULT_BRANCH_NAME = "master";
        private const string KVD_REPO_MESSAGE_LABEL_NAME = "message";
        
        private const string KVD_REPO_DEFAULT_IS_READONLY = "false";
        
        private const string KVD_URL_DEFAULT_HUB = "https://hub.kuvalda.io";
        private const string KVD_URL_DEFAULT_OBJECT_FMT = "/objects/{0}";
        private const string KVD_URL_DEFAULT_TAGS_FMT = "/keys/{0}";

        private readonly IConfiguration _conf;
        private readonly RepositoryOptions _options;
        private readonly ApplicationInstanceSettings _instanceOptions;
        private readonly EndpointOptions _endpointOptions;
        private readonly string[] _args; 

        public Program(string[] args)
        {
            _args = args ?? throw new ArgumentNullException(nameof(args));
            
            _conf = new ConfigurationBuilder()
                .AddInMemoryCollection(GetDefaultConfigurationCollection())
                .AddJsonFile(Path.Combine(Environment.CurrentDirectory, KVD_REPO_SYSTEM_FOLDER_NAME, KVD_CONFIG_FILE_NAME), true)
                .AddEnvironmentVariables(src => src.Prefix = KVD_ENVIRONMENT_VARIABLE_PREFIX)
                .AddCommandLine(src => src.Args = args)
                .Build();

            _options = new RepositoryOptions()
            {
                RepositorySystemFolder = _conf[nameof(RepositoryOptions.RepositorySystemFolder)],
                HeadFilePath = _conf[nameof(RepositoryOptions.HeadFilePath)],
                DefaultBranchName = _conf[nameof(RepositoryOptions.DefaultBranchName)],
                MessageLabel = _conf[nameof(RepositoryOptions.MessageLabel)]
            };

            _instanceOptions = new ApplicationInstanceSettings()
            {
                RepositoryPath = _conf[nameof(ApplicationInstanceSettings.RepositoryPath)],
                IsHttpReadOnly = _conf[nameof(ApplicationInstanceSettings.IsHttpReadOnly)] == "true"
            };

            _endpointOptions = new EndpointOptions()
            {
                RepoUrl = _conf[nameof(EndpointOptions.RepoUrl)],
                ObjectsFormat = _conf[nameof(EndpointOptions.ObjectsFormat)],
                TagsFormat = _conf[nameof(EndpointOptions.TagsFormat)],
            };
        }
        
        private KeyValuePair<string, string>[] GetDefaultConfigurationCollection()
        {
            return new Dictionary<string, string>
            {
                
                [nameof(RepositoryOptions.RepositorySystemFolder)] = KVD_REPO_SYSTEM_FOLDER_NAME,
                [nameof(RepositoryOptions.HeadFilePath)] = KVD_REPO_HEAD_FILE_NAME,
                [nameof(RepositoryOptions.DefaultBranchName)] = KVD_REPO_DEFAULT_BRANCH_NAME,
                [nameof(RepositoryOptions.MessageLabel)] = KVD_REPO_MESSAGE_LABEL_NAME,
                
                [nameof(ApplicationInstanceSettings.RepositoryPath)] = Environment.CurrentDirectory,
                [nameof(ApplicationInstanceSettings.IsHttpReadOnly)] = KVD_REPO_DEFAULT_IS_READONLY,
                
                [nameof(EndpointOptions.RepoUrl)] = KVD_URL_DEFAULT_HUB,
                [nameof(EndpointOptions.ObjectsFormat)] = KVD_URL_DEFAULT_OBJECT_FMT,
                [nameof(EndpointOptions.TagsFormat)] = KVD_URL_DEFAULT_TAGS_FMT
                
            }.ToArray();
        }


        private IServiceProvider ConfigureServices()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            var svc = new ServiceCollection();

            svc.AddSingleton(Log.Logger)
                .AddLogging(builder => builder.AddSerilog());

            AddConfiguration(_args, svc);
            AddTreeServices(svc);
            AddStorageServices(svc);
            AddHashServices(svc);
            AddTopLevelServices(svc);

            InitCommands(svc);

            return svc.BuildServiceProvider();
        }

        private void AddTopLevelServices(ServiceCollection svc)
        {
            svc.AddTransient<IRepositoryInitializeService, RepositoryInitializeService>()
                .AddSingleton<IRepositoryFacade, RepositoryFacade>()
                .AddTransient<ICommitCreateService, CommitCreateService>()
                .AddTransient<ISerializationProvider, JsonSerializationProvider>()
                .AddTransient<ICommitServiceFacade, CommitServiceFacade>()
                .AddTransient<ICommitGetService, CommitGetService>()
                .AddTransient<IStatusService, StatusService>()
                .AddTransient<ILogService, LogService>()
                .AddTransient<IChangesCompressService, FastRsyncChangesCompressService>()
                .AddTransient<IChangesDecompressService, FastRsyncChangesDecompressService>()
                .AddTransient<IRepositoryCompressFacade, RepositoryCompressFacade>()
                .AddTransient<CheckoutService>()
                .AddTransient<ICheckoutService, CheckoutDecompressService>(ctx =>
                {
                    var checkout = ctx.GetRequiredService<CheckoutService>();
                    var refs = ctx.GetRequiredService<IRefsService>();
                    var repoCompressFacade = ctx.GetRequiredService<IRepositoryCompressFacade>();
                    return new CheckoutDecompressService(checkout, refs, repoCompressFacade);
                });
        }

        private void AddHashServices(ServiceCollection svc)
        {
            svc.AddTransient<IHashComputeProvider, SHA2HashComputeProvider>()
                .AddTransient<IHashModificationFactory, HashModificationFactory>()
                .AddTransient<IHashTableCreator, HashTableCreator>()
                .AddTransient<IHashModificationFactory, HashModificationFactory>();
        }

        private void AddStorageServices(ServiceCollection svc)
        {
            svc.AddTransient<IFileSystem, FileSystem>()
                .AddTransient<IEntityObjectStorage<CommitModel>, EntityObjectStorage<CommitModel>>()
                .AddTransient<IEntityObjectStorage<TreeNode>, EntityObjectStorage<TreeNode>>()
                .AddTransient<IEntityObjectStorage<CompressModel>, EntityObjectStorage<CompressModel>>()
                .AddTransient<IEntityObjectStorage<IDictionary<string, string>>,
                    EntityObjectStorage<IDictionary<string, string>>>()
                .AddTransient<ICommitStoreService, CommitStoreService>()
                .AddTransient<ITempObjectStorage, TempObjectStorage>(ctx =>
                {
                    var filesystem = ctx.GetRequiredService<IFileSystem>();
                    var path = Path.Combine(ctx.GetRequiredService<ApplicationInstanceSettings>().RepositoryPath,
                        ctx.GetRequiredService<RepositoryOptions>().RepositorySystemFolder, TEMP_FOLDER_NAME);
                    var logger = ctx.GetRequiredService<ILogger>();
                    return new TempObjectStorage(path, filesystem, logger);
                })
                .AddTransient<IRefsService, RefsService>(ctx =>
                {
                    var filesystem = ctx.GetRequiredService<IFileSystem>();
                    var options = ctx.GetRequiredService<RepositoryOptions>();
                    var path = Path.Combine(ctx.GetRequiredService<ApplicationInstanceSettings>().RepositoryPath,
                        options.RepositorySystemFolder);
                    var logger = ctx.GetRequiredService<ILogger>();
                    return new RefsService(path, filesystem, options, logger);
                })
                .AddTransient<ICompressObjectsStorage, CompressObjectsStorage>();

            if (!_instanceOptions.IsHttpReadOnly)
            {
                svc.AddTransient<IObjectStorage, FileSystemObjectStorage>(ctx =>
                {
                    var filesystem = ctx.GetRequiredService<IFileSystem>();
                    var path = Path.Combine(ctx.GetRequiredService<ApplicationInstanceSettings>().RepositoryPath,
                        ctx.GetRequiredService<RepositoryOptions>().RepositorySystemFolder, OBJECTS_FOLDER_NAME);
                    return new FileSystemObjectStorage(filesystem, path);
                });
                svc.AddTransient<IKeyValueStorage, KeyValueStorage>(ctx =>
                {
                    var filesystem = ctx.GetRequiredService<IFileSystem>();
                    var path = Path.Combine(ctx.GetRequiredService<ApplicationInstanceSettings>().RepositoryPath,
                        ctx.GetRequiredService<RepositoryOptions>().RepositorySystemFolder, "keys");
                    var logger = ctx.GetRequiredService<ILogger>();
                    return new KeyValueStorage(filesystem, path, logger);
                });
            }
            else
            {
                svc.AddTransient(ctx => new HttpClient());
                svc.AddTransient<IObjectStorage, HttpObjectStorage>();
                svc.AddTransient<IKeyValueStorage, HttpKeyValueStorage>();
            }
        }

        private void AddTreeServices(ServiceCollection svc)
        {
            svc.AddTransient<ITreeFilter, TreeFilter>(ctx =>
                {
                    var service = new TreeFilter(ctx.GetRequiredService<IFileSystem>());
                    service.PredefinedIgnores = new List<string>()
                    {
                        ctx.GetRequiredService<RepositoryOptions>().RepositorySystemFolder
                    };
                    return service;
                })
                .AddTransient<TreeCreator>()
                .AddTransient<ITreeCreator, TreeCreatorFiltered>(ctx => new TreeCreatorFiltered(
                    ctx.GetRequiredService<TreeCreator>(), ctx.GetRequiredService<ITreeFilter>()))
                .AddTransient<IDifferenceEntriesCreator, DifferenceEntriesCreator>()
                .AddTransient<IFlatTreeCreator, FlatTreeCreator>()
                .AddTransient<IFlatTreeDiffer, FlatTreeDiffer>();
        }

        private void InitCommands(ServiceCollection svc)
        {
            svc.AddTransient<HelpCommand>()
                .AddTransient<InitCommand>()
                .AddTransient<StatusCommand>()
                .AddTransient<CommitCommand>()
                .AddTransient<LogCommand>()
                .AddTransient<CheckoutCommand>()
                .AddTransient<CompressCommand>()
                .AddTransient<DecompressCommand>();

            svc.AddSingleton<IDictionary<string, ICliCommand>>(ctx => new Dictionary<string, ICliCommand>()
            {
                ["init"] = ctx.GetRequiredService<InitCommand>(),
                ["status"] = ctx.GetRequiredService<StatusCommand>(),
                ["commit"] = ctx.GetRequiredService<CommitCommand>(),
                ["checkout"] = ctx.GetRequiredService<CheckoutCommand>(),
                ["log"] = ctx.GetRequiredService<LogCommand>(),
                ["compress"] = ctx.GetRequiredService<CompressCommand>(),
                ["decompress"] = ctx.GetRequiredService<DecompressCommand>(),
            });

            svc.AddTransient<IStartup, Startup>();
        }

        private void AddConfiguration(string[] args, ServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton(_conf);
            serviceCollection.AddSingleton(_options);
            serviceCollection.AddSingleton(_instanceOptions);
            serviceCollection.AddSingleton(_endpointOptions);
        }
    }
}