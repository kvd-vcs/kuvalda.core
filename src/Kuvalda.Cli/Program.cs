using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Kuvalda.Core;
using Kuvalda.Core.Checkout;
using Kuvalda.Core.Status;
using Kuvalda.FastRsyncNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Kuvalda.Cli
{
    class Program
    {
        private const string TEMP_FOLDER_NAME = "temp";
        private const string OBJECTS_FOLDER_NAME = "objects";
        
        private const string KVD_CONFIG_FILE_NAME = ".kvdconfig";
        private const string KVD_ENVIRONMENT_VARIABLE_PREFIX = "KVD_";
        private const string KVD_REPO_SYSTEM_FOLDER_NAME = ".kvd";
        private const string KVD_REPO_HEAD_FILE_NAME = "HEAD";
        private const string KVD_REPO_DEFAULT_BRANCH_NAME = "master";
        private const string KVD_REPO_MESSAGE_LABEL_NAME = "message";
        
        static async Task Main(string[] args)
        {
            await ConfigureServices(args).GetService<IStartup>().Run(args);
        }

        private static IServiceProvider ConfigureServices(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            
            var svc = new ServiceCollection();

            svc.AddSingleton(Log.Logger)
                .AddLogging(builder => builder.AddSerilog());

            AddConfiguration(args, svc);
            AddTreeServices(svc);
            AddStorageServices(svc);
            AddHashServices(svc);
            AddTopLevelServices(svc);

            InitCommands(svc);

            return svc.BuildServiceProvider();
        }

        private static void AddTopLevelServices(ServiceCollection svc)
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

        private static void AddHashServices(ServiceCollection svc)
        {
            svc.AddTransient<IHashComputeProvider, SHA2HashComputeProvider>()
                .AddTransient<IHashModificationFactory, HashModificationFactory>()
                .AddTransient<IHashTableCreator, HashTableCreator>()
                .AddTransient<IHashModificationFactory, HashModificationFactory>();
        }

        private static void AddStorageServices(ServiceCollection svc)
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
                .AddTransient<IObjectStorage, FileSystemObjectStorage>(ctx =>
                {
                    var filesystem = ctx.GetRequiredService<IFileSystem>();
                    var path = Path.Combine(ctx.GetRequiredService<ApplicationInstanceSettings>().RepositoryPath,
                        ctx.GetRequiredService<RepositoryOptions>().RepositorySystemFolder, OBJECTS_FOLDER_NAME);
                    return new FileSystemObjectStorage(filesystem, path);
                })
                .AddTransient<IRefsService, RefsService>(ctx =>
                {
                    var filesystem = ctx.GetRequiredService<IFileSystem>();
                    var options = ctx.GetRequiredService<RepositoryOptions>();
                    var path = Path.Combine(ctx.GetRequiredService<ApplicationInstanceSettings>().RepositoryPath, options.RepositorySystemFolder);
                    var logger = ctx.GetRequiredService<ILogger>();
                    return new RefsService(path, filesystem, options, logger);
                })
                .AddTransient<IKeyValueStorage, KeyValueStorage>(ctx =>
                {
                    var filesystem = ctx.GetRequiredService<IFileSystem>();
                    var path = Path.Combine(ctx.GetRequiredService<ApplicationInstanceSettings>().RepositoryPath,
                        ctx.GetRequiredService<RepositoryOptions>().RepositorySystemFolder, "keys");
                    var logger = ctx.GetRequiredService<ILogger>();
                    return new KeyValueStorage(filesystem, path, logger);
                })
                .AddTransient<ICompressObjectsStorage, CompressObjectsStorage>();
        }

        private static void AddTreeServices(ServiceCollection svc)
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

        private static void InitCommands(ServiceCollection svc)
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

        private static void AddConfiguration(string[] args, ServiceCollection serviceCollection)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(GetDefaultConfigurationCollection())
                .AddJsonFile(Path.Combine(Environment.CurrentDirectory, KVD_CONFIG_FILE_NAME), true)
                .AddEnvironmentVariables(src => src.Prefix = KVD_ENVIRONMENT_VARIABLE_PREFIX)
                .AddCommandLine(src => src.Args = args)
                .Build();

            var options = new RepositoryOptions()
            {
                RepositorySystemFolder = configuration[nameof(RepositoryOptions.RepositorySystemFolder)],
                HeadFilePath = configuration[nameof(RepositoryOptions.HeadFilePath)],
                DefaultBranchName = configuration[nameof(RepositoryOptions.DefaultBranchName)],
                MessageLabel = configuration[nameof(RepositoryOptions.MessageLabel)]
            };

            var instanceOptions = new ApplicationInstanceSettings()
            {
                RepositoryPath = configuration[nameof(ApplicationInstanceSettings.RepositoryPath)]
            };

            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddSingleton(options);
            serviceCollection.AddSingleton(instanceOptions);
        }

        private static KeyValuePair<string, string>[] GetDefaultConfigurationCollection()
        {
            return new Dictionary<string, string>
            {
                [nameof(RepositoryOptions.RepositorySystemFolder)] = KVD_REPO_SYSTEM_FOLDER_NAME,
                [nameof(RepositoryOptions.HeadFilePath)] = KVD_REPO_HEAD_FILE_NAME,
                [nameof(RepositoryOptions.DefaultBranchName)] = KVD_REPO_DEFAULT_BRANCH_NAME,
                [nameof(RepositoryOptions.MessageLabel)] = KVD_REPO_MESSAGE_LABEL_NAME,
                [nameof(ApplicationInstanceSettings.RepositoryPath)] = Environment.CurrentDirectory
            }.ToArray();
        }
    }
}