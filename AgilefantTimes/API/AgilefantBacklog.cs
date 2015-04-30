using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using AgilefantTimes.API.Common;

namespace AgilefantTimes.API
{
    [DataContract]
    public class AgilefantBacklog
    {
        public static AgilefantBacklog[] GetAgilefantBacklogs(int teamNumber, ref CookieContainer sessionCookies)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create("http://agilefant.cosc.canterbury.ac.nz:8080/agilefant302/ajax/retrieveSubBacklogs.action");
            webRequest.AllowAutoRedirect = true;
            webRequest.CookieContainer = sessionCookies;
            webRequest.SetPostData("backlogId=" + teamNumber);
            string jsonData = null;
            using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                sessionCookies = webRequest.CookieContainer;
                var stream = webResponse.GetResponseStream();
                var streamReader = new StreamReader(stream);
                jsonData = streamReader.ReadToEnd();
                streamReader.Close();
            }
            return JsonToAgilefantBacklogs(jsonData);
        }

        private static AgilefantBacklog[] JsonToAgilefantBacklogs(string json)
        {
            json = "{\"Backlogs\":" + json + "}";
            var stream = new MemoryStream();
            stream.Write(Encoding.UTF8.GetBytes(json), 0, json.Length);
            stream.Position = 0;

            var serializer = new DataContractJsonSerializer(typeof(AgilefantBacklogJsonRoot));
            var jsonRoot = (AgilefantBacklogJsonRoot)serializer.ReadObject(stream);

            return jsonRoot.Backlogs;
        }

        [DataContract]
        protected class AgilefantBacklogJsonRoot
        {
            [DataMember]
            public AgilefantBacklog[] Backlogs { get; protected set; }
        }

        [DataContract]
        public class AgilefantBacklogProductSummary
        {
            [DataMember(Name = "class")]
            public string InternalClass { get; set; }
            [DataMember(Name = "description")]
            public string Description { get; set; }
            [DataMember(Name = "id")]
            public int Id { get; set; }
            [DataMember(Name = "name")]
            public string Name { get; set; }
            [DataMember(Name = "product")]
            public bool Product { get; set; }
            [DataMember(Name = "standAlone")]
            public bool StandAlone { get; set; }
        }

        [DataMember(Name = "backlogSize")]
        public object BacklogSize { get; set; }
        [DataMember(Name = "baselineLoad")]
        public int BaselineLoad { get; set; }
        [DataMember(Name = "class")]
        public string InternalClass { get; set; }
        [DataMember(Name = "description")]
        public string Description { get; set; }
        [DataMember(Name = "endDate")]
        public long EndDate { get; set; }
        [DataMember(Name = "id")]
        public int Id { get; set; }
        [DataMember(Name = "name")]
        public string Name { get; set; }
        [DataMember(Name = "product")]
        public bool Product { get; set; }
        [DataMember(Name = "rank")]
        public int Rank { get; set; }
        [DataMember(Name = "root")]
        public AgilefantBacklogProductSummary Root { get; set; }
        [DataMember(Name = "standAlone")]
        public bool StandAlone { get; set; }
        [DataMember(Name = "startDate")]
        public long StartDate { get; set; }
        [DataMember(Name = "status")]
        public string Status { get; set; }
    }
}
