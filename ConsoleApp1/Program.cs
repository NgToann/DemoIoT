using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static HttpClient client = new HttpClient();
        static void Main(string[] args)
        {
            // 
            client.BaseAddress = new Uri("http://34.87.20.124/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            var x = PutDeviceModel("api/devices/credentials");

            string macAddress = "";
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Only consider Ethernet network interfaces, thereby ignoring any
                // loopback devices etc.
                if (nic.NetworkInterfaceType != NetworkInterfaceType.Ethernet) continue;
                if (nic.OperationalStatus == OperationalStatus.Up)
                {
                    macAddress = ByteArrayToString(nic.GetPhysicalAddress().GetAddressBytes());
                    break;
                }
            }
            Console.WriteLine(macAddress);
        }
        public class DeviceModel
        {
            public string macAddress { get; set; }
            public string uuid { get; set; }
        }
        static async Task<DeviceModel> PutDeviceModel(string path)
        {
            var deviceTest = new DeviceModel
            {
                macAddress = "9d:38:56:bd:f9:47",
                uuid = "c88262bf-2a9a-46b9-8b21-7c6b0c0c49f5"
            };
            var bodyTest = JsonConvert.SerializeObject(deviceTest);
            HttpResponseMessage response = await client.PutAsJsonAsync(
                path, bodyTest);
            response.EnsureSuccessStatusCode();

            DeviceModel device = null;
            HttpResponseMessage getResponse = await client.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                device = await getResponse.Content.ReadAsAsync<DeviceModel>();
            }
            return device;
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
