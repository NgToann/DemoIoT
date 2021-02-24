using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App1
{
    public class DeviceModel
    {
        public string uuid { get; set; }
        public string macAddress { get; set; }
    }
    public class MQTTCredential
    {
        public string endpoint { get; set; }
        public int port { get; set; }
        public string protocol { get; set; }
        public string clientId { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
}
