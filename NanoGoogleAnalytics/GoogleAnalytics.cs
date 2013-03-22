using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace NanoGoogleAnalytics
{
    /// <summary>
    /// https://developers.google.com/analytics/devguides/collection/other/mobileWebsites
    /// </summary>
    public static class GoogleAnalytics
    {
        /// <summary>
        /// Send a tracking request to Google Analytics.
        /// </summary>
        public static void SendTrackingRequest(string pageName, string trackingId = "UA-XXXXXX-XX", string referrer = null)
        {
            string visitorId = AnonymousUserId;
            referrer = referrer ?? "/" + AppVersion;

            string utmGifLocation = "http://www.google-analytics.com/__utm.gif";

            var x = string.Format("{0}?utmwv=4.4sa&utmn={1}&utmp=/{2}&utmac={3}&utmcc=__utma%3D999.999.999.999.999.1%3B&utmvid={4}&utmr={5}",
                utmGifLocation, (new Random()).Next(Int32.MaxValue), pageName,
                trackingId, visitorId, referrer);

            Submit(x);
        }

        static string userId = null;
        public static string AppVersion = "000_aaa";

        /// <summary>
        /// Anonymized, consistent unique idenitifer for the user
        /// </summary>
        private static string AnonymousUserId
        {
            get
            {
                if (userId == null)
                {
                    string computer = "?";
                    string login = "?";
                    try
                    {
                        computer = Environment.MachineName;
                        login = Environment.UserName;
                    }
                    catch { }

                    using (MD5 md5 = new MD5CryptoServiceProvider())
                    {
                        byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(computer + login));
                        userId = BitConverter.ToString(hash).Replace("-", "");
                    }
                }
                return userId;
            }
        }

        static void Submit(string url)
        {
            try
            {
                WebClient wc = new CustomWC(false, 30000);
                wc.Proxy = WebRequest.GetSystemWebProxy();
                wc.DownloadStringCompleted += wc_DownloadStringCompleted;
                wc.DownloadStringAsync(new Uri(url));
            }
            catch { }
        }

        static void wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // do nothing
        }
    }

    public class CustomWC : WebClient
    {
        public CustomWC(bool keepalive = true, int timeout = -1)
        {
            this.keepalive = keepalive;
            this.timeout = timeout;
        }
        protected override WebRequest GetWebRequest(Uri address)
        {
            var br = base.GetWebRequest(address);
            ((HttpWebRequest)br).KeepAlive = keepalive;
            if (timeout > 0)
            {
                br.Timeout = timeout;
            }
            return br;
        }

        public bool keepalive { get; set; }

        public int timeout { get; set; }
    }
}