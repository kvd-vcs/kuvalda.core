using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Kuvalda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;

namespace Kuvalda.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = ConfigureServices(args);
        }

        private static object ConfigureServices(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            
            AddConfiguration(args, serviceCollection);

            serviceCollection.AddTransient<IFileSystem, FileSystem>();
            
            var logger = new Logger()
            
            serviceCollection.AddTransient<IRepositoryInitializeService, RepositoryInitializeService>();
            
            serviceCollection.AddSingleton<IRepositoryFacade, RepositoryFacade>();
            
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