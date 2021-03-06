using System;
using System.Collections.Generic;

namespace adoStats_core
{
    public class StatsModel
    {
        public StatsModel()
        {
            Repos = new List<RepoStats>();
        }
        public List<RepoStats> Repos {get;private set;}

    }

    public class RepoStats
    {
        public RepoStats()
        {
            TestsFailing =
            TestsPassing =
            TestsSkipped =
            LinesTotal = 
            LinesCovered = 
            BranchesCovered = 
            BranchesTotal = 0; 
        }
        public string Name {get;set;}
        public DateTime Date {get;set;}
        public string Uri {get;set;}
        public string Organization {get;set;}
        public string RepoProject {get;set;}
        public string BuildProject {get;set;}

        public string BuildName {get;set;}
        public string BuildDefinitionId {get;set;}
        public string BuildDefinitionUrl {get;set;}
        public string BuildUrl {get;set;}
        public int TestsPassing {get;set;}
        public int TestsFailing {get;set;}
        public int TestsSkipped {get;set;}
        public int LinesCovered {get;set;}
        public int LinesTotal {get;set;}
        public int BranchesCovered {get;set;}
        public int BranchesTotal {get;set;}
        public string Hash { get; internal set; }
        public CLOCSum CLOCSum {get;set;}
    }

}