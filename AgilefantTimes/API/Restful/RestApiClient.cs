using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using AgilefantTimes.API.Agilefant;
using AgilefantTimes.Output;
using Newtonsoft.Json;

namespace AgilefantTimes.API.Restful
{
    public class RestApiClient
    {
        private readonly RestServer _server;
        private AgilefantClient _client;
        private readonly Config _config;
        private readonly List<AgilefantClient> _logins;

        public RestApiClient(Config config, int port = 8080, string serverDirectory = null)
        {
            _config = config;
            _server = new RestServer(port, serverDirectory);
            _logins = new List<AgilefantClient>();

            _server += new RestfulUrlHandler("/rest/([0-9]+/)?sprint/summary(/([0-9]+/?)?)?", (p, s) =>
            {
                var session = GetClientSession(p);
                if (session == null) return;

                int teamNumber, sprintNumber;
                if (!int.TryParse(s[1], out teamNumber))
                    teamNumber = _config.TeamNumber;
                if (!(s.Length >= 5 && int.TryParse(s[4], out sprintNumber)) && !(s.Length >= 4 && int.TryParse(s[3], out sprintNumber)))
                    sprintNumber = _config.SprintNumber;

                var teamsTask = session.GetTeams();
                var usersTask = session.GetUsers();
                var backlogsTask = session.GetBacklogs(teamNumber);
                
                var teams = teamsTask.Result;
                var users = teams[teamNumber - 1].Members;
                Array.Sort(users, (a, b) =>
                                  {
                                      if (string.IsNullOrWhiteSpace(a.Name) || string.IsNullOrWhiteSpace(b.Name))
                                      {
                                          return string.Compare(a.Initials, b.Initials, StringComparison.Ordinal);
                                      }
                                      return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
                                  });


                var u = usersTask.Result;
                foreach (var user in users)
                {
                    var result = u.FirstOrDefault(t => t.UserCode == user.Initials);
                    user.Name = result == null ? "" : result.Name;
                }

                var backlogs = backlogsTask.Result;
                var sprintSummaries = session.GetSprintSummaries(backlogs[0].Id).Result;
                var sprintSummary = AgilefantClient.SelectSprint(sprintNumber, sprintSummaries);

                var sprint = int.Parse(Regex.Match(sprintSummary.Name, "[0-9]+").Value);

                var hours = (from user in users
                             let tasks = session.GetTime(teamNumber, backlogs[0].Id, sprintSummary.Id, user.Id).Result
                             select new JsonOutputTime((_config.DisplayUsercode ? user.Initials : user.Name), tasks)).ToList();
                var jsonOutput = new JsonOutput(backlogs[0].Name, sprintSummary.Name, hours, sprint);
                var json = JsonConvert.SerializeObject(jsonOutput, Formatting.Indented);
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
                var json = JsonConvert.SerializeObject(sprint, Formatting.Indented);
                p.WriteSuccess(json);
            });

            _server += new RestfulUrlHandler("/rest/(([^0-9/]+)|([a-z]{3}[0-9]{2,3}))/sprint/[0-9]+/?", (p, s) =>
            {
                var session = GetClientSession(p);
                if (session == null) return;

                var userCode = s[1];
                var sprintNumber = int.Parse(s[3]);

                var backlogs = session.GetBacklogs(_config.TeamNumber);
                var users = session.GetUsers().Result;
                var userId = -1;
                var name = "";
                foreach (var user in users.Where(user => user.UserCode == userCode))
                {
                    userId = user.Id;
                    name = user.Name;
                    break;
                }
                if (userId < 0)
                {
                    AgilefantTeam userTeam = null;
                    foreach (var team in session.GetTeams().Result)
                        foreach (var member in team.Members.Where(member =>
                            member.Initials.ToLower(CultureInfo.InvariantCulture) == userCode.ToLower(CultureInfo.InvariantCulture)))
                        {
                            userTeam = team;
                            userId = member.Id;
                            break;
                        }

                    if (userTeam == null)
                    {
                        p.WriteResponse("404 Not Found", "{\"success\":false,\"reason\":\"No Such User\"}", "application/json");
                        return;
                    }

                    backlogs = session.GetBacklogs(userTeam.Id);
                    name = userCode;
                }
                
                var sprintSummaries = session.GetSprintSummaries(backlogs.Result[0].Id).Result;
                var sprintSummary = AgilefantClient.SelectSprint(sprintNumber, sprintSummaries);
                var times = session.GetLoggedTaskTime(userId, sprintSummary.StartDate, sprintSummary.EndDate);
                var days = (sprintSummary.EndDate.DayOfYear <= DateTime.Now.DayOfYear ? sprintSummary.EndDate.DayOfYear : DateTime.Now.DayOfYear)
                            - sprintSummary.StartDate.DayOfYear;
                if (days < 0) days = 0;
                var stats = new UserPerformed(userId, userCode, name, times.Result, days);
                p.WriteSuccess(JsonConvert.SerializeObject(stats, Formatting.Indented));
            });

            _server += new RestfulUrlHandler("/rest/teams/?", (p, s) =>
            {
                var session = GetClientSession(p);
                if (session == null) return;

                var teams = session.GetTeams().Result;
                var u = session.GetUsers().Result.ToDictionary(user => user.Initials, user => user.Name);
                foreach (var member in teams.SelectMany(team => team.Members))
                {
                    string name;
                    if (u.TryGetValue(member.Initials, out name))
                    {
                        member.Name = name;
                    }
                }

                var json = JsonConvert.SerializeObject(teams, Formatting.Indented);
                p.WriteSuccess(json);
            });

            _server += new RestfulUrlHandler("/rest/team/[0-9]/?", (p, s) =>
            {
                var session = GetClientSession(p);
                if (session == null) return;

                var usersTask = session.GetUsers();
                var teams = session.GetTeams().Result;

                int teamNumber;
                if (!int.TryParse(s[2], out teamNumber) || teamNumber < 0 || teamNumber >= teams.Length)
                {
                    return;
                }

                var u = usersTask.Result.ToDictionary(user => user.Initials, user => user.Name);
                foreach (var member in teams[teamNumber + 1].Members)
                {
                    string name;
                    if (u.TryGetValue(member.Initials, out name))
                    {
                        member.Name = name;
                    }
                }

                var json = JsonConvert.SerializeObject(teams[teamNumber + 1], Formatting.Indented);
                p.WriteSuccess(json);
            });

            _server += new RestfulUrlHandler("/rest/login/?", (p, s) =>
            {
                if (p.HttpHeaders.ContainsKey("Authorization") || p.HttpPostData.Length != 0)
                {
                    string username, password;
                    if (p.HttpHeaders.ContainsKey("Authorization"))
                    {
                        var result = p.DecodeAuthenticationHeader();
                        var ind = result.IndexOf(':');
                        username = result.Substring(0, ind);
                        password = result.Substring(ind + 1);
                    }
                    else
                    {
                        var keys = p.HttpPostData.Split('&').Select(key => key.Split('=')).ToDictionary(values => values[0], values => values[1]);
                        username = keys["username"];
                        password = keys["password"];
                    }

                    try
                    {
                        var session = AgilefantSession.LoginTemporary(username, password).Result;
                        if (session == null)
                        {
                            throw new SecurityException("Not logged in.");
                        }
                        var client = new AgilefantClient(session);
                        
                        _logins.Add(client);
                        var data = _logins.Count - 1;
                        p.HttpResponseSetCookies.Add("aft-session", data);
                        p.WriteSuccess("{\"success\":true}");
                    }
                    catch (Exception)
                    {
                        p.WriteAuthRequired(false, "Thou must login before slaying dragons.", "{\"success\":false}", "application/json");
                    }
                }
                else if (p.HttpCookies.ContainsKey("aft-session"))
                {
                    p.WriteSuccess("{\"success\":true}");
                }
                else
                {
                    p.WriteAuthRequired(p.HttpPostData.Length == 0, "Thou must login before slaying dragons.", "{\"success\":false}", "application/json");
                }
            });

            _server += new RestfulUrlHandler("/rest/?", (p, s) =>
            {
                var methods = new List<object>
                {
                    new
                    {
                        url = "/rest/{{teamNumber?}}/sprint/summary/{{sprintNumber?}}",
                        fields =
                            new[] {"teamNumber, optional, the team number", "sprintNumber, optional, the sprint number"},
                        description = "Gets summary details about a sprint for a team."
                    },
                    new
                    {
                        url = "/rest/{{teamNumber?}}/sprint/{{sprintNumber?}}",
                        fields =
                            new[] {"teamNumber, optional, the team number", "sprintNumber, optional, the sprint number"},
                        description = "Gets full, uninterpreted details about a sprint for a team."
                    },
                    new
                    {
                        url = "/rest/{{userCode}}/sprint/{{sprintNumber}}",
                        fields =
                            new[]
                            {
                                "userCode, required, the username of the user",
                                "sprintNumber, required, the sprint number"
                            },
                        description = "Gets all activity of a user for a sprint."
                    },
                    new
                    {
                        url = "/rest/teams",
                        fields = new string[0],
                        description =
                            "Gets the details about all teams, their sprints and members (where the current authentication allows)."
                    },
                    new
                    {
                        url = "/rest/team/{{teamNumber}}",
                        fields = new[] {"teamNumber, required, the number of the team"},
                        description =
                            "Gets the details about a team, its sprints and members (where the current authentication allows)."
                    },
                    new
                    {
                        url = "/rest/login",
                        fields = new string[0],
                        description =
                            "Logs the user in. Accepts optional post parameters username and password. A logged in user " +
                            "will use their own account instead of the global account."
                    },
                    new
                    {
                        url = "/rest",
                        fields = new string[0],
                        description = "Gets this help text about the avalible URLs."
                    }
                };
                p.WriteSuccess(JsonConvert.SerializeObject(methods, Formatting.Indented));
            });
        }

        public void Start()
        {
            _server.StartAsync();
        }

        public void Stop()
        {
            _server.Stop();
            _client?.Session.Logout();
        }

        private AgilefantClient GetClientSession(HttpRequestProcessor processor)
        {
            AgilefantClient client = _client;
            int index = -1;
            try
            {
                if (processor.HttpCookies.ContainsKey("aft-session"))
                {
                    var b64 = (string)processor.HttpCookies["aft-session"];
                   
                    if (!int.TryParse(b64, out index))
                    {
                        index = -1;
                        throw new SecurityException("You are not logged in");
                    }
                    client = _logins[index];
                }

                if (client != null)
                {
                    var response = client.Session.Get("loginContext.action").Result;
                    if (string.IsNullOrWhiteSpace(response.Headers.Location) && !response.Content.Content.Contains("Agilefant login") ||
                        !string.IsNullOrWhiteSpace(response.Headers.Location) && (!response.Headers.Location.Contains("login.jsp") && !response.Headers.Location.Contains("error.json")))
                        return client;
                    client.Session.Logout();
                    client.Session.ReLogin();
                    return client;
                }

                var session = AgilefantSession.Login(_config.Username, _config.Password).Result;
                _client = new AgilefantClient(session);
                return _client;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                try
                {
                    if(index != -1)
                    {
                        throw new Exception("Can't relogin");
                    }
                    _client.Session.Logout();
                    _client.Session.ReLogin();
                    return _client;
                }
                catch (Exception)
                {
                    if (index  != -1)
                    {
                        _logins.RemoveAt(index);
                    }
                    processor.HttpResponseHeaders["Location"] = "/rest/login";
                    processor.WriteResponse("302 Found");
                    return null;
                }
            }
        }
    }
}
