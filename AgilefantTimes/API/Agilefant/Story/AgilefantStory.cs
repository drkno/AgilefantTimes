using AgilefantTimes.API.Agilefant.Common;
using AgilefantTimes.API.Agilefant.Task;
using Newtonsoft.Json;

namespace AgilefantTimes.API.Agilefant.Story
{
    public class AgilefantStory : AgilefantBase
    {
        [JsonProperty("description")]
        public string Description { get; private set; }
        [JsonProperty("effortSpent")]
        public int EffortSpent { get; private set; }
        [JsonProperty("highestPoints")]
        public string HighestPoints { get; private set; }
        [JsonProperty("iteration")]
        public AgilefantSprintSummary Iteration { get; private set; }
        [JsonProperty("labels")]
        public object[] Labels { get; private set; }
        [JsonProperty("metrics")]
        public AgilefantStoryMetrics Metrics { get; private set; }
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("parent")]
        public object Parent { get; private set; }
        [JsonProperty("rank")]
        public int Rank { get; private set; }
        [JsonProperty("responsibles")]
        public AgilefantResponsible[] AgilefantResponsibles { get; private set; }
        [JsonProperty("state")]
        public string State { get; private set; }
        [JsonProperty("storyPoints")]
        public int StoryPoints { get; private set; }
        [JsonProperty("storyValue")]
        public object StoryValue { get; private set; }
        [JsonProperty("tasks")]
        public AgilefantTask[] Tasks { get; private set; }
        [JsonProperty("treeRank")]
        public int TreeRank { get; private set; }
        [JsonProperty("workQueueRank")]
        public object WorkQueueRank { get; private set; }
    }
}
