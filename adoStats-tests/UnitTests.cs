using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using adoStats_core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace adoStats_tests
{
    [TestClass]
    public class UnitTests
    {
        public UnitTests()
        {
            var host = ServiceContainer.host;
            var serviceScope = host.Services.CreateScope();
            var services = serviceScope.ServiceProvider;
            var settings = new adoStats_core.Settings{ADOHost="host",Repositories = new List<RepositoryConfig>{new RepositoryConfig{BuildUrl="url",GitUrl="git"}},Organization="blah"};
            ss = services.GetRequiredService<StatsServiceFactory>().Create(settings);

        }
        private readonly StatsService ss;
        private readonly AzureDevWebUtilities azureDevWeb = new AzureDevWebUtilities();

        [TestMethod]
        public void PATShouldBeFindable()
        {
            var p = azureDevWeb.GetADOPAT();
            Assert.IsNotNull(p);
        }

        [TestMethod]
        public void HostShouldBeFindable()
        {
            var p = azureDevWeb.GetADOHost();
            Assert.IsNotNull(p);
        }

       
        [TestMethod]
        public async Task TempFileCreated()
        {
            var actual = new Settings{ADOHost="host",Repositories = new List<RepositoryConfig>{new RepositoryConfig{BuildUrl="url",GitUrl="git"}}};
            var tempFileName = await ss.CreateTempSettingsFile(actual);
            var f = new FileInfo(tempFileName);
            Settings expected;
            using (FileStream fs = File.OpenRead(tempFileName))
            {
                expected = await JsonSerializer.DeserializeAsync<Settings>(fs);
            }
            Assert.AreEqual(actual.ADOHost,expected.ADOHost);
            Assert.AreEqual(actual.Repositories[0].GitUrl,expected.Repositories[0].GitUrl);
            Assert.That.ComplexAreEqual<Settings>(expected,actual);
            Console.WriteLine($"Settings file: {tempFileName}");
            // https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to
            // https://stackoverflow.com/questions/13297563/read-and-parse-a-json-file-in-c-sharp

            // https://www.daveoncsharp.com/2009/09/how-to-use-temporary-files-in-csharp/

        }

        [TestMethod]
        public void RepoParsesCorrectly()
        {
            var gitUrl= "https://dev.azure.com/orgname/projname/_git/repo.name";
            adoStats_core.Repository r = ss.ParseRepositoryUrl(gitUrl);
            Assert.AreEqual("repo.name", r.Name);
            Assert.AreEqual("Git",r.Type);
            Assert.AreEqual("orgname", r.OrganizationName);
            Assert.AreEqual("project",r.ProjectName);
        }

        [TestMethod]
        public void BuildParsesCorrectly()
        {
            var buildUrl= "https://dev.azure.com/orgname/projname/_build?definitionId=1966";
            adoStats_core.BuildDefinition b = ss.ParseBuildDefUrl(buildUrl);
            Assert.AreEqual(1966, b.Id);
            Assert.AreEqual("orgname", b.OrganizationName);
            Assert.AreEqual("project",b.ProjectName);
        }


        [TestMethod]
        public void GetCorrectNameForRepository()
        {
            var gitUrl= "https://dev.azure.com/orgname/projname/_git/repo.name";
            var expected = "repo.name";
            var name = ss.ParseRepositoryUrl(gitUrl).Name;
            Assert.AreEqual(expected, name);
        }
    }
}
