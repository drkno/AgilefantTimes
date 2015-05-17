﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using AgilefantTimes.API.Agilefant;
using AgilefantTimes.Output;
using Newtonsoft.Json;

namespace AgilefantTimes.API.Restful
{
    public class RestApiClient
    {
        //private int _session;
        private readonly RestServer _server;
        //private readonly Dictionary<int, AgilefantClient> _sessions;
        private AgilefantClient _client;
        private readonly Config _config;

        public RestApiClient(Config config, int port = 8080, string serverDirectory = null)
        {
            _config = config;
            _server = new RestServer(port, serverDirectory);
            //_sessions = new Dictionary<int, AgilefantClient>();

            _server += new RestfulUrlHandler("/rest/([0-9]+/)?sprint/summary(/([0-9]+/?)?)?", (p, s) =>
            {
                var session = GetClientSession(p);
                if (session == null) return;

                int teamNumber, sprintNumber;
                if (!int.TryParse(s[1], out teamNumber))
                    teamNumber = _config.TeamNumber;
                if (!(s.Length >= 5 && int.TryParse(s[4], out sprintNumber)) && !(s.Length >= 4 && int.TryParse(s[3], out sprintNumber)))
                    sprintNumber = _config.SprintNumber;

                var users = session.GetUsers().Result;
                var backlogs = session.GetBacklogs(teamNumber).Result;
                var sprintSummaries = session.GetSprintSummaries(backlogs[0].Id).Result;
                var sprintSummary = AgilefantClient.SelectSprint(sprintNumber, sprintSummaries);

                var hours = (from user in users
                             let tasks = session.GetTime(teamNumber, backlogs[0].Id, sprintSummary.Id, user.Id).Result
                             select new JsonOutputTime((_config.DisplayUsercode ? user.UserCode : user.Name), tasks)).ToList();
                var jsonOutput = new JsonOutput(backlogs[0].Name, sprintSummary.Name, hours);
                var json = JsonConvert.SerializeObject(jsonOutput);
                p.WriteSuccess(json);
            });

            _server += new RestfulUrlHandler("/rest/([0-9]+/)?sprint(/([0-9]+/?)?)?", (p, s) =>
            {
                var session = GetClientSession(p);
                if (session == null) return;

                int teamNumber, sprintNumber;
                if (!int.TryParse(s[1], out teamNumber))
                    teamNumber = _config.TeamNumber;
                if (!(s.Length >= 4 && int.TryParse(s[3], out sprintNumber)) && !(s.Length >= 3 && int.TryParse(s[2], out sprintNumber)))
                    sprintNumber = _config.SprintNumber;

                var backlogs = session.GetBacklogs(teamNumber).Result;
                var sprintSummaries = session.GetSprintSummaries(backlogs[0].Id).Result;
                var sprintSummary = AgilefantClient.SelectSprint(sprintNumber, sprintSummaries);
                var sprint = session.GetSprint(sprintSummary.Id).Result;
                var json = JsonConvert.SerializeObject(sprint);
                p.WriteSuccess(json);
            });

            _server += new RestfulUrlHandler("/rest/[0-9]{3}[0-9]{2}/sprint/[0-9]+/?", (p, s) =>
            {
                var session = GetClientSession(p);
                if (session == null) return;
                var userCode = s[1];
                var sprintNumber = int.Parse(s[3]);


                



            });

            _server += new RestfulUrlHandler("/rest/teams/?", (p, s) =>
            {
                var session = GetClientSession(p);
                if (session == null) return;


            });
        }

        public void Start()
        {
            _server.Start();
        }

        public void Stop()
        {
            _server.Stop();
            if (_client != null) _client.Session.Logout();
        }

        private AgilefantClient GetClientSession(HttpRequestProcessor processor)
        {
            try
            {
                if (_client != null) return _client;

                var session = AgilefantSession.Login(_config.Username, _config.Password).Result;
                _client = new AgilefantClient(session);
                return _client;
                /*var sessionNumber = processor.HttpCookies["MurcySession"];
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
                return client;*/
            }
            catch (Exception)
            {
                processor.WriteAuthRequired();
                return null;
            }
        }
    }
}