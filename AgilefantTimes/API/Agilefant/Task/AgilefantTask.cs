using AgilefantTimes.API.Agilefant.Common;
using Newtonsoft.Json;

namespace AgilefantTimes.API.Agilefant.Task
{
    public class AgilefantTask : AgilefantBase
    {
        [JsonProperty("description")]
        public string Description { get; private set; }
        [JsonProperty("effortLeft")]
        public int? EffortLeft { get; private set; }
        [JsonProperty("effortSpent")]
        public int? EffortSpent { get; private set; }
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("originalEstimate")]
        public int? OriginalEstimate { get; private set; }
        [JsonProperty("rank")]
        public int Rank { get; private set; }
        [JsonProperty("responsibles")]
        public AgilefantResponsible[] AgilefantResponsibles { get; private set; }
        [JsonProperty("state")]
        public string State { get; private set; }
        [JsonProperty("workingOnTask")]
        public object[] WorkingOnTask { get; private set; }
    }
}
