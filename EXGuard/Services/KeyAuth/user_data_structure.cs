using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KeyAuth
{
    [DataContract]
    public class user_data_structure
    {
        [DataMember]
        public string username { get; set; }

        [DataMember]
        public string ip { get; set; }
        [DataMember]
        public string hwid { get; set; }
        [DataMember]
        public string createdate { get; set; }
        [DataMember]
        public string lastlogin { get; set; }
        [DataMember]
        public List<Data> subscriptions { get; set; } // array of subscriptions (basically multiple user ranks for user with individual expiry dates
    }
}
