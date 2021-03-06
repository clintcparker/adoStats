using System.Net.Http;
using System.Threading.Tasks;

namespace adoStats_core{
    
    public interface IAzureDevWebService
    {
        public Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request, bool retry = true);
        public HttpRequestMessage CreateDefaultRequest();
    }

}