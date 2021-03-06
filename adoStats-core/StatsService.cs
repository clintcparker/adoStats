
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LibGit2Sharp;
using System.Web;
using System.Linq;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using CloneOptions = LibGit2Sharp.CloneOptions;
using System.Net.Http;
using Polly;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Collections.Generic;

namespace adoStats_core
{
    public partial class StatsService
    {
        public StatsService(IAzureDevWebService _azureDevWebService, AzureDevWebUtilities _azureDevWebUtilities)
        {
            azureDevWebService = _azureDevWebService;
            azureDevWebUtils = _azureDevWebUtilities;
            var appFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            AppDir = Path.Combine(appFolder, Constants.LocalAppName);
            GitDir = Path.Combine(AppDir, "git");
            Branches = new List<string>{"Branches","Blocks","Method","Instruction"};
            Lines = new List<string>{"Lines","Line"};
        }
        
        public void Init(Settings settings)
        {
            adoConn = new VssConnection(new Uri(Path.Combine(Constants.ADOUriHost, settings.Organization)), new VssBasicCredential(string.Empty, azureDevWebUtils.GetADOPAT()));
        }
        
        public readonly string AppDir;
        public readonly string GitDir;

        private VssConnection adoConn { get; set; }
        private readonly AzureDevWebUtilities azureDevWebUtils;
        private IAzureDevWebService azureDevWebService { get; }
        private List<string> Branches {get;}
        private List<string> Lines {get;}


        public async Task<StatsModel> GetStats(string settingsFile)
        {
            var sm = new StatsModel();
            //read file
            Settings settings;
            using (FileStream fs = File.OpenRead(settingsFile))
            {
                settings = await JsonSerializer.DeserializeAsync<Settings>(fs);
            }
            var tasks = new List<Task<RepoStats>>();

            //check for path
            foreach (var currentRepo in settings.Repositories)
            {
                var r = ParseRepositoryUrl(currentRepo.GitUrl);

                GetLocalRepoPath(r);
                foreach(var date in settings.Dates)
                {
                    tasks.Add(GetRepoStatsForDate(r,date,currentRepo.BuildUrl, sm));
                    //sm.Repos.Add(rs);
;                }


            }
            await Task.WhenAll(tasks);
            return sm;
        }

        public async Task<RepoStats> GetRepoStatsForDate(adoStats_core.Repository r, DateTime date, string buildUrl,StatsModel sm)
        {
            var rs = new RepoStats();
            try {
                    rs.Name = r.Name;
                    rs.Organization = r.OrganizationName;
                    rs.RepoProject = r.ProjectName;
                    rs.Date = date;
                    rs.Uri = r.Uri.ToString();
                    sm.Repos.Add(rs);
                    var hash = await GetLatestHashForDate(r, date);
                    rs.Hash = hash;
                    Build buildRef = await GetBuildForHash(buildUrl, hash);
                    rs.BuildDefinitionId = buildRef.Definition.Id.ToString();
                    rs.BuildDefinitionUrl = buildRef.Definition.Url;
                    rs.BuildName = buildRef.Definition.Name;
                    rs.BuildProject = buildRef.Project.Name;
                    rs.BuildUrl = buildRef.Url;
                    var testRuns = await GetTestRunInformation(buildRef);
                    foreach(var tr in testRuns){
                        rs.TestsPassing += tr.PassedTests;
                        var theseTestsSkipped = tr.NotApplicableTests+tr.UnanalyzedTests;
                        rs.TestsSkipped += theseTestsSkipped;
                        rs.TestsFailing += tr.TotalTests - tr.PassedTests - theseTestsSkipped;
                    }
                    var covInfo = await GetBuildCoverage(buildRef);
                    foreach(var coverageData in covInfo.CoverageData){
                        foreach (var coverageStat in coverageData.CoverageStats)
                        {
                            if(Lines.Contains(coverageStat.Label)){
                                rs.LinesCovered += coverageStat.Covered;
                                rs.LinesTotal += coverageStat.Total;
                            }
                            else if(Branches.Contains(coverageStat.Label)){
                                rs.BranchesCovered+= coverageStat.Covered;
                                rs.BranchesTotal += coverageStat.Total;
                            }else {
                                Console.WriteLine($"Not logging coverage type `{coverageStat.Label}`");
                            }
                        }
                    }
                    var clocData = await GetCLOCJson(r,hash);
                    rs.CLOCSum = clocData;
                    return rs;
            } catch (Exception e)
            {
                Console.Error.WriteLine($"Problem with {r.Name} for {date.ToShortDateString()}");
                Console.Error.WriteLine(e.Message);
                Console.WriteLine(String.Empty);
                return rs;
            }
        }
        public async Task<CLOCSum> GetCLOCJson(Repository repo, string hash)
        {
            await CheckoutHashLocally(repo, hash);
            try {
                var clocCommand = BuildCLOCCommand(repo);
                var json = clocCommand.Bash();
                var clocSum = JsonSerializer.Deserialize<CLOCJson>(json).SUM;
                return clocSum;
            } catch (Exception e)
            {
                throw new PathException("Make sure that `cloc` is added to your PATH environment variable",e);
            } finally {
                CheckoutMain(repo);
            }
        }

        private void CheckoutMain(Repository repo)
        {
            var command = $"git -C {repo.LocalPath} switch -";
            command.Bash();
        }

        private async Task CheckoutHashLocally(Repository repo, string hash)
        {
            await Task.Run(()=>{
                var command = $"git -C {repo.LocalPath} pull; git -C {repo.LocalPath} checkout {hash}";
                command.Bash();
            });
        }

        public string BuildCLOCCommand(Repository repository)
        {
            return $"cloc {repository.LocalPath} --json";
        }
        
        internal async Task<List<TestRun>> GetTestRunInformation(Build build)
        {

            var tc = adoConn.GetClient<TestManagementHttpClient>();
            var bc = adoConn.GetClient<BuildHttpClient>();
            //var coverageInfo = await tc.GetBuildCodeCoverageAsync(build.Project.Name, build.Id, -1);
            var testRuns = await tc.GetTestRunsAsync(build.Project.Id, build.Uri.ToString());
            return testRuns;
        }

        internal async Task<CodeCoverageSummary> GetBuildCoverage(Build build){
            var tc = adoConn.GetClient<TestManagementHttpClient>();
            var uri = tc.HttpClient.BaseAddress;
            var path = Path.Combine(tc.HttpClient.BaseAddress.AbsoluteUri.ToString(),build.Project.Name,Constants.COVERAGE_API_PATH);
            var requestUri = new Uri(path);
            requestUri = requestUri.AppendQuery("buildId",build.Id.ToString());
            var resp = await tc.HttpClient.GetAsync(requestUri);
            var cov = await resp.Content.ReadAsAsync<CodeCoverageSummary>();
            return cov;
        }

        public async Task<Build> GetBuildForHash(string buildUrl, string hash)
        {
             var localDef = ParseBuildDefUrl(buildUrl);
            var pcc = adoConn.GetClient<ProjectCollectionHttpClient>();
            var projectName = localDef.ProjectName;
            var pc = adoConn.GetClient<ProjectHttpClient>();
            var proj = await pc.GetProject(projectName);
            var bc = adoConn.GetClient<BuildHttpClient>();
            var def = await bc.GetDefinitionAsync(projectName, localDef.Id);
            //look for a good build first. if its not there then queue a new one.
            var builds = await bc.GetBuildsAsync2(projectName, Enumerable.Repeat(def.Id, 1),top:1);
            try
            {
                var cachedBuild = builds?.First(b => b.SourceVersion == hash && b.Definition.Id == localDef.Id && b.Result == BuildResult.Succeeded);
                if (cachedBuild != null)
                {
                    return cachedBuild;
                }
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("No cached build found for {0}", def.Name);
            }
            var b=await QueueBuildForHash( buildUrl,  hash);
            return await GetBuildCompletion(b);
        }
        public async Task<Build> GetBuildCompletion(Build b, bool retry = true)
        {
            if (retry)
            {
                return (await Policy
                    .HandleResult<Build>(b => b.Status == BuildStatus.InProgress || b.Status == BuildStatus.NotStarted) 
                    .WaitAndRetryAsync(15,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                        (exception, retry, timespan) =>
                        {
                            Console.WriteLine($"{b.Definition.Name} {exception.Result.Status}");
                        }
                    ).ExecuteAndCaptureAsync(
                            () => GetBuildAsync(b)
                    )).Result;
            }
            else
            {
                return await GetBuildAsync(b);
            }
        }

        public async Task<Build> GetBuildAsync(Build b)
        {
            var bc = adoConn.GetClient<BuildHttpClient>();
            var build = await bc.GetBuildAsync(b.Project.Id,b.Id);
            return build;
        }
        

        internal async Task<Build> QueueBuildForHash(string buildUrl, string hash)
        {
            var localDef = ParseBuildDefUrl(buildUrl);
            var pcc = adoConn.GetClient<ProjectCollectionHttpClient>();
            var projectName = localDef.ProjectName;
            var pc = adoConn.GetClient<ProjectHttpClient>();
            var proj = await pc.GetProject(projectName);
            var bc = adoConn.GetClient<BuildHttpClient>();
            var def = await bc.GetDefinitionAsync(projectName, localDef.Id);
            //look for a good build first. if its not there then queue a new one.
            var builds = await bc.GetBuildsAsync2(projectName, Enumerable.Repeat(def.Id, 1));
            try
            {
                var cachedBuild = builds?.First(b => b.SourceVersion == hash && b.Definition.Id == localDef.Id && b.Result == BuildResult.Succeeded);
                if (cachedBuild != null)
                {
                    return cachedBuild;
                }
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("No cached build found for {0}", def.Name);
            }

            var b = new Build();
            b.Project = proj;
            b.Definition = def;
            b.SourceVersion = hash;
            b.SourceBranch = "main";
            var activeBuild = await bc.QueueBuildAsync(b,true);
            return activeBuild;
        }


        internal string GetLocalRepoPath(Repository repository)
        {
            var co = new CloneOptions();
            co.CredentialsProvider = CredentialsProvider;
            string workingDir = repository.LocalPath;
            try
            {
                LibGit2Sharp.Repository.Clone(repository.Uri.ToString(), workingDir, co);
            }
            catch (NameConflictException)
            {
                Console.WriteLine("Already cloned");
            }
            return workingDir;
        }
        internal string GetLocalRepoPath(string gitUrl)
        {
            var repository = ParseRepositoryUrl(gitUrl);
            return GetLocalRepoPath(repository);
        }

        internal adoStats_core.Repository ParseRepositoryUrl(string gitUrl)
        {
            var u = new Uri(gitUrl);
            var on = u.Segments[1].Replace("/", "");
            var pn = u.Segments[2].Replace("/", "");
            var type = u.Segments[3].Replace("/", "") == "_git" ? "Git" : "unknown";
            var n = u.Segments[4].Replace("/", "");
            var r = new Repository
            {
                Name = n,
                OrganizationName = on,
                ProjectName = pn,
                Type = type,
                Uri = u,
                LocalPath = Path.Combine(GitDir, n)
            };
            return r;
        }

        internal adoStats_core.BuildDefinition ParseBuildDefUrl(string buildUrl)
        {
            var u = new Uri(buildUrl);
            var on = u.Segments[1].Replace("/", "");
            var pn = u.Segments[2].Replace("/", "");
            var id = Int32.Parse(HttpUtility.ParseQueryString(u.Query)["definitionId"]);
            var r = new BuildDefinition
            {
                OrganizationName = on,
                ProjectName = pn,
                Id = id
            };
            return r;
        }

        internal async Task<string> CreateTempSettingsFile(Settings settings)
        {
            string fileName = Utilities.CreateTempFile();

            using (FileStream fs = File.Open(fileName, FileMode.Append))
            {
                await JsonSerializer.SerializeAsync(fs, settings);
            }
            return fileName;
        }

        internal async Task<string> GetLatestHashForDate(Repository r, DateTime d)
        {
            var sc = new GitQueryCommitsCriteria();
            var IV = new GitVersionDescriptor();
            IV.VersionOptions = GitVersionOptions.None;
            IV.VersionType = GitVersionType.Branch;
            IV.Version = "main";
            sc.ItemVersion = IV;
            sc.ToDate = d.ToShortDateString();
            sc.Top = 1;
            var gc = adoConn.GetClient<Microsoft.TeamFoundation.SourceControl.WebApi.GitHttpClient>();
            var gitRepositories = await gc.GetRepositoriesAsync(r.ProjectName);
            var gr = gitRepositories.First(x=>x.WebUrl == r.Uri.ToString());
            var commits = await  gc.GetCommitsAsync(r.ProjectName,gr.Id,sc);

            var comm =  commits.FirstOrDefault();
            return comm.CommitId;
        }


        private Credentials CredentialsProvider(string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            var _credentials = new UsernamePasswordCredentials
            {
                Username = "bob",
                Password = azureDevWebUtils.GetADOPAT()
            };
            return _credentials;
        }
    }
}