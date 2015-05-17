using System;
using System.Linq;
using System.Threading.Tasks;

namespace AgilefantTimes.API.Agilefant
{
    public class AgilefantClient
    {
        public AgilefantSession Session { get; private set; }

        /// <summary>
        /// Creates a new Agilefant Client
        /// </summary>
        /// <param name="session"></param>
        public AgilefantClient(AgilefantSession session)
        {
            Session = session;
        }

        /// <summary>
        /// Gets a list of sprints for the current user
        /// </summary>
        /// <returns></returns>
        public Task<AgilefantSprintSummary[]> GetSprintSummaries(int backlogId)
        {
            return AgilefantSprintSummary.GetSprints(backlogId, Session);
        }

        /// <summary>
        /// Gets detail about a specific sprint.
        /// </summary>
        /// <param name="sprintId">Id of the sprint to get.</param>
        /// <returns>Sprint details.</returns>
        public Task<AgilefantSprint> GetSprint(int sprintId)
        {
            return AgilefantSprint.GetSprint(sprintId, Session);
        }

        /// <summary>
        /// Gets a list of agilefant users
        /// </summary>
        /// <returns></returns>
        public Task<AgilefantUser[]> GetUsers()
        {
            return AgilefantUser.GetAgilefantUsers(Session);
        }

        /// <summary>
        /// Gets a list of the backlogs for a team
        /// </summary>
        /// <param name="teamNumber"></param>
        /// <returns></returns>
        public Task<AgilefantBacklog[]> GetBacklogs(int teamNumber)
        {
            return AgilefantBacklog.GetAgilefantBacklogs(teamNumber, Session);
        }

        /// <summary>
        /// Gets the time for a user from a team on a backlog for a specific sprint
        /// </summary>
        /// <param name="teamNumber">The team id</param>
        /// <param name="backlogId">The backlog id</param>
        /// <param name="sprintId">The sprint id</param>
        /// <param name="userId">The user id</param>
        /// <returns>The times for the user</returns>
        public Task<AgilefantTime> GetTime(int teamNumber, int backlogId, int sprintId, int userId)
        {
            return AgilefantTime.GetTimes(teamNumber, backlogId, sprintId, userId, Session);
        }

        /// <summary>
        /// Gets a sprint based on current configuration.
        /// </summary>
        /// <param name="sprintNumber">Number of the sprint to get.</param>
        /// <param name="sprintPool">Pool of sprints to select from.</param>
        /// <returns>The requested sprint (or nearest guess).</returns>
        public static AgilefantSprintSummary SelectSprint(int sprintNumber, AgilefantSprintSummary[] sprintPool)
        {
            Array.Sort(sprintPool, (a, b) => a.StartDate.CompareTo(b.StartDate));

            if (sprintNumber >= 0)
            {
                foreach (var sprint in sprintPool.Where(sprint => sprint.Name.Contains(sprintNumber.ToString())))
                {
                    return sprint;
                }
                return sprintPool[sprintNumber];
            }

            var now = DateTime.Now.Date;
            var closest = sprintPool[0];
            foreach (var sprint in sprintPool)
            {
                if (sprint.StartDate <= now && sprint.EndDate >= now)
                {
                    closest = sprint;
                    break;
                }
                var diff = now < sprint.StartDate ? sprint.StartDate - now : now - sprint.EndDate;
                var currDiff = now < closest.StartDate ? closest.StartDate - now : now - closest.EndDate;
                if (diff < currDiff)
                {
                    closest = sprint;
                }
            }
            return closest;
        }
    }
}
