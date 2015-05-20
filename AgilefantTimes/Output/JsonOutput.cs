using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace AgilefantTimes.Output
{
    [DataContract]
    public class JsonOutput
    {
        [DataMember]
        public string TeamName { get; private set; }
        [DataMember]
        public string SprintName { get; private set; }
        [DataMember]
        public List<JsonOutputTime> Hours { get; private set; }
        [DataMember]
        public int MaxHours { get; private set; }
        [DataMember]
        public int MinHours { get; private set; }
        [DataMember]
        public int MedianLowerHours { get; private set; }
        [DataMember]
        public int MedianUpperHours { get; private set; }
        [DataMember]
        public double AverageHours { get; private set; }
        [DataMember]
        public double AverageStoryHours { get; private set; }
        [DataMember]
        public double AverageTaskHours { get; private set; }
        [DataMember]
        public double TotalHours { get; private set; }
        [DataMember]
        public int SprintNumber { get; private set; }

        /// <summary>
        /// Creates a new JSON output object, used for outputing data as JSON.
        /// </summary>
        /// <param name="name">TeamName to use.</param>
        /// <param name="sprintName">SprintName to use.</param>
        /// <param name="hours">Hours to use.</param>
        /// <param name="sprintNumber">Sprint number.</param>
        public JsonOutput(string name, string sprintName, List<JsonOutputTime> hours, int sprintNumber)
        {
            TeamName = name;
            SprintName = sprintName;
            Hours = hours;
            SprintNumber = sprintNumber;

            var sorted = hours.OrderBy(h => h.TotalHours);
            MinHours = hours.IndexOf(sorted.First());
            MaxHours = hours.IndexOf(sorted.Last());
            AverageHours = hours.Average(h => h.TotalHours);
            AverageStoryHours = hours.Average(h => h.StoryHours);
            AverageTaskHours = hours.Average(h => h.TaskHours);
            MedianUpperHours = hours.IndexOf(sorted.ElementAt(hours.Count / 2));
            MedianLowerHours = hours.IndexOf(sorted.ElementAt(hours.Count / 2 - 1));
            TotalHours = hours.Sum(h => h.TotalHours);
        }

        /// <summary>
        /// Converts this object to JSON data.
        /// </summary>
        /// <returns>JSON data.</returns>
        public string ToJson()
        {
            var serializer = new DataContractJsonSerializer(typeof(JsonOutput));
            var memoryStream = new MemoryStream();
            serializer.WriteObject(memoryStream, this);
            memoryStream.Position = 0;
            var streamReader = new StreamReader(memoryStream);
            var result = streamReader.ReadToEnd();
            streamReader.Close();
            return result;
        }
    }
}
