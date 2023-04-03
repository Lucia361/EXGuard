using System;
using System.Collections.Generic;

namespace KeyAuth
{
    public class user_data_class
    {
        public string username { get; set; }
        public string ip { get; set; }
        public string hwid { get; set; }
        public string createdate { get; set; }
        public string lastlogin { get; set; }
        public List<Data> subscriptions { get; set; } // array of subscriptions (basically multiple user ranks for user with individual expiry dates
    }
}
