using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AgilefantTimes.API.Restful
{
    public class HttpRequestProcessor
    {
        private readonly TcpClient _socket;
        private readonly Action<HttpRequestProcessor> _requestHandler;
        private Stream _inputStream;
        private StreamWriter _outputStream;

        public HttpMethod HttpMethod { get; private set; }
        public string HttpUrl { get; private set; }
        public string HttpVersion { get; private set; }
        public Hashtable HttpHeaders { get; private set; }
        public Hashtable HttpResponseHeaders { get; private set; }
        public Hashtable HttpCookies { get; private set; }
        public Hashtable HttpResponseSetCookies { get; private set; }
        public string HttpPostData { get; private set; }
        public bool ResponseWritten { get; private set; }

        private const int MaxPostSize = 10485760;
        private const int BufSize = 4096;

        public HttpRequestProcessor(TcpClient tcpClient, Action<HttpRequestProcessor> handleRequest)
        {
            _requestHandler = handleRequest;
            _socket = tcpClient;
        }

        public string DecodeAuthenticationHeader()
        {
            var authString = (string) HttpHeaders["Authorization"];
            var data = Convert.FromBase64String(authString.Split(' ')[1]);
            return Encoding.UTF8.GetString(data);
        }

        private string InputReadLine()
        {
            var data = "";
            while (true)
            {
                var nextChar = _inputStream.ReadByte();
                if (nextChar == '\n')
                {
                    break;
                }
                if (nextChar == '\r')
                {
                    continue;
                }
                if (nextChar == -1)
                {
                    Thread.Sleep(1);
                    continue;
                }
                data += Convert.ToChar(nextChar);
            }
            return data;
        }

        public void ProcessInput()
        {
            _inputStream = new BufferedStream(_socket.GetStream());
            _outputStream = new StreamWriter(new BufferedStream(_socket.GetStream()));

            while (true)
            {
                ResponseWritten = false;
                HttpHeaders = new Hashtable();
                HttpResponseHeaders = new Hashtable();
                HttpCookies = new Hashtable();
                HttpResponseSetCookies = new Hashtable();
                try
                {
                    ParseRequest();
                    ReadHeaders();
                    ReadCookies();

                    string postData = null;
                    if (HttpMethod != HttpMethod.Post)
                    {
                        GetPostData();
                    }
                    Debug.WriteLine("[" + Thread.CurrentThread.ManagedThreadId + "] " + HttpMethod + " " + HttpUrl);
                    _requestHandler.Invoke(this);
                }
                catch (Exception e)
                {
                    WriteServerFailure();
                    _outputStream.Flush();
#if DEBUG
                    Console.Error.WriteLine(e.StackTrace);
#endif
                    break;  // 500 internal server error? probably happened here...
                }
                _outputStream.Flush();
                _inputStream.Flush();

                if ((string)HttpHeaders["Connection"] == "close") break;
            }

            _inputStream = null;
            _outputStream = null;
            _socket.Close();
        }

        private void ParseRequest()
        {
            var request = InputReadLine();
            var tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }

            switch (tokens[0].ToLower())
            {
                case "get": HttpMethod = HttpMethod.Get; break;
                case "post": HttpMethod = HttpMethod.Post; break;
                case "put": HttpMethod = HttpMethod.Put; break;
                case "head": HttpMethod = HttpMethod.Head; break;
                case "delete": HttpMethod = HttpMethod.Delete; break;
                case "trace": HttpMethod = HttpMethod.Trace; break;
                case "options": HttpMethod = HttpMethod.Options; break;
                default: goto case "trace";
            }

            HttpUrl = tokens[1];
            HttpVersion = tokens[2];
        }

        private void ReadCookies()
        {
            var cookie = (string)HttpHeaders["Cookie"];
            if (cookie == null) return;
            var cookies = cookie.Split(new []{"; ", ";"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var spl in cookies.Select(s => s.Split('=')))
            {
                HttpCookies[spl[0].Trim()] = spl[1].Trim();
            }
        }

        private void ReadHeaders()
        {
            string line;
            while ((line = InputReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }

                var separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("Invalid HTTP header: \"" + line + "\"");
                }
                var name = line.Substring(0, separator);
                var value = line.Substring(separator + 1).Trim();
                HttpHeaders[name] = value;
            }
        }

        private void GetPostData()
        {
            var ms = new MemoryStream();
            if (HttpHeaders.ContainsKey("Content-Length"))
            {
                var contentLen = Convert.ToInt32(HttpHeaders["Content-Length"]);
                if (contentLen > MaxPostSize)
                {
                    throw new Exception(string.Format("POST Content-Length({0}) too big!", contentLen));
                }
                var buf = new byte[BufSize];
                var toRead = contentLen;
                while (toRead > 0)
                {
                    var numread = _inputStream.Read(buf, 0, Math.Min(BufSize, toRead));
                    if (numread == 0)
                    {
                        if (toRead == 0)
                        {
                            break;
                        }
                        throw new Exception("Client disconnected while reading POST data.");
                    }
                    toRead -= numread;
                    ms.Write(buf, 0, numread);
                }
                ms.Seek(0, SeekOrigin.Begin);
            }

            var reader = new StreamReader(ms);
            HttpPostData = reader.ReadToEnd();
            reader.Close();
        }

        public void WriteSuccess(string response = null, string contentType = "application/json")
        {
            WriteResponse("200 OK", response, contentType);
        }

        public void WriteAuthRequired(bool basicAuthentication = true, string errorMessage = "<b>401, Thou must login before slaying dragons.</b>")
        {
            HttpResponseHeaders["WWW-Authenticate"] = "Basic realm=\"Login Required\"";
            WriteResponse("401 Not Authorized", errorMessage);
        }

        public void WriteFailure(string errorMessage = "<b>404, I think it was over there somewhere...</b>")
        {
            WriteResponse("404 File Not Found", errorMessage);
        }

        private void WriteServerFailure(string errorMessage = "<b>500, Oh fiddlesticks! That's an error and it is all YOUR fault.</b>")
        {
            HttpHeaders["Connection"] = "close";
            WriteResponse("500 Internal Server Error", errorMessage);
        }

        public void WriteResponse(string status, string response = null, string contentType = null)
        {
            if (ResponseWritten) throw new Exception("Cannot send new response after response has been sent.");
            ResponseWritten = true;
            _outputStream.WriteLine("HTTP/1.1 " + status);
            var connection = (string) HttpHeaders["Connection"];
            if (string.IsNullOrWhiteSpace(connection))
            {
                connection = "close";
                HttpHeaders["Connection"] = connection;
            }
            _outputStream.WriteLine("Connection: " + connection);
            if (!string.IsNullOrWhiteSpace(response))
            {
                contentType = string.IsNullOrWhiteSpace(contentType) ? "text/html" : contentType;
                _outputStream.WriteLine("Content-Type: " + contentType);
                _outputStream.WriteLine("Content-Length: " + response.Length);
            }
            foreach (var header in HttpResponseHeaders.Keys)
            {
                _outputStream.WriteLine(header + ": " + HttpResponseHeaders[header]);
            }
            foreach (var cookie in HttpResponseSetCookies.Keys)
            {
                _outputStream.WriteLine("Set-Cookie: " + cookie + "=" + HttpResponseSetCookies[cookie]);
            }
            _outputStream.WriteLine("");
            if (!string.IsNullOrWhiteSpace(response))
            {
                _outputStream.WriteLine(response);
            }
        }
    }
}