using System.Threading.Tasks;

namespace AgilefantTimes.API.Agilefant
{
    public class AgilefantClient
    {
        private readonly AgilefantSession _session;

        /// <summary>
        /// Creates a new Agilefant Client
        /// </summary>
        /// <param name="session"></param>
        public AgilefantClient(AgilefantSession session)
        {
            _session = session;
        }

        /// <summary>
        /// Gets a list of sprints for the current user
        /// </summary>
        /// <returns></returns>
        public Task<AgilefantSprintSummary[]> GetSprintSummaries(int backlogId)
        {
            return AgilefantSprintSummary.GetSprints(backlogId, _session);
        }

        /// <summary>
        /// Gets detail about a specific sprint.
        /// </summary>
        /// <param name="sprintId">Id of the sprint to get.</param>
        /// <returns>Sprint details.</returns>
        public Task<AgilefantSprint> GetSprint(int sprintId)
        {
            return AgilefantSprint.GetSprint(sprintId, _session);
        }

        /// <summary>
        /// Gets a list of agilefant users
        /// </summary>
        /// <returns></returns>
        public Task<AgilefantUser[]> GetUsers()
        {
            return AgilefantUser.GetAgilefantUsers(_session);
        }

        /// <summary>
        /// Gets a list of the backlogs for a team
        /// </summary>
        /// <param name="teamNumber"></param>
        /// <returns></returns>
        public Task<AgilefantBacklog[]> GetBacklogs(int teamNumber)
        {
            return AgilefantBacklog.GetAgilefantBacklogs(teamNumber, _session);
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
            return AgilefantTime.GetTimes(teamNumber, backlogId, sprintId, userId, _session);
        }
    }
}
