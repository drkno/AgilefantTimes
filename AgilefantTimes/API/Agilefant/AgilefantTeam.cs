using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace AgilefantTimes.API.Agilefant
{
    public class AgilefantTeam
    {
        private AgilefantTeam(string name, string description, int id, AgilefantResponsible[] members, Sprint[] sprints)
        {
            Name = name;
            Description = description;
            Id = id;
            Members = members;
            Sprints = sprints;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Id { get; private set; }
        public AgilefantResponsible[] Members { get; private set; }
        public Sprint[] Sprints { get; private set; }

        public static async Task<Dictionary<int, AgilefantTeam>> GetTeams(AgilefantSession session)
        {
            var response = await session.Get("ajax/retrieveAllProductsWithStandalone.action");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var summaries = JsonConvert.DeserializeObject<AgilefantBacklogProductSummary[]>(json);
            var menu = await AgilefantMenuData.GetMenuData(session);
            var teams = new Dictionary<int, AgilefantTeam>();
            foreach (var summary in summaries)
            {
                var data = menu.FirstOrDefault(menuData => menuData.Title == summary.Name);
                if (data == null) continue;
                var sprintData = await AgilefantSprint.GetSprints(data.Children[0].Id, session);

                var sprints = sprintData.Select(sprint => new Sprint(sprint.Name, sprint.Description, sprint.Id, sprint.StartDate, sprint.EndTime)).ToList();

                sprints.Sort((a, b) =>
                             {
                                 var ai = Regex.Match(a.Name, "[0-9]+");
                                 var bi = Regex.Match(b.Name, "[0-9]+");
                                 if (!ai.Success || !bi.Success) return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
                                 return int.Parse(ai.Value).CompareTo(int.Parse(bi.Value));
                             });
                var sprint0 = Regex.Match(sprints.First().Name, "[0-9]+");
                if (sprint0.Success && int.Parse(sprint0.Value) == 0) sprints.RemoveAt(0);

                var assignees = sprintData[0].Assignees;
                for (var i = 1; i < sprintData.Length && assignees.Length == 0; i++)
                {
                    assignees = sprintData[i].Assignees;    // sprint 0 sometimes does not have assignees
                }

                var name = summary.Name;
                var match = Regex.Match(name, @"^\s*[0-9]+\s*[.:]");
                if (match.Success)
                {
                    name = name.Substring(match.Length);
                }

                teams.Add(summary.Id, new AgilefantTeam(name.Trim(), summary.Description, summary.Id, assignees, sprints.ToArray()));
            }
            return teams;
        }

        public class Sprint
        {
            // ReSharper disable once UnusedParameter.Local
            public Sprint(string name, string description, int id, DateTime startDate, DateTime endDate)
            {
                Name = name;
                Id = id;
                StartDate = startDate;
                EndDate = endDate;
            }

            public string Name { get; }
            public string Description { get; } = "<Description Temporarily Disabled>";
            public int Id { get; }
            public DateTime StartDate { get; }
            public DateTime EndDate { get; }
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
