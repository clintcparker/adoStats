

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.Graph.Client;

namespace adoStats_core
{
    public interface IAzureDevService
    {
        Task<List<GitCommitRef>> GetLatestCommits(string repositoryId, DateTime date);
        Task<List<GitRepository>> GetRepositories();

        // Task<List<BuildDefinition>> GetBuildDefinitions(GitRepository repository);

        Task<List<Build>> GetBuilds(GitRepository repository, int count, string definitionId = null);

        Task<List<TestRun>> GetTestRunsForBuild(Build build);
        Task<List<GraphUser>> GetUsers();
        Task DeleteUser(GraphUser user);
    }
}