using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using adoStats_core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.CommandLine;

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


        static async Task<int> Main(string[] args)
        {
            return await stats();
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
                    var repos = (await myService.GetRepositories()).FindAll(x=>x.Name.Contains("Payments"));
                    List<GitRepository> reposToRemove = new List<GitRepository>();
                    foreach (var repo in repos)
                    {
                        //Console.WriteLine(repo.Name);
                        //var c = (await myService.GetLatestCommits(repo.Id.ToString(),DateTime.Parse("5/24/2020")))[0];
                            
                        //var defs = await myService.GetBuildDefinitions(repo);
                        //BuildDefinition def;
                        // if (defs.Count == 0)
                        // {
                        //     Console.WriteLine($"No definition found for {repo.Name}");
                        //     reposToRemove.Add(repo);
                        //     continue;
                        // }
                        //var gated = defs.Find(d=>d.Name.Contains("Gated"));
                        //var sonar = defs.Find(d=>d.Name.Contains("Sonar"));
                        //def = gated ?? sonar ?? defs[0];
                        //Console.WriteLine($"{def.Name}\t{def.Id}\t{def.Uri}");
                        //var builds = (await myService.GetBuilds(repo,50,def.Id.ToString()));
                        //var matchingBuilds = builds.FindAll(b=>b.SourceVersion==c.CommitId);
                        
                        // Console.WriteLine($"{c.CommitId}\t\t\t{c.Author.Name}\t\t{c.Comment}");
                            
                        // var b = matchingBuilds.Find(b=>b.Definition.Name.Contains("Gated")) ?? matchingBuilds.Find(b=>b.Definition.Name.Contains("Sonar")) ?? builds.Find(b=>b.Result.ToString().Contains("Succeeded"));

                        // if (b.SourceVersion == c.CommitId) 
                        // {
                        //     Console.WriteLine("-------------------- MATCHING BUILD FOUND --------------------");
                            
                        // }
                        // Console.WriteLine($"{b.Definition.Name}\t\t{b.Result}\t\t{b.SourceVersion}\t\t{b.SourceBranch}");
                        // if(b.Definition.Name.Contains("Gated"))
                        // {
                        //     Console.WriteLine("-------------------- GATED BUILD FOUND --------------------");
                            
                        // }
                        // var testRuns = await myService.GetTestRunsForBuild(b);

                        // foreach (TestRun tr in testRuns)
                        // {
                        //     Console.WriteLine($"{tr.PassedTests} / {tr.TotalTests} Tests Passed");
                        // }
                    }
                    foreach( var r in reposToRemove){
                        repos.Remove(r);
                    }
                    Console.WriteLine($"{repos.Count} valid repos found");
                        // Console.WriteLine(pageContent.Substring(0, 500));
                    //}
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
    }
}
