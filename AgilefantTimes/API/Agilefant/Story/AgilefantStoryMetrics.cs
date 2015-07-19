using AgilefantTimes.API.Agilefant.Common;
using Newtonsoft.Json;

namespace AgilefantTimes.API.Agilefant.Story
{
    public class AgilefantStoryMetrics : AgilefantBase
    {
        [JsonProperty("effortLeft")]
        public int EffortLeft { get; private set; }
        [JsonProperty("effortSpent")]
        public int EffortSpent { get; private set; }
        [JsonProperty("originalEstimate")]
        public int OriginalEstimate { get; private set; }
    }
}