using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace KeyAuth
{
    [DataContract]
    public class response_structure
    {
        [DataMember]
        public bool success { get; set; }

        [DataMember]
        public string sessionid { get; set; }

        [DataMember]
        public string contents { get; set; }

        [DataMember]
        public string response { get; set; }

        [DataMember]
        public string message { get; set; }

        [DataMember]
        public string download { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public user_data_structure info { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public app_data_structure appinfo { get; set; }

        [DataMember]
        public List<msg> messages { get; set; }

        [DataMember]
        public List<users> users { get; set; }
    }
}
