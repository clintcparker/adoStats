using System;

namespace adoStats_core
{
    public class Repository
    {
        public string Name { get; internal set; }
        public string Type { get; internal set; }
        public string OrganizationName { get; internal set; }
        public string ProjectName { get; internal set; }
        public Uri Uri { get; internal set; }
        public string LocalPath { get; internal set; }
    }
}