using OxyPlot;
using OxyPlot.Series;
using MbientLab.MetaWear;
using MbientLab.MetaWear.Core;
using MbientLab.MetaWear.Data;
using MbientLab.MetaWear.Sensor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Globalization.DateTimeFormatting;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using OxyPlot.Axes;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using System.Net;
using Windows.UI.Xaml.Media;
using Windows.Storage;





// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RealTimeGraph
{
    public class MainViewModel
    {
        public const int MAX_DATA_SAMPLES = 960;
        public MainViewModel()
        {
            MyModel = new PlotModel
            {
                Title = "Acceleration",
                IsLegendVisible = true
            };
            MyModel.Series.Add(new LineSeries
            {
                BrokenLineStyle = LineStyle.Solid,
                MarkerStroke = OxyColor.FromRgb(1, 0, 0),
                LineStyle = LineStyle.Solid,
                Title = "x-axis"
            });
            MyModel.Series.Add(new LineSeries
            {
                MarkerStroke = OxyColor.FromRgb(0, 1, 0),
                LineStyle = LineStyle.Solid,
                Title = "y-axis"
            });
            MyModel.Series.Add(new LineSeries
            {
                MarkerStroke = OxyColor.FromRgb(0, 0, 1),
                LineStyle = LineStyle.Solid,
                Title = "z-axis"
            });
            MyModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                AbsoluteMinimum = -1f,
                AbsoluteMaximum = 1f,
                Minimum = -1f,
                Maximum = 1f,
                Title = "Value"
            });
            MyModel.Axes.Add(new LinearAxis
            {
                IsPanEnabled = true,
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                AbsoluteMinimum = 0,
                Minimum = 0,
                Maximum = MAX_DATA_SAMPLES
            });
        }

        public PlotModel MyModel { get; private set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LineGraph : Page
    {
        string roll_record = "";
        string pitch_record = "";
        string yaw_record = "";
        string fileAbbr = "RB1.csv";

        bool record = false;
        List<String> data;


        private IMetaWearBoard metawear;
        private IAccelerometer accelerometer;
        private IGyroBmi160 gyro;
        public LineGraph()
        {
            InitializeComponent();
            this.initWebSocketServer();
        }
        string val = "";

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {


            base.OnNavigatedTo(e);
            metawear = MbientLab.MetaWear.Win10.Application.GetMetaWearBoard(e.Parameter as BluetoothLEDevice);
            PrintAsync("HI");
            //ISensorFusionBosch sensorFusion = metawear.GetModule<ISensorFusionBosch>();
            ISensorFusionBosch sensorFusion = metawear.GetModule<ISensorFusionBosch>();

            sensorFusion.Configure();
            sensorFusion.Start();
            sensorFusion.EulerAngles.Start();
            await sensorFusion.EulerAngles.AddRouteAsync(source =>
    source.Stream(data =>
    //PrintAsync("Quaternion = " + data.Value<EulerAngles>().ToString())
    this.SetupData(data.Value<EulerAngles>().Roll.ToString(), data.Value<EulerAngles>().Pitch.ToString(), data.Value<EulerAngles>().Yaw.ToString(), data.Value<EulerAngles>().ToString())//this.extractData(data.Value<EulerAngles>().ToString())
    )
);

    //    await sensorFusion.EulerAngles.AddRouteAsync(source =>
      //      source.Stream(data =>
            //PrintAsync("Quaternion = " + data.Value<EulerAngles>().ToString())
          //  this.SetupData(data.Value<EulerAngles>().Roll.ToString(), data.Value<EulerAngles>().Pitch.ToString(), data.Value<EulerAngles>().Yaw.ToString())
        //     )
        // );


            sensorFusion.Start();
            sensorFusion.EulerAngles.Start();
            //sensorFusion.Start();

            /*  accelerometer = metawear.GetModule<IAccelerometer>();
              gyro = metawear.GetModule<IGyroBmi160>();
              gyro.Start();
                  gyro.AngularVelocity.Start();
              accelerometer.Acceleration.Start();
              accelerometer.Start();
              */

            /*await sensorFusion.Quaternion.AddRouteAsync(source =>
   source.Stream(data => val=data.Value<Quaternion>().ToString()));
   */
            PrintAsync("WHAT?");



            // this.readGyro(e);
            //  this.ReadAcc(e);

            /* problematic working code with error 250 timeout shit
             * 

            base.OnNavigatedTo(e);

            var samples = 0;
            var model = (DataContext as MainViewModel).MyModel;

            metawear = MbientLab.MetaWear.Win10.Application.GetMetaWearBoard(e.Parameter as BluetoothLEDevice);
              accelerometer = metawear.GetModule<IAccelerometer>();
            accelerometer.Configure(odr: 100f, range: 8f);
            // gyro
            gyro = metawear.GetModule<IGyroBmi160>();
            gyro.Configure(MbientLab.MetaWear.Sensor.GyroBmi160.OutputDataRate._25Hz);

            gyro.AngularVelocity.Start();
            accelerometer.Acceleration.Start();

            await gyro.AngularVelocity.AddRouteAsync(source => source.Buffer().Name("gyro"));
            await accelerometer.Acceleration.AddRouteAsync(source => source.Fuse("gyro").Stream(_ => {
                var array = _.Value<IData[]>();

                // accelerometer is the source input, index 0
                // gyro name is first input, index 1
                Console.WriteLine($"acc = {array[0].Value<Acceleration>()}, gyro = {array[1].Value<AngularVelocity>()}");
            }));

            */
            /* Gyro works
            await gyro.PackedAngularVelocity.AddRouteAsync(source => source.Stream(async data => {
                var value = data.Value<AngularVelocity>();
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                    (model.Series[0] as LineSeries).Points.Add(new DataPoint(samples, value.X));
                    (model.Series[1] as LineSeries).Points.Add(new DataPoint(samples, value.Y));
                    (model.Series[2] as LineSeries).Points.Add(new DataPoint(samples, value.Z));
                    samples++;

                    model.InvalidatePlot(true);
                    if (samples > MainViewModel.MAX_DATA_SAMPLES)
                    {
                        model.Axes[1].Reset();
                        model.Axes[1].Maximum = samples;
                        model.Axes[1].Minimum = (samples - MainViewModel.MAX_DATA_SAMPLES);
                        model.Axes[1].Zoom(model.Axes[1].Minimum, model.Axes[1].Maximum);
                    }
                });
            }));

            */
            /*
             *  metawear = MbientLab.MetaWear.Win10.Application.GetMetaWearBoard(e.Parameter as BluetoothLEDevice);

            var samples = 0;
            var model = (DataContext as MainViewModel).MyModel;

            accelerometer = metawear.GetModule<IAccelerometer>();
            accelerometer.Configure(odr: 100f, range: 8f);
            // gyro
            gyro = metawear.GetModule<IGyroBmi160>();
            gyro.Configure(MbientLab.MetaWear.Sensor.GyroBmi160.OutputDataRate._25Hz);


                         await accelerometer.PackedAcceleration.AddRouteAsync(source => source.Stream(async data => {
                        var value = data.Value<Acceleration>();
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                            (model.Series[0] as LineSeries).Points.Add(new DataPoint(samples, value.X));
                            (model.Series[1] as LineSeries).Points.Add(new DataPoint(samples, value.Y));
                            (model.Series[2] as LineSeries).Points.Add(new DataPoint(samples, value.Z));
                            samples++;

                            model.InvalidatePlot(true);
                            if (samples > MainViewModel.MAX_DATA_SAMPLES) {
                                model.Axes[1].Reset();
                                model.Axes[1].Maximum = samples;
                                model.Axes[1].Minimum = (samples - MainViewModel.MAX_DATA_SAMPLES);
                                model.Axes[1].Zoom(model.Axes[1].Minimum, model.Axes[1].Maximum);
                            }
                        });
                    }));

             gyro.Start();
            accelerometer.Start();

            await Task.Delay(15000);
            Console.WriteLine("Resetting device");
            await metawear.GetModule<IDebug>().ResetAsync();

    */





        }

        private void extractData(string data)
        {
            string[] saperated = data.Split(",");
            string printer = "";

            string Heading="";
            string Pitch="";
            string Roll="";
            string Yaw="";

            string[] IMUdata = { Heading, Pitch, Roll, Yaw };
            for (int i = 0; i < saperated.Length; i++)
            {
                printer = printer + " i=" + i + " " + saperated[i];

                IMUdata[i] = saperated[i];

            }
            PrintAsync(printer);
           // TestPrint(saperated[3].ToString());

            this.compileSendData(IMUdata);

        }

        private String serializeObject(String data)
        {

            string[] saperated = data.Split(",");
            string results = "{ " + saperated[0];
            for (int i = 1; i < saperated.Length; i++)
            {
                results = "," + saperated[i];



            }

            results = results + "}";
            return results;

        }

        private void sleep()
        { Thread.Sleep(2000); }

        public async void readGyro(NavigationEventArgs e)
        {
            metawear = MbientLab.MetaWear.Win10.Application.GetMetaWearBoard(e.Parameter as BluetoothLEDevice);

            var samples = 0;
            var model = (DataContext as MainViewModel).MyModel;



            // gyro
            gyro = metawear.GetModule<IGyroBmi160>();
            gyro.Configure(MbientLab.MetaWear.Sensor.GyroBmi160.OutputDataRate._25Hz);

            await gyro.PackedAngularVelocity.AddRouteAsync(source => source.Stream(async data =>
            {
                var value = data.Value<AngularVelocity>();
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    (model.Series[0] as LineSeries).Points.Add(new DataPoint(samples, value.X));
                    (model.Series[1] as LineSeries).Points.Add(new DataPoint(samples, value.Y));
                    (model.Series[2] as LineSeries).Points.Add(new DataPoint(samples, value.Z));
                    samples++;

                    model.InvalidatePlot(true);
                    if (samples > MainViewModel.MAX_DATA_SAMPLES)
                    {
                        model.Axes[1].Reset();
                        model.Axes[1].Maximum = samples;
                        model.Axes[1].Minimum = (samples - MainViewModel.MAX_DATA_SAMPLES);
                        model.Axes[1].Zoom(model.Axes[1].Minimum, model.Axes[1].Maximum);
                    }
                });
            }));




        }

        public async void ReadAcc(NavigationEventArgs e)
        {
            metawear = MbientLab.MetaWear.Win10.Application.GetMetaWearBoard(e.Parameter as BluetoothLEDevice);

            var samples = 0;
            var model = (DataContext as MainViewModel).MyModel;

            accelerometer = metawear.GetModule<IAccelerometer>();
            accelerometer.Configure(odr: 100f, range: 8f);
            // gyro
            gyro = metawear.GetModule<IGyroBmi160>();
            gyro.Configure(MbientLab.MetaWear.Sensor.GyroBmi160.OutputDataRate._25Hz);
            gyro.AngularVelocity.Start();
            gyro.Start();


            await accelerometer.Acceleration.AddRouteAsync(source => source.Stream(async data =>
            {
                var value = data.Value<Acceleration>();
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {

                    /*(model.Series[0] as LineSeries).Points.Add(new DataPoint(samples, value.X));
                    (model.Series[1] as LineSeries).Points.Add(new DataPoint(samples, value.Y));
                    (model.Series[2] as LineSeries).Points.Add(new DataPoint(samples, value.Z));
                    samples++;

                    model.InvalidatePlot(true);
                    if (samples > MainViewModel.MAX_DATA_SAMPLES)
                    {
                        model.Axes[1].Reset();
                        model.Axes[1].Maximum = samples;
                        model.Axes[1].Minimum = (samples - MainViewModel.MAX_DATA_SAMPLES);
                        model.Axes[1].Zoom(model.Axes[1].Minimum, model.Axes[1].Maximum);
                    }*/

                    PrintAsync("X " + value.X + "  Y " + value.Y + "  Z" + value.Z);

                });
            }));




        }

        private async Task PrintAsync(string s)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MetaInfo.Text = s + "  " + val;

                
            });



        }
        
        private async Task SetupData(string roll, string pitch, string yaw, string data)
        {
            this.extractData(data);
   
               await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //MetaInfo.Text = s + "  " + val;

                //State.Text = s;

                roll_record = roll;
                pitch_record = pitch;
                yaw_record = yaw;

                dataRollPitchYaw.Text = "roll : " + roll + " pitch : " + pitch + " yaw : " + yaw;

            });



        }
       


        private async void back_Click(object sender, RoutedEventArgs e)
        {
            if (!metawear.InMetaBootMode)
            {
                metawear.TearDown();
                await metawear.GetModule<IDebug>().DisconnectAsync();
            }
            Frame.GoBack();
        }

        private void streamSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (streamSwitch.IsOn)
            {
                gyro.AngularVelocity.Start();
                gyro.Start();
            }
            else
            {
                accelerometer.Stop();
                accelerometer.Acceleration.Stop();
            }
        }




        WebSocketSharp.WebSocket ws;
        // WebSocketSharp.Server.WebSocketServer ws;
        //  FeedbackBehaviour feedbackBehaviour;
        bool ipState = false;
        string ip = "";
        private void initWebSocketServer()
        {
            bool ipState = false;
            string ip = "";
            try
            {
                ip = this.GetLocalIPAddress();
                ipState = true;

            }
            catch (Exception e)
            {
                Log(e.Message);
            }

            if (ipState)
            {
                //ip =;
                //   Client code working
                //  ws = new WebSocket("ws://" + "10.12.3.133" + ":8100/F");

                // chat server
             ws = new WebSocket("ws://" + "192.168.0.137" + ":4649/Chat");

                //   ws = new WebSocket("ws://" + "192.168.49.87" + ":8100/F"); 

                // ws = new WebSocket("ws://" + "10.12.3.104" + ":8100/F");

                ws.Connect();
                this.send("Hello, Im Meta ");

                /*
                 * broken server
              ws = new WebSocketSharp.Server.WebSocketServer("ws://" + ip + ":8100");
              ws.AddWebSocketService<Echo>("/Echo");
              ws.Start();

          /*



              //  System.Net.IPAddress ipadd=System.Net.IPAddress.Parse("ws://" + ip + ":8100");
              //ws = new WebSocketNet.WebSocket();

              //  ws.ConnectAsync(new Uri("ws://10.12.3.133:8000/Handler"));
              // ws.Send("HI");
              //ws.ConnectAsync("ws://" + ip + ":8100");
              // ws = new WebSocketServer(ipadd);
              // this.Log(" Client is connecting too:" + "ws://" + ip + ":8000");
              // ws.Send("Hello");
              /*
              ws.Start();
              feedbackBehaviour = new FeedbackBehaviour(this);
              ws.AddWebSocketService<FeedbackBehaviour>("/FB", () => feedbackBehaviour);
  */
            }
        }

        // data counter, sends data every 20 frames or so
        int c = 0;
        void compileSendData(string [] IMUData)
        {

            
            IMUPacket packet= new IMUPacket();

            // extract numerical information only
            string h= IMUData[0];
            string[] heading=h.Split(":");
            packet.Heading = heading[1];

            string p = IMUData[1];
            string[] pitch = p.Split(":");
            
            packet.Pitch = pitch[1];


            string R = IMUData[2];
            string[] Roll = R.Split(":");

            packet.Roll = Roll[1];




            string Y = IMUData[3];
            string[] Yaw = Y.Split(":");

            packet.Yaw = Yaw[1];

            string Ipacket=Newtonsoft.Json.JsonConvert.SerializeObject(packet);
            // if(sendData)

            c++;
          
            if(c>5)
            {
                c = 0;
                //if(ws.IsAlive)
            //ws.Send(Ipacket);
            }
            
            
        }


        public partial class IMUPacket
          {



            public string Heading = "";
            public string  Pitch = "";
            public string Roll = "";
            public string Yaw ="";

            public IMUPacket()
            { }
            public IMUPacket(string heading, string pitch, string roll, string yaw)
            {
                this.Heading = heading;
                this.Pitch = pitch;
                this.Roll = roll;
                this.Yaw = yaw;

            }
}

        void send(String msg)
        {
            if (ws.IsAlive)
                ws.Send(msg);
            else
                this.Log("Error: msg cannot be sent, ws connection is not alive");

        }

        int messageIndex = 0;

        public async void Log(String s)
        {
    

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.LogBox.Text= this.LogBox.Text + (messageIndex + ":  " + s + "\r");
                messageIndex++;
            });

        }

        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {

            this.wrapup();
        }

        private void wrapup()
        {

          //  ws.Stop();

        }

        bool isServerOn=false;

        //   FeedbackBehaviour fb;

        bool sendData = false;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.sendData = !sendData;
            // ServerSwitch.Foreground = Brush.White;
            // if (!isServerOn)

            /*
            var IPAddress = IPBox.Text;
            ws = new WebSocket(IPAddress);
            ws.Connect();

            ws.Send("Hello, Im Meta");
            */
        }

        private void Reconnect_Click(object sender, RoutedEventArgs e)
        {
            initWebSocketServer();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            fileAbbr = filenamecsv.Text+".csv";

            record = !record;

            if (record)
            {

                //  MessageBox.Show("Recording is Started!"); //just debugging
                State.Text = "Recording .....";

                data = new List<string>(); // setup the list if needed
                data.Clear(); //clear the list             

                //start thread
                Thread t = new Thread(new ThreadStart(ThreadProc));
                t.Start();
            }
            else
            {
                State.Text = "Stop";
                // MessageBox.Show("Recording is Stopped"); //Debugging
            }
        }

        private void ThreadProc()
        {

            data.Add("roll,pitch,yaw");

            while (record)
            {
                System.Random random = new System.Random();
                //load your list here if it was list and doo lopping from here to tw.writeline

              
               // int roll = random.Next(1000);
             //   int pitch = random.Next(1000);
             //   int yaw = random.Next(1000);


                string IMU = roll_record + "," + pitch_record + "," + yaw_record;

                data.Add(IMU);

                Thread.Sleep(10); //sleep 10 ms
            }

            //if data roll pitch yaw is already in List, just write directly to tw with foreach

            WriteToCsv();

        }

        private async void WriteToCsv()
        {
            
            StorageFolder folder = KnownFolders.MusicLibrary;
            StorageFile filename = await folder.CreateFileAsync(fileAbbr, CreationCollisionOption.ReplaceExisting);

            //var data = new List<string>() { "4", "5", "6" };

            await FileIO.WriteLinesAsync(filename, data);
        }
    }
}
