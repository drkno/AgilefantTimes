using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AgilefantTimes.API.Restful
{
    public partial class RestServer
    {
        private readonly int _port;
        private readonly string _serverBaseDirectory;
        private TcpListener _listener;
        private bool _isActive;
        private readonly List<RestfulUrlHandler> _handlers;
        private Thread _listenThread;
        private readonly ConcurrentDictionary<string, string> _filesCache;
        private readonly FileSystemWatcher _watcher;

        public RestServer(int port, string serverBaseDirectory = null)
        {
            _port = port;
            _serverBaseDirectory = serverBaseDirectory == null ? null : Path.GetFullPath(serverBaseDirectory);
            _handlers = new List<RestfulUrlHandler>();
            _filesCache = new ConcurrentDictionary<string, string>();
            if (serverBaseDirectory == null) return;
            _watcher = new FileSystemWatcher(_serverBaseDirectory)
                       {
                           IncludeSubdirectories = true,
                           NotifyFilter = NotifyFilters.FileName
                                          | NotifyFilters.Size
                                          | NotifyFilters.LastWrite
                                          | NotifyFilters.CreationTime
                       };
            _watcher.Changed += FileSystemChanged;
            _watcher.Created += FileSystemChanged;
            _watcher.Deleted += FileSystemChanged;
            _watcher.Renamed += FileSystemChanged;
        }

        private void FileSystemChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            string removed;
            _filesCache.TryRemove(fileSystemEventArgs.FullPath, out removed);
        }

        private void AddHandler(RestfulUrlHandler handler)
        {
            _handlers.Add(handler);
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
                    var processor = new HttpRequestProcessor(s, HandleRequest);
                    var thread = new Thread(processor.ProcessInput);
                    thread.Start();
                    Thread.Sleep(1);
                }
            }
            catch (SocketException e)
            {
                // The server died due to a bug in mono
                if (e.Message.Contains("The socket has been shut down"))
                {
                    Logger.Log("Mono shutdown the server. Please use .NET Core whenever it starts supporting your platform to avoid this.", LogLevel.Error);
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

            if (!path.StartsWith(_serverBaseDirectory))
            {
                requestProcessor.WriteResponse("403 Forbidden", "Thou shall not pass!");
                return;
            }

            string data;
            if (_filesCache.TryGetValue(path, out data))
            {
                var ext = Path.GetExtension(path);
                var type = MimeTypes[ext];
                requestProcessor.WriteSuccess(data, type);
                return;
            }

            if (_serverBaseDirectory != null && File.Exists(path))
            {
                var ext = Path.GetExtension(path);
                var type = MimeTypes[ext];
                var reader = new StreamReader(path);
                data = reader.ReadToEnd();
                requestProcessor.WriteSuccess(data, type);
                reader.Close();
                _filesCache[path] = data;
            }
            else
            {
                requestProcessor.WriteResponse("404 Not Found", "<iframe src=\"https://thebest404pageever.com/\" " +
                                                                "style=\"position: absolute; width: 99%; height: 99%; top: 0; left: 0;\">" +
                                                                "404, thou must find mordor before getting eagles.</iframe>", "text/html");
            }
        }

        private void Start()
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
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = true;
            }
        }

        public void Stop()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
            }
            _isActive = false;
            _listener.Stop();
            _listenThread.Join();
        }
    }
}
