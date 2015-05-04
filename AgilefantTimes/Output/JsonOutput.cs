using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// Creates a new JSON output object, used for outputing data as JSON.
        /// </summary>
        /// <param name="name">TeamName to use.</param>
        /// <param name="sprintName">SprintName to use.</param>
        /// <param name="hours">Hours to use.</param>
        public JsonOutput(string name, string sprintName, List<JsonOutputTime> hours)
        {
            TeamName = name;
            SprintName = sprintName;
            Hours = hours;
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
