#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

#endregion

namespace AgilefantTimes.API.Agilefant
{
    /// <summary>
    /// Represents an Agilefant Session
    /// </summary>
    public class AgilefantSession
    {
        private const string LoginUrl = "http://agilefant.cosc.canterbury.ac.nz:8080/agilefant302/j_spring_security_check";
        private const string AgilefantUrl = "http://agilefant.cosc.canterbury.ac.nz:8080/agilefant302/";

        private HttpClient _httpClient;
        private bool _loggedIn;
        private string _username;
        private string _password;

        private AgilefantSession(HttpClientHandler handler, string username, string password)
        {
            _httpClient = new HttpClient(handler);
            _loggedIn = true;
            _username = username;
            _password = password;
        }

        /// <summary>
        /// Gets a response from agilefant
        /// </summary>
        /// <param name="query">The query. This is appended to http://agilefant.cosc.canterbury.ac.nz:8080/agilefant302/</param>
        /// <returns>The response</returns>
        public Task<HttpResponseMessage> Get(string query)
        {
            Console.WriteLine("1");
            EnsureLoggedIn();
            Console.WriteLine("2");
            return _httpClient.GetAsync(AgilefantUrl + query);
        }

        /// <summary>
        /// Posts some httpcontent to Agilefant
        /// </summary>
        /// <param name="query">The query. This is appended to http://agilefant.cosc.canterbury.ac.nz:8080/agilefant302/</param>
        /// <param name="content">The content to post</param>
        /// <returns>The response</returns>
        public Task<HttpResponseMessage> Post(string query, HttpContent content)
        {
            EnsureLoggedIn();
            return _httpClient.PostAsync(AgilefantUrl + query, content);
        }

        /// <summary>
        /// Posts a blank string to the specified url. Useful
        /// for empty post request
        /// </summary>
        /// <param name="query">The query</param>
        /// <returns>The url</returns>
        public Task<HttpResponseMessage> Post(string query)
        {
            return Post(query, new StringContent(""));
        }

        #region Login

        /// <summary>
        /// Logs in and creates a new Agilefant session.
        /// </summary>
        /// <param name="username">The username to log in with.</param>
        /// <param name="password">The password to log in with.</param>
        /// <exception cref="SecurityException">If credentials are incorrect.</exception>
        /// <exception cref="WebException">If there was an error connecting to Agilefant.</exception>
        /// <returns>A new Agilefant session.</returns>
        public static async Task<AgilefantSession> Login(string username, string password)
        {
            try
            {
                var handler = await InternalLogin(username, password);
                return new AgilefantSession(handler, username, password);
            }
            catch (Exception e)
            {
                Console.WriteLine("Login Exception:\n" + e.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// Logs in to Agilefant.
        /// </summary>
        /// <param name="username">The username to log in with.</param>
        /// <param name="password">The password to log in with.</param>
        /// <exception cref="SecurityException">If credentials are incorrect.</exception>
        /// <exception cref="WebException">If there was an error connecting to Agilefant.</exception>
        /// <exception cref="InvalidOperationException">If currently logged in.</exception>
        public void ReLogin(string username, string password)
        {
            if (_loggedIn) throw new InvalidOperationException("Cannot login while not logged out.");
            var handler = InternalLogin(username, password).Result;
            _httpClient = new HttpClient(handler);
            _loggedIn = true;
            _username = username;
            _password = password;
        }

        /// <summary>
        /// Logs in to Agilefant using the same credentials as the last login.
        /// </summary>
        /// <exception cref="SecurityException">If credentials are incorrect.</exception>
        /// <exception cref="WebException">If there was an error connecting to Agilefant.</exception>
        /// <exception cref="InvalidOperationException">If currently logged in.</exception>
        public void ReLogin()
        {
            ReLogin(_username, _password);
        }

        /// <summary>
        /// Performs the internal operations of the login functionality.
        /// </summary>
        /// <param name="username">Username to login with.</param>
        /// <param name="password">Password to login with.</param>
        /// <returns>Handle to the login session.</returns>
        private static async Task<HttpClientHandler> InternalLogin(string username, string password)
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    UseCookies = true
                };
                var client = new HttpClient(handler);
                var data = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    {"j_username", username},
                    {"j_password", password}
                });

                Console.WriteLine("Got here");
                var response = client.PostAsync(LoginUrl, data).Result;
                
                //Will throw an exception if the request failed
                Console.WriteLine("Got here");
                Console.WriteLine(response.StatusCode);
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                if (content.Contains("Invalid username or password, please try again."))
                {
                    throw new SecurityException("Invalid username or password, please try again.");
                }
                return handler;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("InternalLogin Exception:\n" + e.StackTrace);
                return null;
            }

            
        }

        /// <summary>
        /// Logs the current session out.
        /// </summary>
        public void Logout()
        {
            var response = Get("j_spring_security_logout?exit=Logout").Result;
            response.EnsureSuccessStatusCode();
            _loggedIn = false;
        }

        /// <summary>
        /// Ensures that the current user is logged in.
        /// </summary>
        private void EnsureLoggedIn()
        {
            return; // todo
            if (!_loggedIn) throw new SecurityException("User is not logged in.");
        }

        #endregion
    }

    
}