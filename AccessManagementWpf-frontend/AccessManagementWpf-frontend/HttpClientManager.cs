using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AccessManagementWpf_frontend
{
    public static class HttpClientManager
    {
        private static readonly CookieContainer _cookieContainer = new CookieContainer();


        private static readonly HttpClientHandler _handler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer,
            UseCookies = true
        };


        private static readonly HttpClient _client = new HttpClient(_handler);


        public static HttpClient Client => _client;
    }
}
