#region

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

#endregion

namespace AgilefantTimes
{
    [DataContract]
    public class Config
    {
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public int TeamNumber { get; set; }

        [DataMember]
        public int SprintNumber { get; set; }

        public static Config Load(string location)
        {
            if (!File.Exists(location))
            {
                throw new Exception("Missing configuration file at " + location);
            }
            var serializer = new DataContractJsonSerializer(typeof (Config));
            var fileSteam = new FileStream(location, FileMode.Open);
            var config = serializer.ReadObject(fileSteam);
            fileSteam.Close();
            return (Config) config;
        }
    }
}