using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
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
        public MqttFactory factory;
        public IMqttClient mqttClient;
        DispatcherTimer timerReSend;
        static HttpClient client = new HttpClient();
        //public MqttClientOptions options;
        //public IManagedMqttClient mqttClient = new MqttFactory().CreateManagedMqttClient();
        public MainPage()
        {
            this.InitializeComponent();
            factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient();
            //while (true)
            //{
            Debug.WriteLine("Start");

            //MQTTSetup();
          
            Debug.WriteLine("Stop");
            //}
            timerReSend = new DispatcherTimer();
            timerReSend.Interval = new TimeSpan(0, 0, 30);
            timerReSend.Tick += TimerResend_Tick;
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

        public void getConfigInforClick(object sender, RoutedEventArgs e)
        {

        }

        

        public void submitButtonClick(object sender, RoutedEventArgs e)
        {
            timerReSend.Start();
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
            var message = new MqttApplicationMessageBuilder()
                                     .WithTopic("testtopic/mot")
                                     .WithPayload("Hello HiveMQ !")
                                     .WithAtLeastOnceQoS()
                                     .Build();

            var options = new MqttClientOptionsBuilder()
                                    //.WithTcpServer("test.mosquitto.org", 1883)
                                    .WithTcpServer("broker.hivemq.com", 1883)
                                    //.WithClientId("tranlysfw")
                                    .WithCredentials("tranlysfw", "zxcvbnm1")
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
    }
}
