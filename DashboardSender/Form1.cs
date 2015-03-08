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

        bool issetup = false;

        enum Packets
        {
            Setup,
            DriveTrainHS,
            DriveTrainLS,
            Lights,
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
            lastData = Data;


            // Build Setup Data
            if (!issetup)
            {
                var SetupCommand = new CommandMessenger.SendCommand((int)Packets.Setup);
                SetupCommand.AddArgument((short)lastData.DriveTrain.EngineRPMMax);

                cmdMessenger.SendCommand(SetupCommand);

                issetup = true;
            }

            // Build Engine Data
            var DriveTrainHSCommand = new CommandMessenger.SendCommand((int)Packets.DriveTrainHS);
            DriveTrainHSCommand.AddArgument((short)lastData.DriveTrain.EngineRPM);
            DriveTrainHSCommand.AddArgument((short)lastData.DriveTrain.SpeedMPH);
            cmdMessenger.SendCommand(DriveTrainHSCommand);

            //Build Light Data
            var LightsCommand = new CommandMessenger.SendCommand((int)Packets.Lights);
            LightsCommand.AddArgument(lastData.Lights.LeftIndicatorActive);
            LightsCommand.AddArgument(lastData.Lights.LeftIndicatorOn);
            LightsCommand.AddArgument(lastData.Lights.RightIndicatorActive);
            LightsCommand.AddArgument(lastData.Lights.RightIndicatorOn);
            LightsCommand.AddArgument(lastData.Lights.LowBeam);
            LightsCommand.AddArgument(lastData.Lights.HighBeam);
            cmdMessenger.SendCommand(LightsCommand);
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
