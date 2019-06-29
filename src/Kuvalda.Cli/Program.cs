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
            
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton(Log.Logger);
            
            serviceCollection.AddTransient<IStartup, Startup>();
            
            AddConfiguration(args, serviceCollection);

            serviceCollection.AddTransient<IFileSystem, FileSystem>()
                .AddLogging(builder => builder.AddSerilog())
                .AddTransient<IRepositoryInitializeService, RepositoryInitializeService>()
                .AddSingleton<IRepositoryFacade, RepositoryFacade>()
                .AddTransient<ICommitCreateService, CommitCreateService>()
                .AddTransient<IEntityObjectStorage<CommitModel>, EntityObjectStorage<CommitModel>>()
                .AddTransient<IEntityObjectStorage<TreeNode>, EntityObjectStorage<TreeNode>>()
                .AddTransient<IEntityObjectStorage<CompressModel>, EntityObjectStorage<CompressModel>>()
                .AddTransient<IEntityObjectStorage<IDictionary<string, string>>,
                    EntityObjectStorage<IDictionary<string, string>>>()
                .AddTransient<IObjectStorage, FileSystemObjectStorage>(ctx =>
                {
                    var filesystem = ctx.GetRequiredService<IFileSystem>();
                    var path = Path.Combine(ctx.GetRequiredService<ApplicationInstanceSettings>().RepositoryPath,
                        ctx.GetRequiredService<RepositoryOptions>().RepositorySystemFolder);
                    return new FileSystemObjectStorage(filesystem, path + "/objects");
                })
                .AddTransient<ISerializationProvider, JsonSerializationProvider>()
                .AddTransient<IHashComputeProvider, SHA2HashComputeProvider>()
                .AddTransient<ITreeFilter, TreeFilter>(ctx =>
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
                .AddTransient<IHashModificationFactory, HashModificationFactory>()
                .AddTransient<IDifferenceEntriesCreator, DifferenceEntriesCreator>()
                .AddTransient<IFlatTreeCreator, FlatTreeCreator>()
                .AddTransient<IFlatTreeDiffer, FlatTreeDiffer>()
                .AddTransient<ICommitStoreService, CommitStoreService>()
                .AddTransient<IRefsService, RefsService>(ctx =>
                {
                    var filesystem = ctx.GetRequiredService<IFileSystem>();
                    var path = Path.Combine(ctx.GetRequiredService<ApplicationInstanceSettings>().RepositoryPath,
                        ctx.GetRequiredService<RepositoryOptions>().RepositorySystemFolder);
                    return new RefsService(path, filesystem, ctx.GetRequiredService<RepositoryOptions>());
                })
                .AddTransient<ICommitServiceFacade, CommitServiceFacade>()
                .AddTransient<ICommitGetService, CommitGetService>()
                .AddTransient<ICheckoutService, CheckoutService>()
                .AddTransient<IStatusService, StatusService>()
                .AddTransient<IHashTableCreator, HashTableCreator>()
                .AddTransient<ILogService, LogService>()
                .AddTransient<IChangesCompressService, FastRsyncChangesCompressService>()
                .AddTransient<IChangesDecompressService, FastRsyncChangesDecompressService>()
                .AddTransient<ITempObjectStorage, TempObjectStorage>(ctx =>
                {
                    var filesystem = ctx.GetRequiredService<IFileSystem>();
                    var path = Path.Combine(ctx.GetRequiredService<ApplicationInstanceSettings>().RepositoryPath,
                        ctx.GetRequiredService<RepositoryOptions>().RepositorySystemFolder, "temp");
                    return new TempObjectStorage(path, filesystem);
                });

            InitCommands(serviceCollection);

            return serviceCollection.BuildServiceProvider();
        }

        private static void InitCommands(ServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<HelpCommand>()
                .AddTransient<InitCommand>()
                .AddTransient<StatusCommand>()
                .AddTransient<CommitCommand>()
                .AddTransient<LogCommand>()
                .AddTransient<CheckoutCommand>()
                .AddTransient<CompressCommand>()
                .AddTransient<DecompressCommand>();
            
            serviceCollection.AddSingleton<IDictionary<string, ICliCommand>>(ctx => new Dictionary<string, ICliCommand>()
            {
                ["help"] = ctx.GetRequiredService<HelpCommand>(),
                ["init"] = ctx.GetRequiredService<InitCommand>(),
                ["status"] = ctx.GetRequiredService<StatusCommand>(),
                ["commit"] = ctx.GetRequiredService<CommitCommand>(),
                ["checkout"] = ctx.GetRequiredService<CheckoutCommand>(),
                ["log"] = ctx.GetRequiredService<LogCommand>(),
                ["compress"] = ctx.GetRequiredService<CompressCommand>(),
                ["decompress"] = ctx.GetRequiredService<DecompressCommand>(),
            });
        }

        private static void AddConfiguration(string[] args, ServiceCollection serviceCollection)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(GetDefaultConfigurationCollection())
                .AddJsonFile(Path.Combine(Environment.CurrentDirectory, ".kvdconfig"), true)
                .AddEnvironmentVariables(src => src.Prefix = "KVD_")
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
                [nameof(RepositoryOptions.RepositorySystemFolder)] = ".kvd",
                [nameof(RepositoryOptions.HeadFilePath)] = "HEAD",
                [nameof(RepositoryOptions.DefaultBranchName)] = "master",
                [nameof(RepositoryOptions.MessageLabel)] = "message",
                [nameof(ApplicationInstanceSettings.RepositoryPath)] = Environment.CurrentDirectory
            }.ToArray();
        }
    }
}