using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using AgilefantTimes.API.Common;

namespace AgilefantTimes.API
{
    [DataContract]
    public class AgilefantUser
    {
        public static AgilefantUser[] GetAgilefantUsers(ref CookieContainer sessionCookies)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create("http://agilefant.cosc.canterbury.ac.nz:8080/agilefant302/ajax/userChooserData.action");
            webRequest.AllowAutoRedirect = true;
            webRequest.CookieContainer = sessionCookies;
            webRequest.SetPostData("");
            string jsonData = null;
            using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                sessionCookies = webRequest.CookieContainer;
                var stream = webResponse.GetResponseStream();
                var streamReader = new StreamReader(stream);
                jsonData = streamReader.ReadToEnd();
                streamReader.Close();
            }
            return JsonToAgilefantUsers(jsonData);
        }

        private static AgilefantUser[] JsonToAgilefantUsers(string json)
        {
            json = "{\"Users\":" + json + "}";
            var stream = new MemoryStream();
            stream.Write(Encoding.UTF8.GetBytes(json), 0, json.Length);
            stream.Position = 0;

            var serializer = new DataContractJsonSerializer(typeof(AgilefantUserJsonRoot));
            var jsonRoot = (AgilefantUserJsonRoot)serializer.ReadObject(stream);

            return jsonRoot.Users.Select(afUser => afUser.OriginalObject).ToArray();
        }

        [DataMember(Name = "admin")]
        public bool Admin { get; protected set; }
        [DataMember(Name = "autoassignToStories")]
        public bool AutoassignToStories { get; protected set; }
        [DataMember(Name = "autoassignToTasks")]
        public bool AutoassignToTasks { get; protected set; }
        [DataMember(Name = "class")]
        public string InternalClass { get; protected set; }
        [DataMember(Name = "email")]
        public string Email { get; protected set; }
        [DataMember(Name = "enabled")]
        public bool Enabled { get; protected set; }
        [DataMember(Name = "fullName")]
        public string FullName { get; protected set; }
        [DataMember(Name = "id")]
        public int Id { get; protected set; }
        [DataMember(Name = "initials")]
        public string Initials { get; protected set; }
        [DataMember(Name = "loginName")]
        public string LoginName { get; protected set; }
        [DataMember(Name = "markStoryStarted")]
        public string MarkStoryStarted { get; protected set; }
        [DataMember(Name = "name")]
        public string Name { get; protected set; }
        [DataMember(Name = "recentItemsNumberOfWeeks")]
        public int RecentItemsNumberOfWeeks { get; protected set; }
        [DataMember(Name = "weekEffort")]
        public object WeekEffort { get; protected set; }

        [DataContract]
        protected class AgilefantUserJsonRoot
        {
            [DataMember]
            public AgilefantUserWrapper[] Users { get; protected set; }
        }

        [DataContract]
        protected class AgilefantUserWrapper
        {
            [DataMember(Name = "baseClassName")]
            public string BaseClassName { get; protected set; }
            [DataMember(Name = "enabled")]
            public bool Enabled { get; protected set; }
            [DataMember(Name = "id")]
            public int Id { get; protected set; }
            [DataMember(Name = "idList")]
            public object IdList { get; protected set; }
            [DataMember(Name = "matchedString")]
            public string MatchedString { get; protected set; }
            [DataMember(Name = "name")]
            public string Name { get; protected set; }
            [DataMember(Name = "originalObject")]
            public AgilefantUser OriginalObject { get; protected set; }
        }
    }
}


