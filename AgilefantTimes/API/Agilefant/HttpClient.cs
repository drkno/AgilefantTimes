using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AgilefantTimes.API.Agilefant
{
    public class HttpClient
    {
        private readonly HttpClientHandler _clientHandler;
        public HttpClient(HttpClientHandler handler)
        {
            _clientHandler = handler;
        }

        private HttpResponseMessage Get(string url)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.AllowAutoRedirect = _clientHandler.AllowAutoRedirect;
            if (_clientHandler.UseCookies)
            {
                webRequest.CookieContainer = _clientHandler.CookieContainer;
            }
            webRequest.UserAgent = "Testing Agent";
            webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            var message = new HttpResponseMessage();
            message.Content = new HttpResponseContent();
            message.Headers = new HttpHeaders();
            using (var response = (HttpWebResponse)webRequest.GetResponse())
            {
                message.StatusCode = response.StatusCode;
                if (response.Headers.AllKeys.Contains("Location"))
                {
                    message.Headers.Location = response.Headers["Location"];
                }
                using (var stream = response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        var reader = new StreamReader(stream);
                        message.Content.Content = reader.ReadToEnd();
                        reader.Close();
                    }
                    else
                    {
                        message.Content.Content = "";
                    }
                }
                if (_clientHandler.UseCookies)
                {
                    _clientHandler.CookieContainer = webRequest.CookieContainer;
                }
            }
            return message;
        }

        public Task<HttpResponseMessage> GetAsync(string url)
        {
            var task = new Task<HttpResponseMessage>(() => Get(url));
            task.Start();
            return task;
        }

        public HttpResponseMessage Post(string url, HttpContent content)
        {
            var webRequest = (HttpWebRequest) WebRequest.Create(url);
            webRequest.AllowAutoRedirect = _clientHandler.AllowAutoRedirect;
            if (_clientHandler.UseCookies)
            {
                webRequest.CookieContainer = _clientHandler.CookieContainer;
            }
            webRequest.UserAgent = "Testing Agent";
            webRequest.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            webRequest.Method = "POST";
            using (var stream = webRequest.GetRequestStream())
            {
                var postData = content.Serialise();
                stream.Write(Encoding.UTF8.GetBytes(postData), 0, postData.Length);
            }
            webRequest.ContentType = "application/x-www-form-urlencoded";
            var message = new HttpResponseMessage();
            message.Content = new HttpResponseContent();
            message.Headers = new HttpHeaders();
            using (var resp = (HttpWebResponse) webRequest.GetResponse())
            {
                message.StatusCode = resp.StatusCode;
                if (resp.Headers.AllKeys.Contains("Location"))
                {
                    message.Headers.Location = resp.Headers["Location"];
                }
                using (var stream = resp.GetResponseStream())
                {
                    if (stream != null)
                    {
                        var reader = new StreamReader(stream);
                        message.Content.Content = reader.ReadToEnd();
                        reader.Close();
                    }
                    else
                    {
                        message.Content.Content = "";
                    }
                }
                if (_clientHandler.UseCookies)
                {
                    _clientHandler.CookieContainer = webRequest.CookieContainer;
                }
            }
            return message;
        }

        public Task<HttpResponseMessage> PostAsync(string url, HttpContent content)
        {
            var task = new Task<HttpResponseMessage>(() => Post(url, content));
            task.Start();
            return task;
        }
    }

    #region Post Data
    public abstract class HttpContent
    {
        public abstract string Serialise();
    }

    public class FormUrlEncodedContent : HttpContent
    {
        private readonly Dictionary<string, string> _dictionary;
        public FormUrlEncodedContent(Dictionary<string, string> dictionary)
        {
            _dictionary = dictionary;
        }

        public FormUrlEncodedContent(List<KeyValuePair<string, string>> kvp)
        {
            var dictionary = kvp.ToDictionary(keyItem => keyItem.Key, valueItem => valueItem.Value);
            _dictionary = dictionary;
        }

        public override string Serialise()
        {
            var content = _dictionary.Keys.Aggregate("", (current, key) => current + (key + '=' + _dictionary[key] + '&'));
            return content.Substring(0, content.Length - 1);
        }
    }

    public class StringContent : HttpContent
    {
        private readonly string _str;
        public StringContent(string str)
        {
            _str = str;
        }

        public override string Serialise()
        {
            return _str;
        }
    }
    #endregion

    public class HttpClientHandler
    {
        public bool AllowAutoRedirect { get; set; }

        public bool UseCookies
        {
            get { return CookieContainer != null; }
            set { CookieContainer = value ? new CookieContainer() : null; }
        }

        public CookieContainer CookieContainer { get; set; }
    }

    public class HttpResponseMessage
    {
        public void EnsureSuccessStatusCode()
        {
            var code = (int) StatusCode;
            if (code >= 400)
            {
                throw new Exception("Status code was not successful.");
            }
        }

        public HttpStatusCode StatusCode { get; set; }
        public HttpResponseContent Content { get; set; }
        public HttpHeaders Headers { get; set; }
    }

    public class HttpHeaders
    {
        public string Location { get; set; }
    }

    public class HttpResponseContent
    {
        public string Content { get; set; }

        public Task<string> ReadAsStringAsync()
        {
            var task = new Task<string>(() => Content);
            task.Start();
            return task;
        }

        public Task<Stream> ReadAsStreamAsync()
        {
            var task = new Task<Stream>(() =>
            {
                var stream = new MemoryStream();
                stream.Write(Encoding.UTF8.GetBytes(Content), 0, Content.Length);
                stream.Position = 0;
                return stream;
            });
            task.Start();
            return task;
        }
    }

    public enum HttpMethod
    {
        Get,
        Post,
        Put,
        Head,
        Delete,
        Trace,
        Options
    }
}
