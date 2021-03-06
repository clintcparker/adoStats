
using System;

namespace adoStats_core
{
    public partial class StatsService
    {
        public class PathException : Exception
        {
            public PathException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}