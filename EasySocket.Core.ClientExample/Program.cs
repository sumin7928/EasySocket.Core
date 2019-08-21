using System;
using System.IO;
using EasySocket.Core.Networks.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EasySocket.Core.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string addr = GetArgsStringValue(args, 0, "127.0.0.1");
            int port = GetArgsNumberValue(args, 1, 12000);

            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            ServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton(config)
                .AddLogging(logger =>
                {
                    logger.AddConsole();
                    logger.SetMinimumLevel(LogLevel.Debug);
                })
                .AddTransient<EasyClient>()
                .AddTransient<Startup>()
                .BuildServiceProvider();

            serviceProvider.GetRequiredService<Startup>().Run(addr, port);
        }

        private static int GetArgsNumberValue(String[] args, int index, int defaultValue)
        {
            if (args.Length > index && int.TryParse(args[index], out int value))
            {
                return value;
            }
            else
            {
                return defaultValue;
            }
        }

        private static string GetArgsStringValue(String[] args, int index, string defaultValue)
        {
            if (args.Length > index)
            {
                return args[index];
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
