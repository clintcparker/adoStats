using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adoStats_core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace adoStats_cli
{
    class Program
    {
        // static async Task<int> Main(string configFile, string ADOPAT)
        // {
        //     Console.WriteLine(configFile);
        //     Console.WriteLine(ADOPAT);
        //     return await Task.FromResult(0);
        // }


        static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand();
            rootCommand.AddCommand(new Stats());
            await rootCommand.InvokeAsync(args);//.Wait();
            //return await stats();
        }

        static async Task<int> stats()
        {
            var host = ServiceContainer.host;
            using (var serviceScope = host.Services.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                try
                {
                    var myService = services.GetRequiredService<IAzureDevService>();
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    // var logger = services.GetRequiredService<ILogger<Program>>();

                    // logger.LogError(ex, "An error occurred.");
                }
            }
            return 0;
        }
   
        public class Stats : System.CommandLine.Command
        {

            public Stats() : base("stats", "get them stats")
            {
                Add(new Option<string>("--host"));
                Handler = CommandHandler.Create<string>(GetStats);
            }

            public void GetStats(string host)
            {
                Console.WriteLine($"your host is {host}");
            }
        }
    }
}
