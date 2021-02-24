using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Connecting;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.UI.Core;
using MQTTnet.Extensions.ManagedClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    // Use secure TCP connection.
 


    public sealed partial class MainPage : Page
    {
        static MQTTCredential mQTTCredential;
        static DeviceModel deviceDefault;
        static string API_ENDPOINT = "http://34.87.20.124";
        private static string CREDENTIAL_FILENAME = "credentiallocal.json";

        public MqttFactory factory;
        public IMqttClient mqttClient;
        DispatcherTimer timerReSend;
        static HttpClient client;
        //public MqttClientOptions options;
        //public IManagedMqttClient mqttClient = new MqttFactory().CreateManagedMqttClient();
        public MainPage()
        {
            this.InitializeComponent();
            //factory = new MqttFactory();
            //mqttClient = factory.CreateMqttClient();

            client = new HttpClient
            {
                BaseAddress = new Uri(API_ENDPOINT)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            //// should be change, when get actual macAddress and uuid
            deviceDefault = new DeviceModel
            {
                macAddress = "9d:38:56:bd:f9:47",
                uuid = "c88262bf-2a9a-46b9-8b21-7c6b0c0c49f5"
            };

            ////while (true)
            ////{
            //Debug.WriteLine("Start");

            ////MQTTSetup();

            //Debug.WriteLine("Stop");
            ////}

            //// timer loop
            //timerReSend = new DispatcherTimer();
            //timerReSend.Interval = new TimeSpan(0, 0, 10);
            //timerReSend.Tick += TimerResend_Tick;

            var x = new Handler();
            x.MQTTHandler();
            
        }
        int sendingTimes = 0;
        private void TimerResend_Tick(object sender, object e)
        {
            try
            {
                if (sendingTimes == 5) timerReSend.Stop();
                MQTTSetup();
                sendingTimes++;
            }
            catch (MQTTnet.Exceptions.MqttProtocolViolationException error_exception)
            {
                Debug.WriteLine(error_exception);
            }
        }

        
        public void submitButtonClick(object sender, RoutedEventArgs e)
        {
            //timerReSend.Start();
            MQTTSetup();
        }
        //public async Task mqttpush()
        //{
        //    var options = new ManagedMqttClientOptionsBuilder()
        //                            .WithAutoReconnectDelay(TimeSpan.FromSeconds(1))
        //                            .WithClientOptions(new MqttClientOptionsBuilder()
        //                                .WithClientId("tranlysfw")
        //                                .WithTcpServer("test.mosquitto.org", 1883)
        //                                //.WithCredentials("tranlysfw", "zxcvbnm1")
        //                                //.WithTls()
        //                                .Build())
        //                                .Build();

        //    var message = new MqttApplicationMessageBuilder()
        //                           .WithTopic("testtopic/mot")
        //                           .WithPayload("Hello World")
        //                           //.WithAtMostOnceQoS()
        //                           .WithAtLeastOnceQoS()
        //                           .Build();


        //    //await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("testtopic/mot").Build());
        //    await mqttClient.StartAsync(options);
        //    await mqttClient.PublishAsync(message);
        //    await Task.Delay(5000);
        //}


        /// <summary>
        /// Connect to broker.
        /// </summary>
        /// <returns>Task.</returns>

        public async Task<int> MQTTSetup()
        {
            await GetMQTTCredentials();

            var message = new MqttApplicationMessageBuilder()
                                     .WithTopic("testtopic/mot")
                                     .WithPayload("Hello HiveMQ !")
                                     .WithAtLeastOnceQoS()
                                     .Build();

            //var options = new MqttClientOptionsBuilder()
            //                        //.WithTcpServer("test.mosquitto.org", 1883)
            //                        .WithTcpServer("broker.hivemq.com", 1883)
            //                        //.WithClientId("tranlysfw")
            //                        .WithCredentials("tranlysfw", "zxcvbnm1")
            //                        //.WithTls()
            //                        .WithCleanSession(false)
            //                        //.WithKeepAlivePeriod(System.TimeSpan.FromSeconds(3))
            //                        //.WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V311)
            //                        .Build();

            var options = new MqttClientOptionsBuilder()
                                    //.WithTcpServer("test.mosquitto.org", 1883)
                                    .WithTcpServer("34.87.20.124", mQTTCredential.port)
                                    .WithClientId(mQTTCredential.clientId)
                                    .WithCredentials(mQTTCredential.username, mQTTCredential.password)
                                    //.WithTls()
                                    .WithCleanSession(false)
                                    //.WithKeepAlivePeriod(System.TimeSpan.FromSeconds(3))
                                    //.WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V311)
                                    .Build();

            await mqttClient.ConnectAsync(options, CancellationToken.None);


            //await mqttClient.ConnectAsync(options, CancellationToken.None);

            //message = new MqttApplicationMessageBuilder()
            //                        .WithTopic("testtopic/mot")
            //                        .WithPayload("Hello World")
            //                        .WithAtLeastOnceQoS()
            //                        .Build();

            await mqttClient.PublishAsync(message, CancellationToken.None); // Since 3.0.5 with CancellationToken         
            //await Task.Delay(TimeSpan.FromSeconds(5));
            await mqttClient.DisconnectAsync();
            return 1;
        }

        //public static async Task<MQTTCredential> GetMQTTCredentials(DeviceModel device)
        public static async Task<MQTTCredential> GetMQTTCredentials()
        {
            // 1 Check credential local is available or not.
            var storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var checkFileExist = await storageFolder.TryGetItemAsync(CREDENTIAL_FILENAME);
            if (checkFileExist != null)
            {
                var credentialLocalFile = await storageFolder.GetFileAsync(CREDENTIAL_FILENAME);
                var contentFromFile = await Windows.Storage.FileIO.ReadTextAsync(credentialLocalFile);
                if (!string.IsNullOrEmpty(contentFromFile))
                {
                    mQTTCredential = JsonConvert.DeserializeObject<MQTTCredential>(contentFromFile);
                    return mQTTCredential;
                }
            }

            // 2 if not availale. get from server, save data to storage
            var body = JsonConvert.SerializeObject(deviceDefault);
            HttpContent content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.PutAsync("api/devices/credentials", content);

            if (response.IsSuccessStatusCode)
            {
                //var result = await response.Content.ReadAsAsync<MQTTCredential>();
                //return result;
                var resultString = await response.Content.ReadAsStringAsync();
                mQTTCredential = JsonConvert.DeserializeObject<MQTTCredential>(resultString);

                // Create credentialJson file; replace if exists
                //Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                //var x = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
                //var t = await storageFolder.GetFileAsync("credentiallocal.json");

                var storeCredential = await storageFolder.CreateFileAsync(CREDENTIAL_FILENAME, Windows.Storage.CreationCollisionOption.ReplaceExisting);
                await Windows.Storage.FileIO.WriteTextAsync(storeCredential, JsonConvert.SerializeObject(mQTTCredential));

                return mQTTCredential;
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
        public class DeviceModel
        {
            public string macAddress { get; set; }
            public string uuid { get; set; }
        }
        
        private void btnGetCredential_Click(object sender, RoutedEventArgs e)
        {
            if (mQTTCredential != null)
                btnGetCredential.Content = JsonConvert.SerializeObject(mQTTCredential);
        }
        
    }
}
