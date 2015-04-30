using System.Net;
using System.Text;

namespace AgilefantTimes.API.Common
{
    public static class Extensions
    {
        public static void SetPostData(this HttpWebRequest request, string data)
        {
            request.Method = "POST";
            var st = request.GetRequestStream();
            var byteArray = Encoding.UTF8.GetBytes(data);
            st.Write(byteArray, 0, byteArray.Length);
            st.Close();
            request.ContentType = "application/x-www-form-urlencoded";
        }
    }
}
