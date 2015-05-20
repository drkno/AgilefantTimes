using Newtonsoft.Json;

namespace AgilefantTimes.API.Agilefant.Common
{
    public abstract class AgilefantBase
    {
        //[JsonProperty("class")]
        //private string InternalClass { get; set; }

        [JsonProperty("id")]
        public int Id { get; protected set; }
    }
}
