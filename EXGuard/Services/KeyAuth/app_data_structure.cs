using System.Runtime.Serialization;

namespace KeyAuth
{
    [DataContract]
    public class app_data_structure
    {
        [DataMember]
        public string numUsers { get; set; }
        [DataMember]
        public string numOnlineUsers { get; set; }
        [DataMember]
        public string numKeys { get; set; }
        [DataMember]
        public string version { get; set; }
        [DataMember]
        public string customerPanelLink { get; set; }
        [DataMember]
        public string downloadLink { get; set; }
    }
}
