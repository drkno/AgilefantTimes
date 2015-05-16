using System;
using System.Collections.Generic;
using System.Security;
using AgilefantTimes.API.Agilefant;
using Newtonsoft.Json;

namespace AgilefantTimes.API.Restful
{
    public class RestApiClient
    {
        private int _session;
        private readonly RestServer _server;
        private readonly Dictionary<int, AgilefantClient> _sessions; 

        public RestApiClient(int port = 8080, string serverDirectory = null)
        {
            _server = new RestServer(port, serverDirectory);
            _sessions = new Dictionary<int, AgilefantClient>();

            _server += new RestfulUrlHandler("/rest/sprint/summary/[0-9]+", (p, s) =>
            {
                var session = GetClientSession(p);
                if (session == null) return;
                var summaries = session.GetSprintSummaries(int.Parse(s[3]));
                var json = JsonConvert.SerializeObject(summaries);
                p.WriteSuccess(json);
            });

            _server += new RestfulUrlHandler("/rest/sprint/[0-9]+", (p, s) =>
            {
                var session = GetClientSession(p);
                if (session == null) return;
                var sprint = session.GetSprint(int.Parse(s[2]));
                var json = JsonConvert.SerializeObject(sprint);
                p.WriteSuccess(json);
            });

            _server += new RestfulUrlHandler("/rest/backlog/[0-9]+", (p, s) =>
            {
                var session = GetClientSession(p);
                if (session == null) return;
                var backlog = session.GetBacklogs(int.Parse(s[2]));
                var json = JsonConvert.SerializeObject(backlog);
                p.WriteSuccess(json);
            });

            _server += new RestfulUrlHandler("/rest/teammembers", (p, s) =>
            {
                var session = GetClientSession(p);
                if (session == null) return;
                var users = session.GetUsers();
                var json = JsonConvert.SerializeObject(users);
                p.WriteSuccess(json);
            });

            _server += new RestfulUrlHandler("/rest/time/[0-9]+/[0-9]+/[0-9]+/[0-9]+", (p, s) =>
            {
                var session = GetClientSession(p);
                if (session == null) return;
                var time = session.GetTime(int.Parse(s[2]), int.Parse(s[3]), int.Parse(s[4]), int.Parse(s[5]));
                var json = JsonConvert.SerializeObject(time);
                p.WriteSuccess(json);
            });
        }

        public void Start()
        {
            _server.Start();
        }

        public void Stop()
        {
            _server.Stop();
        }

        private AgilefantClient GetClientSession(HttpRequestProcessor processor)
        {
            try
            {
                var sessionNumber = processor.HttpCookies["MurcySession"];
                var authorisation = processor.DecodeAuthenticationHeader();
                if (sessionNumber == null && authorisation == null)
                {
                    throw new SecurityException("User is not logged in.");
                }

                if (sessionNumber != null) return _sessions[int.Parse((string) sessionNumber)];

                var login = authorisation.Split(':');
                var session = AgilefantSession.Login(login[0], login[1]);
                var client = new AgilefantClient(session.Result);
                _sessions[_session] = client;
                processor.HttpResponseSetCookies["MurcySession"] = _session;
                _session++;
                return client;
            }
            catch (Exception)
            {
                processor.WriteAuthRequired();
                return null;
            }
        }
    }
}
