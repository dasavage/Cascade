using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Cascade.Core.Cascade
{
    public static class Utility
    {
        public static string UpdateAvalible()
        {
            const string urlUpdateApiFile = "https://raw.githubusercontent.com/dasavage/Cascade/master/latest.version";

            if (!UrlReachable(urlUpdateApiFile))
            {
                return "";
            }

            var client = new WebClient();
            return client.DownloadString(urlUpdateApiFile);
        }

        public static bool ValidVersionString(string versionString)
        {
            return versionString.Contains(".") && IsNumber(versionString.Replace(".", ""));
        }

        private static bool IsNumber(this string aNumber)
        {
            int n;
            var isNumeric = int.TryParse("123", out n);
            return isNumeric;
        }

        private static bool UrlReachable(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 15000;
            request.Method = "HEAD"; // As per Lasse's comment
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (WebException)
            {
                return false;
            }
        }
    }
}
