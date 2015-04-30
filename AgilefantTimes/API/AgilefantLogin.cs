using System;
using System.IO;
using System.Net;
using AgilefantTimes.API.Common;

namespace AgilefantTimes.API
{
    public static class AgilefantLogin
    {
        public static CookieContainer PerformLogin(string username, string password)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create("http://agilefant.cosc.canterbury.ac.nz:8080/agilefant302/j_spring_security_check");
            webRequest.AllowAutoRedirect = true;
            webRequest.CookieContainer = new CookieContainer();
            webRequest.SetPostData("j_username=" + username + "&j_password=" + password);
            webRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            CookieContainer cookieContainer = null;
            using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                var stream = webResponse.GetResponseStream();
                var streamReader = new StreamReader(stream);
                if (streamReader.ReadToEnd().Contains("Invalid username or password, please try again."))
                {
                    throw new Exception("Invalid username or password, please try again.");
                }
                cookieContainer = webRequest.CookieContainer;
            }
            return cookieContainer;
        }

    }
}
