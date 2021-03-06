
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

[assembly: InternalsVisibleToAttribute("adoStats-tests")]

namespace adoStats_core
{
    public class AzureDevWebUtilities
    {
        public string GetADOHost()
        {
            const string hostVar = "ADO__HOST";
            var c_host = StripProtocolAndCollection(GetVarFromEnvOrScratch(hostVar));
            return c_host;
        }

        public string GetVarFromEnvOrScratch(string var)
        {
            var val = Environment.GetEnvironmentVariable(var);
            if (String.IsNullOrWhiteSpace(val))
            {
                //"/Users/clint.parker/Developer/github/adoStats/adoStats-tests/bin/Debug/netcoreapp3.1"
                string nextDirectory = Directory.GetCurrentDirectory();
                string projectRoot = "adoStats";
                while (Path.GetFileName(nextDirectory) != projectRoot && Path.GetDirectoryName(Directory.GetParent(nextDirectory).FullName) != Path.GetDirectoryName(nextDirectory))
                {
                    nextDirectory = Directory.GetParent(nextDirectory).FullName;
                }
                var jsonFile = Path.Combine(nextDirectory, "adoStats-core", "scratch", "scratch.json");

                using (JsonTextReader reader = new JsonTextReader(File.OpenText(jsonFile)))
                {
                    while (reader.Read())
                    {
                        if (reader.Value != null)
                        {
                            if (reader.Path == var && reader.TokenType == JsonToken.String)
                            {
                                val = reader.Value.ToString();
                            }
                        }
                    }
                }
            }

            return val;
        }


        public string StripProtocolAndCollection(string rawHost)
        {
            Uri rawHostUri;
            if (!Uri.TryCreate(rawHost,UriKind.Absolute,out rawHostUri))
            {
                rawHostUri = new Uri($"https://{rawHost}");
            } 

            return rawHostUri.Host;
        }


        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public string GetADOPAT()
        {
            var c_pat = GetVarFromEnvOrScratch("ADO__PAT");
            return c_pat;
        }
    }



        internal class ADOArrayResponse<T>
        {
            public int count { get; set; }

            public List<T> value { get; set; }
        }

}