using System;
using System.Text.RegularExpressions;

namespace AgilefantTimes.API.Restful
{
    public class RestfulUrlHandler
    {
        private readonly Regex _urlRegex;
        private readonly Action<HttpRequestProcessor, string[]> _callback;

        public RestfulUrlHandler(string urlRegex, Action<HttpRequestProcessor, string[]> callback)
        {
            _urlRegex = new Regex("^" + urlRegex + "$", RegexOptions.Compiled);
            _callback = callback;
        }

        public bool Execute(HttpRequestProcessor requestProcessor)
        {
            if (!_urlRegex.IsMatch(requestProcessor.HttpUrl)) return false;
            var spl = requestProcessor.HttpUrl.Split(new []{"/"}, StringSplitOptions.RemoveEmptyEntries);
            _callback.Invoke(requestProcessor, spl);
            return true;
        }
    }
}