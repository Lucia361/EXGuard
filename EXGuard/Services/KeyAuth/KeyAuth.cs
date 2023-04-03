using System;
using System.IO;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Net.Security;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;

namespace KeyAuth
{
    public class api
    {
        private json_wrapper response_decoder = new json_wrapper(new response_structure());

        public string name, ownerid, secret, version;
        public static long responseTime;
        private static string sessionid, enckey;
        bool initialized;

        /// <summary>
        /// Set up your application credentials in order to use keyauth
        /// </summary>
        /// <param name="name">Application Name</param>
        /// <param name="ownerid">Your OwnerID, found in your account settings.</param>
        /// <param name="secret">Application Secret</param>
        /// <param name="version">Application Version, if version doesnt match it will open the download link you set up in your application settings and close the app, if empty the app will close</param>
        public api(string name, string ownerid, string secret, string version)
        {
            if (ownerid.Length != 10 || secret.Length != 64)
            {
                error("Application not setup correctly. Please watch video link found in Program.cs");

                Environment.Exit(0);
            }

            this.name = name;
            this.ownerid = ownerid;
            this.secret = secret;
            this.version = version;
        }

        #region app_data
        public app_data_class app_data = new app_data_class();

        private void load_app_data(app_data_structure data)
        {
            app_data.numUsers = data.numUsers;
            app_data.numOnlineUsers = data.numOnlineUsers;
            app_data.numKeys = data.numKeys;
            app_data.version = data.version;
            app_data.customerPanelLink = data.customerPanelLink;
        }
        #endregion

        #region user_data
        public user_data_class user_data = new user_data_class();

        private void load_user_data(user_data_structure data)
        {
            user_data.username = data.username;
            user_data.ip = data.ip;
            user_data.hwid = data.hwid;
            user_data.createdate = data.createdate;
            user_data.lastlogin = data.lastlogin;
            user_data.subscriptions = data.subscriptions; // array of subscriptions (basically multiple user ranks for user with individual expiry dates 
        }
        #endregion

        #region response_struct
        public response_class response = new response_class();

        private void load_response_struct(response_structure data)
        {
            response.success = data.success;
            response.message = data.message;
        }
        #endregion

        /// <summary>
        /// Initializes the connection with keyauth in order to use any of the functions
        /// </summary>
        public void init()
        {
            string sentKey = encryption.iv_key();
            enckey = sentKey + "-" + secret;
            var values_to_upload = new NameValueCollection
            {
                ["type"] = "init",
                ["ver"] = version,
                ["hash"] = checksum(Process.GetCurrentProcess().MainModule.FileName),
                ["enckey"] = sentKey,
                ["name"] = name,
                ["ownerid"] = ownerid
            };

            var response = req(values_to_upload);

            if (response == "KeyAuth_Invalid")
            {
                error("Application not found");

                Environment.Exit(0);
            }

            var json = response_decoder.string_to_generic<response_structure>(response);
            load_response_struct(json);
            if (json.success)
            {
                load_app_data(json.appinfo);
                sessionid = json.sessionid;
                initialized = true;
            }
            else if (json.message == "invalidver")
            {
                app_data.downloadLink = json.download;
            }

        }

        /// <summary>
        /// Authenticates the user using their username and password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="pass">Password</param>
        public void login(string username, string pass)
        {
            if (!initialized)
            {
                error("You must run the function KeyAuthApp.init(); first");
                Environment.Exit(0);
            }

            string hwid = "S-1-5-21-517695572-1913475359-2603254930-500";

            var values_to_upload = new NameValueCollection
            {
                ["type"] = "login",
                ["username"] = username,
                ["pass"] = pass,
                ["hwid"] = hwid,
                ["sessionid"] = sessionid,
                ["name"] = name,
                ["ownerid"] = ownerid
            };

            var response = req(values_to_upload);

            var json = response_decoder.string_to_generic<response_structure>(response);
            load_response_struct(json);

            if (json.success)
                load_user_data(json.info);
        }

        public static string checksum(string filename)
        {
            string result;

            using (MD5 md = MD5.Create())
            {
                using (FileStream fileStream = File.OpenRead(filename))
                {
                    byte[] value = md.ComputeHash(fileStream);
                    result = BitConverter.ToString(value).Replace("-", "").ToLowerInvariant();
                }
            }
            return result;
        }

        public static void error(string message)
        {
            Process.Start(new ProcessStartInfo("cmd.exe", $"/c start cmd /C \"color b && title Error && echo {message} && timeout /t 5\"")
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            });

            Environment.Exit(0);
        }

        private static string req(NameValueCollection post_data)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.Proxy = null;

                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    var raw_response = client.UploadValues("https://keyauth.win/api/1.2/", post_data);

                    stopwatch.Stop();
                    responseTime = stopwatch.ElapsedMilliseconds;

                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                    sigCheck(Encoding.Default.GetString(raw_response), client.ResponseHeaders["signature"], post_data.Get(0));

                    return Encoding.Default.GetString(raw_response);
                }
            }
            catch (WebException webex)
            {
                var response = (HttpWebResponse)webex.Response;
                switch (response.StatusCode)
                {
                    case (HttpStatusCode)429: // client hit our rate limit
                        error("You're connecting too fast to loader, slow down.");
                        Environment.Exit(0);
                        return "";
                    default: // site won't resolve. you should use keyauth.uk domain since it's not blocked by any ISPs
                        error("Connection failure. Please try again, or contact us for help.");
                        Environment.Exit(0);
                        return "";
                }
            }
        }

        private static void sigCheck(string resp, string signature, string type)
        {
            if (type == "log") // log doesn't return a response.
            {
                return;
            }

            try
            {
                string clientComputed = encryption.HashHMAC((type == "init") ? enckey.Substring(17, 64) : enckey, resp);
                if (clientComputed != signature)
                {
                    error("Signature check fail. Try to run the program again, your session may have expired.");

                    Environment.Exit(0);
                }
            }
            catch
            {
                error("Signature check fail. Try to run the program again, your session may have expired.");

                Environment.Exit(0);
            }
        }
    }
}
