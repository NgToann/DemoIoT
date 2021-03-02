using Newtonsoft.Json;
using System;
using System.IO;
using System.Management;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static HttpClient client;
        static MQTTCredential mQTTCredential;
        static string API_ENDPOINT = "http://34.87.20.124";
        static async Task Main(string[] args)
        {
            // 
            //client.BaseAddress = new Uri("http://34.87.20.124/");
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(
            //    new MediaTypeWithQualityHeaderValue("application/json"));
            //var x = PutDeviceModel("api/devices/credentials");

            string macAddress = "";
            var nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface nic in nics)
            {
                // Only consider Ethernet network interfaces, thereby ignoring any
                // loopback devices etc.
                if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet) continue;
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    //macAddress = ByteArrayToString(nic.GetPhysicalAddress().GetAddressBytes());
                    macAddress = BitConverter.ToString(nic.GetPhysicalAddress().GetAddressBytes());
                    break;
                }
            }
            Console.WriteLine(macAddress);

            // STEP 1: get mqtt credentials if not available
            // STEP 2: connect to MQTT broker with this credential
            // STEP 3: report device information by publish the topic : bms/<Device_ID>/info
            // STEP 4: report device status by publish the topic : bms/<DEVICE_ID>/state
            client = new HttpClient
            {
                BaseAddress = new Uri(API_ENDPOINT)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var device = new DeviceModel
            {
                macAddress = "9d:38:56:bd:f9:47",
                uuid = "c88262bf-2a9a-46b9-8b21-7c6b0c0c49f5"
            };


            //var mbs = new ManagementObjectSearcher("Select ProcessorId From Win32_processor");
            //ManagementObjectCollection mbsList = mbs.Get();
            //string id = "";
            //foreach (ManagementObject mo in mbsList)
            //{
            //    id = mo["ProcessorId"].ToString();
            //    break;
            //}




            mQTTCredential = await GetMQTTCredentials(device);
            Console.WriteLine(JsonConvert.SerializeObject(mQTTCredential));

            Console.ReadLine();
        }

        //private static string GetId()
        //{
            //if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.System.Profile.HardwareIdentification"))
            //{
            //    var token = HardwareIdentification.GetPackageSpecificToken(null);
            //    var hardwareId = token.Id;
            //    var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(hardwareId);

            //    byte[] bytes = new byte[hardwareId.Length];
            //    dataReader.ReadBytes(bytes);

            //    return BitConverter.ToString(bytes).Replace("-", "");
            //}

            //throw new Exception("NO API FOR DEVICE ID PRESENT!");
        //}

        public class DeviceModel
        {
            public string macAddress { get; set; }
            public string uuid { get; set; }
        }
        /*
         {
            "endpoint": "34.87.20.124",
            "port": 1883,
            "protocol": "mqtt",
            "password": "7qLp7Tzp8XORNOe",
            "token": "7qLp7Tzp8XORNOe",
            "username": "c88262bf-2a9a-46b9-8b21-7c6b0c0c49f5",
            "clientId": "c88262bf-2a9a-46b9-8b21-7c6b0c0c49f5"
        }
         */
        public class MQTTCredential
        {
            public string endpoint { get; set; }
            public int port { get; set; }
            public string protocol { get; set; }
            public string password { get; set; }
            public string token { get; set; }
            public string username { get; set; }
            public string clientId { get; set; }
        }
        static async Task<DeviceModel> PutDeviceModel(string path)
        {
            var deviceTest = new DeviceModel
            {
                macAddress = "9d:38:56:bd:f9:47",
                uuid = "c88262bf-2a9a-46b9-8b21-7c6b0c0c49f5"
            };
            var bodyTest = JsonConvert.SerializeObject(deviceTest);
            HttpContent content = new StringContent(bodyTest, Encoding.UTF8, "application/json");
            var x = client.PostAsync("http://34.87.20.124/api/devices/credentials", content);

            DeviceModel device = null;
            HttpResponseMessage getResponse = await client.GetAsync(path);
            if (getResponse.IsSuccessStatusCode)
            {
                var result = await getResponse.Content.ReadAsAsync<MQTTCredential>();
            }
            return device;
        }

        public static async Task<MQTTCredential> GetMQTTCredentials(DeviceModel device)
        {
            if (device == null)
            {
                return null;
            }
            var body = JsonConvert.SerializeObject(device);
            HttpContent content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PutAsync("api/devices/credentials", content);

            if (response.IsSuccessStatusCode)
            {
                //var result = await response.Content.ReadAsAsync<MQTTCredential>();
                var resultString = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<MQTTCredential>(resultString);
                return result ;
            }
            return null;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            for (int i = 0; i < ba.Length; i++)
            {
                hex.AppendFormat("{0:x2}", ba[i]);
                if (i < ba.Length - 1)
                    hex.Append(':');
            }
            return hex.ToString().ToUpper();
        }
    }
}
