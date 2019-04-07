using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Kuvalda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Kuvalda.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigureServices(args).GetService<IStartup>().Run(args);
        }

        private static IServiceProvider ConfigureServices(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
            
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient<IStartup, Startup>();
            
            AddConfiguration(args, serviceCollection);

            serviceCollection.AddTransient<IFileSystem, FileSystem>()
                .AddLogging(builder => builder.AddSerilog())
                .AddTransient<IRepositoryInitializeService, RepositoryInitializeService>()
                .AddSingleton<IRepositoryFacade, RepositoryFacade>();
            
            return serviceCollection.BuildServiceProvider();
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

            serviceCollection.AddSingleton<IConfiguration>(configuration);
            serviceCollection.AddSingleton(options);
        }

        private static KeyValuePair<string, string>[] GetDefaultConfigurationCollection()
        {
            return new Dictionary<string, string>
            {
                [nameof(RepositoryOptions.RepositorySystemFolder)] = ".kvd",
                [nameof(RepositoryOptions.HeadFilePath)] = "HEAD",
                [nameof(RepositoryOptions.DefaultBranchName)] = "master",
                [nameof(RepositoryOptions.MessageLabel)] = "message",
            }.ToArray();
        }
    }
}