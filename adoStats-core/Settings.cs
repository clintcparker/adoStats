
using System;
using System.Collections.Generic;

namespace adoStats_core
{
    public class Settings 
{
    public Settings()
    {
        Repositories = new List<RepositoryConfig>();
        Dates = new List<DateTime>();
    }
    public List<RepositoryConfig> Repositories {get;set;}
    public string ADOHost {get;set;}

    public List<DateTime> Dates {get;set;}
        public string Organization { get; internal set; }
    }

}