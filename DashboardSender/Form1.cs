using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace DashboardSender
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        DashboardLibrary.IDashboardDataProvider dashProvider;
        DashboardLibrary.DashboardData lastData;

        CommandMessenger.TransportLayer.SerialTransport serialTransport;
        CommandMessenger.CmdMessenger cmdMessenger;

        int step = 0;

        bool issetup = false;

        enum Packets
        {
            Setup,
            DriveTrainHS,
            Lights,
            General,
            LowPriority
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            // Populate the form with a list of the available
            // serial port names
            string[] SerialPorts = SerialPort.GetPortNames();
            foreach(string serialPort in SerialPorts)
            {
                cmbSerialPorts.Items.Add(serialPort);
            }
            if(cmbSerialPorts.Items.Count > 0)
            {
                cmbSerialPorts.SelectedIndex = 0;
            }

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            dashProvider = new ETS2DashboardProvider.ETS2DashboardProvider();
            dashProvider.OnDashUpdate += dashProvider_OnDashUpdate;
            dashProvider.Start();

            serialTransport = new CommandMessenger.TransportLayer.SerialTransport
            {
                CurrentSerialSettings = { PortName = cmbSerialPorts.SelectedItem.ToString(), BaudRate = 115200, DtrEnable = false }
            };

            cmdMessenger = new CommandMessenger.CmdMessenger(serialTransport);
            cmdMessenger.BoardType = CommandMessenger.BoardType.Bit16;

            cmdMessenger.StartListening();
        }

        void dashProvider_OnDashUpdate(DashboardLibrary.DashboardData Data)
        {
            if (lastData == null)
                lastData = Data;

            // Build Setup Data
            if (!issetup)
            {
                var SetupCommand = new CommandMessenger.SendCommand((int)Packets.Setup);
                SetupCommand.AddArgument((short)Data.DriveTrain.EngineRPMMax);

                cmdMessenger.SendCommand(SetupCommand);

                issetup = true;
            }

            // Build Engine Data
            var DriveTrainHSCommand = new CommandMessenger.SendCommand((int)Packets.DriveTrainHS);
            DriveTrainHSCommand.AddArgument(Data.DriveTrain.EngineRPM);
            DriveTrainHSCommand.AddArgument(Data.DriveTrain.SpeedMPH);
            cmdMessenger.SendCommand(DriveTrainHSCommand);

            bool sendLights = false;
            bool sendGeneral = false;
            bool sendLowPriority = false;

            if (Data.Lights.LeftIndicatorActive != lastData.Lights.LeftIndicatorActive)
                sendLights = true;

            if (Data.Lights.RightIndicatorActive != lastData.Lights.RightIndicatorActive)
                sendLights = true;

            if (Data.Lights.HighBeam != lastData.Lights.HighBeam)
                sendLights = true;

            //Build Light Data
            if (sendLights)
            {
                var LightsCommand = new CommandMessenger.SendCommand((int)Packets.Lights);
                LightsCommand.AddArgument(Data.Lights.LeftIndicatorActive);
                LightsCommand.AddArgument(Data.Lights.RightIndicatorActive);
                LightsCommand.AddArgument(Data.Lights.HighBeam);
                cmdMessenger.SendCommand(LightsCommand);
            }

            // General Data
            if (Data.General.CruiseControl != lastData.General.CruiseControl)
                sendGeneral = true;

            if (Data.DriveTrain.ParkingBrake != lastData.DriveTrain.ParkingBrake)
                sendGeneral = true;

            if (sendGeneral)
            {
                var GeneralCommand = new CommandMessenger.SendCommand((int)Packets.General);
                GeneralCommand.AddArgument(Data.General.CruiseControl);
                GeneralCommand.AddArgument(Data.DriveTrain.ParkingBrake);
                cmdMessenger.SendCommand(GeneralCommand);
            }


            //if (Data.General.GameTime.Minute != lastData.General.GameTime.Minute)
                //sendLowPriority = true;

            if (sendLowPriority)
            {
                //var GeneralCommand = new CommandMessenger.SendCommand((int)Packets.LowPriority);
                //GeneralCommand.AddArgument(Data.General.GameTime.Hour);
                //GeneralCommand.AddArgument(Data.General.GameTime.Minute);
                //cmdMessenger.SendCommand(GeneralCommand);
            }

            System.Threading.Thread.Sleep(100);
            lastData = Data;

            if(step > 100)
            {
                step = 0;
                issetup = false;
            }



            step++;
        }

        private void DebugTimer_Tick(object sender, EventArgs e)
        {
            if(lastData != null)
            {
                txtEngineRPM.Text = lastData.DriveTrain.EngineRPM.ToString();
                txtEngineRPMMax.Text = lastData.DriveTrain.EngineRPMMax.ToString();
            }
           
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            cmdMessenger.StopListening();
        }
    }
}
