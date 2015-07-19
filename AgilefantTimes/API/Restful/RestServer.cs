using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Threading;

namespace AgilefantTimes.API.Restful
{
    public class RestServer
    {
        private readonly MemoryCache _requestCache;
        private readonly int _port;
        private readonly string _serverBaseDirectory;
        private TcpListener _listener;
        private bool _isActive;
        private readonly List<RestfulUrlHandler> _handlers;
        private Thread _listenThread;

        public RestServer(int port, string serverBaseDirectory = null)
        {
            _port = port;
            _serverBaseDirectory = serverBaseDirectory;
            _handlers = new List<RestfulUrlHandler>();
            _requestCache = new MemoryCache("RestServerCache");
        }

        public void AddHandler(RestfulUrlHandler handler)
        {
            _handlers.Add(handler);
        }

        public void RemoveHandler(RestfulUrlHandler handler)
        {
            _handlers.Remove(handler);
        }

        public void RemoveHandler(int index)
        {
            _handlers.RemoveAt(index);
        }

        public RestfulUrlHandler this[int index]
        {
            get { return _handlers[index]; }
            set { _handlers[index] = value; }
        }

        public static RestServer operator +(RestServer e, RestfulUrlHandler f)
        {
            e.AddHandler(f);
            return e;
        }

        private bool Listen()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();
                while (_isActive)
                {
                    var s = _listener.AcceptTcpClient();
                    var processor = new HttpRequestProcessor(s, HandleRequest, _requestCache);
                    var thread = new Thread(processor.ProcessInput);
                    thread.Start();
                    Thread.Sleep(1);
                }
            }
            catch (SocketException e)
            {
                // fuck you mono
                if (e.Message.Contains("The socket has been shut down"))
                {
                    Console.Error.WriteLine("Mono just committed suicide AND killed the server. Thanks a lot mono.");
                    return false;
                }
                if (e.SocketErrorCode == SocketError.Interrupted) return true;
                throw;
            }
            return true;
        }

        private void HandleRequest(HttpRequestProcessor requestProcessor)
        {
            if (_handlers.Any(restfulUrlHandler => restfulUrlHandler.Execute(requestProcessor)))
            {
                if (!requestProcessor.ResponseWritten) requestProcessor.WriteSuccess();
                return;
            }

            var url = requestProcessor.HttpUrl;
            if (string.IsNullOrWhiteSpace(url) || url == "/") url = "/index.html";
            var path = Path.GetFullPath(_serverBaseDirectory + url);
            Console.WriteLine(path);
            if (_serverBaseDirectory != null && File.Exists(path))
            {
                var reader = new StreamReader(path);
                requestProcessor.WriteSuccess(reader.ReadToEnd(), "text/html");
                reader.Close();
            }
            else
            {
                requestProcessor.HttpResponseHeaders["Location"] = "https://thebest404pageever.com/";
                requestProcessor.WriteResponse("302 Found", "404, thou must find mordor before getting eagles.");
                //requestProcessor.WriteFailure();
            }
        }

        public void Start()
        {
            if (_listenThread != null && _isActive)
            {
                throw new InvalidOperationException("Cannot start the server while it is already running.");
            }
            _isActive = true;
            while (_isActive)
            {
                if (Listen()) break;
            }
        }

        public void StartAsync()
        {
            _listenThread = new Thread(Start);
            _listenThread.Start();
        }

        public void Stop()
        {
            _isActive = false;
            _listener.Stop();
            _listenThread.Join();
        }
    }
}
