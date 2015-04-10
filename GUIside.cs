/*
This WPF project was originally made by Ahmed Suhyl and the
Brobots in the 2013-2014 season.
uTan(clan) adapted it for our purposes for the 2014-2015 season.
Most of the GUI layout and communication setup remained the same, whereas
the data that is being sent and manipulated has been changed to work with our robot
*/







using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Net;

namespace Gamepadtest
{

    /// <summary>       
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GamepadState user1;
        GamepadState user2;
        Int32 port = 5005;
        Int32 arduinoport = 5005;
        string ipadd = "192.168.0.143";
        string ipaddarduino = "192.168.1.104";
        System.Windows.Threading.DispatcherTimer myDispatcherTimer;
        System.Windows.Threading.DispatcherTimer arduinoTimer;
        public bool connectionstate = false;
        Int64 counter = 0;


        public MainWindow()
        {
            InitializeComponent();
            IPAddressBox.Text = ipadd;
            PortBox.Text = port.ToString();

            try
            {
                TestConnection();

                TestController(1);
                TestController(2);
            }
            catch
            {

            }
        }

        private void StartButton_Click_1(object sender, RoutedEventArgs e)
        {
            TestConnection();
            if (myDispatcherTimer != null)
            {
                myDispatcherTimer.Stop();
            }
            myDispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            myDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 30); // 50 Milliseconds 
            myDispatcherTimer.Tick += new EventHandler(UpdateState);
            myDispatcherTimer.Start();


         
        }

        private void StopButton_Click_1(object sender, RoutedEventArgs e)
        {
            TestConnection_Arduino();
            if (arduinoTimer != null)
            {
                arduinoTimer.Stop();
            }
            arduinoTimer = new System.Windows.Threading.DispatcherTimer();
            arduinoTimer.Interval = new TimeSpan(0, 0, 0, 0, 100); // 50 Milliseconds 
            arduinoTimer.Tick += new EventHandler(UpdateState_Arduino);
            arduinoTimer.Start();
            /*
            if (myDispatcherTimer != null)
            {
                myDispatcherTimer.Stop();
            }*/

        }

        public void UpdateState(object o, EventArgs sender)
        {
            if (user1 != null)
            {
                user1.Update();
                R_XPosLabel.Content = user1.RightStick.Position.X.ToString();
                R_YPosLabel.Content = user1.RightStick.Position.Y.ToString();
                L_XPosLabel.Content = user1.LeftStick.Position.X.ToString();
                L_YPosLabel.Content = user1.LeftStick.Position.Y.ToString();

                // string tosend = R_XPosLabel.Content+"|"+R_YPosLabel.Content+"|"+L_XPosLabel.Content+"|"+L_YPosLabel.Content;

            }
            if (user2 != null)
            {
                R_XPosLabel2.Content = user2.RightStick.Position.X.ToString();
                R_YPosLabel2.Content = user2.RightStick.Position.Y.ToString();
                L_XPosLabel2.Content = user2.LeftStick.Position.X.ToString();
                L_YPosLabel2.Content = user2.LeftStick.Position.Y.ToString();
            }

            /* ANGLE CALCULATION
             * 
             * double l_t_angle = 0.0;
              double l_t_mag = user1.LeftStick.Position.Length();
              if (l_t_mag != 0.0)
              {
                  l_t_angle = Math.Acos((double)(user1.LeftStick.Position.Y / l_t_mag));
              }
              if (user1.LeftStick.Position.X < 0)
              {
                  l_t_angle = 6.28 - l_t_angle;
              }
              l_t_angle *= 57.2957795;
              L_AngleLabel.Content = l_t_angle;*/


            /********** FOR COMPETITION PURPOSES 
            counter++;
            if (counter > 1000000)
            {
                TestConnection();
                if (!connectionstate)
                {
                    myDispatcherTimer.Stop();
                }
            }
            **************/

            if (connectionstate)
            {
                string tosend = R_XPosLabel.Content + "|" + R_YPosLabel.Content + "|" + L_XPosLabel.Content + "|" + L_YPosLabel.Content;//not used...
                Connect(ipadd, tosend);
            }
            /*
            TestConnection_Arduino();
            Connect_Arduino(ipaddarduino, arduinoport, "test");*/
        }

        public void UpdateState_Arduino(object o, EventArgs sender)
        {

            user1.Update();
            user2.Update();
            Connect_Arduino(ipaddarduino, arduinoport, "test");



        }

        public void Connect(String server, string message)
        {
            Byte[] data = null;
            try //controller data stuff
            {
                ControllerData sendData = new ControllerData(user1);
                MemoryStream stream1 = new MemoryStream();
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(ControllerData));
                ser.WriteObject(stream1, sendData);
                stream1.Position = 0;
                data = stream1.ToArray();
            }
            catch
            {
            }

            try //connection stuff
            {
                // Create a TcpClient. 
                // Note, for this client to work you need to have a TcpServer  
                // connected to the same address as specified by the server, port 
                // combination.
                TcpClient client = new TcpClient(server, port);
                // Translate the passed message into ASCII and store it as a Byte array.

                //data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing. 
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                //Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response. 

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                //Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();



            }
            catch (ArgumentNullException e)   //there was an "e" here but it was causing errors so i deleted it, put it back and still errors
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)         //there was an "e" here but it was causing errors so i deleted it, put it back and still errors
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch
            {
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }

        public void Connect_Arduino(String server, Int32 port, string message)
        {
            //Byte[] data = null;
            try //controller data stuff
            {

            }
            catch
            {
            }
            ControllerData sendData = new ControllerData(user1);


            
            var client = new UdpClient();
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipaddarduino), arduinoport); // endpoint where server is listening
            client.Connect(ep);

        }

        public void SendTCP(String message)
        {
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void TestConnectionButton_Click_1(object sender, RoutedEventArgs e)
        {
            //ipadd = IPAddressBox.
            ipadd = IPAddressBox.Text;
            TestConnection();
            //TestConnection_Arduino();
        }
        private void TestArduinoConnectionButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (ArdIPAddressBox.Text != "")
                ipaddarduino = ArdIPAddressBox.Text;
            //TestConnection();
            TestConnection_Arduino();
        }


        public void TestConnection_Arduino()
        {
            try
            {

                string server = ipaddarduino;
                int port = arduinoport;
                using (TcpClient tcp = new TcpClient())
                {
                    IAsyncResult ar = tcp.BeginConnect(server, port, null, null);
                    System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
                    try
                    {
                        if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1), false))
                        {
                            tcp.Close();
                            //throw new TimeoutException();
                            ArduinoConnectionStatusLabel.Content = "Connection Failed";
                            ConBorder2.Background = Brushes.PaleVioletRed;
                            //connectionstate = false;
                        }
                        else
                        {
                            ArduinoConnectionStatusLabel.Content = "Connection Success";
                            ConBorder2.Background = Brushes.PaleGreen;
                            tcp.EndConnect(ar);
                            //connectionstate = true;
                        }
                    }
                    finally
                    {
                        wh.Close();
                    }
                }

            }
            catch
            {
                ConnectionStatusLabel.Content = "Connection Failed";
                ConBorder.Background = Brushes.PaleVioletRed;
            }
        }

        public void TestConnection()
        {
            try
            {

                string server = IPAddressBox.Text;
                int port = Convert.ToInt32(PortBox.Text);
                using (TcpClient tcp = new TcpClient())
                {
                    IAsyncResult ar = tcp.BeginConnect(server, port, null, null);
                    System.Threading.WaitHandle wh = ar.AsyncWaitHandle;
                    try
                    {
                        if (!ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1), false))
                        {
                            tcp.Close();
                            //throw new TimeoutException();
                            ConnectionStatusLabel.Content = "Connection Failed";
                            ConBorder.Background = Brushes.PaleVioletRed;
                            connectionstate = false;
                        }
                        ConnectionStatusLabel.Content = "Connection Success";
                        ConBorder.Background = Brushes.PaleGreen;
                        tcp.EndConnect(ar);
                        connectionstate = true;
                    }
                    finally
                    {
                        wh.Close();
                    }
                }

            }
            catch
            {
                ConnectionStatusLabel.Content = "Connection Failed";
                ConBorder.Background = Brushes.PaleVioletRed;
            }




        }

        public void TestController(int user)
        {
            if (user == 1)
            {
                user1 = new GamepadState(SlimDX.XInput.UserIndex.One);
                if (user1.Connected)
                {
                    Controller1StatusLabel.Content = "Connected";
                    T1Border.Background = Brushes.PaleGreen;
                }
                else
                {
                    Controller1StatusLabel.Content = "Not Connected";
                    T1Border.Background = Brushes.PaleVioletRed;
                }
            }
            else if (user == 2)
            {
                user2 = new GamepadState(SlimDX.XInput.UserIndex.Two);
                if (user2.Connected)
                {
                    Controller2StatusLabel.Content = "Connected";
                    T2Border.Background = Brushes.PaleGreen;
                }
                else
                {
                    Controller2StatusLabel.Content = "Not Connected";
                    T2Border.Background = Brushes.PaleVioletRed;
                }
            }

        }

        private void TestController1_Click_1(object sender, RoutedEventArgs e)
        {
            TestController(1);
        }

        private void TestController2_Click_1(object sender, RoutedEventArgs e)
        {
            TestController(2);
        }
    }

    [DataContract]
    public class ControllerData //actual data to be sent across to the BeagleBone
    {

        //from here on out we are adding the vaiables we want
        [DataMember]
        public int MechFR       //front right mechanum wheel
        {
            get;
            set;
        }

        [DataMember]
        public int MechFL        //fron left mechanum wheel
        {
            get;
            set;
        }

        [DataMember]
        public int MechRR       //rear right mechanum wheel
        {
            get;
            set;
        }

        [DataMember]
        public int MechRL        //rear left mechanum wheel
        {
            get;
            set;
        }

        [DataMember]
        public int FrontRight       //normal drive front right
        {
            get;
            set;
        }

        [DataMember]
        public int FrontLeft        //normal drive front left wheel
        {
            get;
            set;
        }

        [DataMember]
        public int RearRight       //normal drive rear right wheel
        {
            get;
            set;
        }

        [DataMember]
        public int RearLeft        //normal drive rear left wheel
        {
            get;
            set;
        }

        [DataMember]
        public int JumpDrive        //engage or disengage jumpdrive
        {
            get;
            set;
        }
        /*
        [DataMember]
        public int Shooter          //PWM value for shooter
        {
            get;
            set;
        }

        [DataMember]
        public int Conveyor         //PWM value for conveyor
        {
            get;
            set;
        }
        */
  
        public ControllerData()
        {

            MechFL = 90;        //these are the mechanum drive PWM signals
            MechFR = 90;        
            MechRL = 90;
            MechRR = 90;
            RearLeft = 90;      //these are the normal drive signals
            RearRight = 90;
            FrontLeft = 90;
            FrontRight = 90;
            //Shooter = 0;        //only want to go in the forward direction
            //Conveyor = 0;       //only want to go in forward direction
            JumpDrive = 1;       //I said 1 for engage/disengage and 0 for hold state



        }

        public ControllerData(GamepadState user1)
        {
            int pwmmin = 20;            
            int pwmmax = 234;
            int pwmstop = 127;
            int pwmmidrange = ((pwmmax - pwmmin) / 2);


            float R = user1.RightStick.Position.X;
            float Y = user1.LeftStick.Position.X;
            float X = user1.LeftStick.Position.Y;

            MechFL = (int) ((X + Y + R)*pwmmidrange) + pwmstop;
            MechRL = (int) ((X + R - Y)*pwmmidrange) + pwmstop;
            MechFR = (int) ((-X + Y + R)*pwmmidrange) + pwmstop;
            MechRR = (int) ((-X - Y + R)*pwmmidrange) + pwmstop;
            //note we may need to cap the values at 0/255 since there are cases where it can go negative

 
            FrontLeft = (int) ((X + R)*pwmmidrange) + pwmstop;
            FrontRight = (int) ((-X + R)*pwmmidrange) + pwmstop;
            RearLeft = (int) ((X + R)*pwmmidrange) + pwmstop;
            RearRight = (int) ((-X + R)*pwmmidrange) + pwmstop;

            //shooter
            //left trigger value from 0 to 1
            //Shooter gets a value from 60 to 90 based on left trigger
            //Shooter = pwmstop + (int)(user1.LeftTrigger * pwmmidrange);

            //conveyor
            //we may want to make this either 0 or 1 but for now
            //same as Shooter
            //Conveyor = pwmstop + (int)(user1.RightTrigger * pwmmidrange);
            //


            //jump drive
            if (user1.Back)
            {
                JumpDrive = 1;
            }
            else
                JumpDrive = 0;
        }
    }
}

