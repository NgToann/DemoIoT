using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using Windows.Data.Json;
using System.Net.Http;
using System.Linq;
using System.Text;
using MQTTnet.Client.Options;
using MQTTnet;
using System.Threading;
using MQTTnet.Client;

namespace App1
{
    public class Handler
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://34.87.20.124")
        };
        private static string CREDENTIAL_FILE = "credential.json";
        private string PUT_MQTT_CREDENTIAL = "api/devices/credentials";
        public IMqttClient mqttClient;


        // TODO: get macAddress and uuid from hardware
        string macAddress = "9d:38:56:bd:f9:47";
        string uuid = "c88262bf-2a9a-46b9-8b21-7c6b0c0c49f5";

        public async Task MQTTHandler()
        {
            try
            {
                // 1. Get Device Credentials from flask disk
                var mqttCredential = await GetCredentials();
                //   1.1. Call Api to get MQTT credentials if no credentials found
                if (mqttCredential == null)
                {
                    var device = new DeviceModel
                    {
                        macAddress = macAddress,
                        uuid = uuid
                    };
                    mqttCredential = await PutCredentials(device);
                }
                if (mqttCredential == null)
                {
                    throw new Exception("Can not get MQTT credential");
                }
                //   1.2. Store credential to flask disk
                await SetCredentials(mqttCredential);
                // 2. Connect to MQTT broker
                var options = new MqttClientOptionsBuilder()
                                .WithTcpServer(mqttCredential.endpoint, mqttCredential.port)
                                .WithClientId(mqttCredential.clientId)
                                .WithCredentials(mqttCredential.username, mqttCredential.password)
                                .WithCleanSession(false)
                                .Build();
                if (mqttClient == null)
                {
                    mqttClient = new MqttFactory().CreateMqttClient();
                }
                await mqttClient.ConnectAsync(options, CancellationToken.None);
                // 3. Get Device Information
                // 4. Report Device information
                // 5. Get device status
                // 6. Report Device status
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
        public async Task<MQTTCredential> GetCredentials()
        {
            var folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sFile = null;
            var files = await folder.GetFilesAsync();
            if (!files.Any())
            {
                return null;
            }
            var file = files.FirstOrDefault(x => x.Name == CREDENTIAL_FILE);
            if (file == null)
            {
                return null;
            }
            //get mqttcredential from file
            var contentFromFile = await Windows.Storage.FileIO.ReadTextAsync(file);
            if (!string.IsNullOrEmpty(contentFromFile))
            {
                return null;
            }
            var mqttCredential = JsonConvert.DeserializeObject<MQTTCredential>(contentFromFile);
            return mqttCredential;
        }
        public async Task<MQTTCredential> PutCredentials(DeviceModel device)
        {
            if (device == null)
            {
                return null;
            }
            var content = new StringContent(JsonConvert.SerializeObject(device), Encoding.UTF8, "application/json");

            var response = await client.PutAsync(PUT_MQTT_CREDENTIAL, content);
            if (response == null || !response.IsSuccessStatusCode)
            {
                return null;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var mqttCredential = JsonConvert.DeserializeObject<MQTTCredential>(responseString);
            return mqttCredential;

        }

        public async Task SetCredentials(MQTTCredential mqttCredential)
        {
            var folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var storeCredential = await folder.CreateFileAsync(CREDENTIAL_FILE, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(storeCredential, JsonConvert.SerializeObject(mqttCredential));
        }
    }
}