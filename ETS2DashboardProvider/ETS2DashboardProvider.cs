using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETS2DashboardProvider
{
    public class ETS2DashboardProvider : DashboardLibrary.IDashboardDataProvider
    {

        public event DashboardLibrary.DashUpdateEventHandler OnDashUpdate;

        private Ets2SdkClient.Ets2SdkTelemetry Ets2Telem;

        public bool Start()
        {
            Ets2Telem = new Ets2SdkClient.Ets2SdkTelemetry();
            Ets2Telem.Data += Ets2Telem_Data;

            return true;
        }

        // Credit: FunBit Telemetry Server
        private DateTime MinutesToDate(uint minutes)
        {
            if (minutes < 0) minutes = 0;
            return new DateTime((long)minutes * 10000000 * 60, DateTimeKind.Utc);
        }

        void Ets2Telem_Data(Ets2SdkClient.Ets2Telemetry data, bool newTimestamp)
        {
            DashboardLibrary.DashboardData newData = new DashboardLibrary.DashboardData();

            newData.DriveTrain = new DashboardLibrary._DriveTrain();
            newData.DriveTrain.EngineRPM = data.Drivetrain.EngineRpm;
            newData.DriveTrain.EngineRPMMax = data.Drivetrain.EngineRpmMax;
            newData.DriveTrain.SpeedMPH = data.Drivetrain.SpeedMph;
            newData.DriveTrain.ParkingBrake = data.Drivetrain.ParkingBrake;

            newData.Lights = new DashboardLibrary._Lights();
            newData.Lights.LeftIndicatorActive = data.Lights.BlinkerLeftActive;
            newData.Lights.LeftIndicatorOn = data.Lights.BlinkerLeftOn;
            newData.Lights.RightIndicatorActive = data.Lights.BlinkerRightActive;
            newData.Lights.RightIndicatorOn = data.Lights.BlinkerRightOn;
            newData.Lights.LowBeam = data.Lights.LowBeams;
            newData.Lights.HighBeam = data.Lights.HighBeams;
            newData.Lights.SideLights = data.Lights.ParkingLights;

            newData.General = new DashboardLibrary._General();
            newData.General.CruiseControl = data.Drivetrain.CruiseControl;
            newData.General.GameTime = MinutesToDate(data.Time);
          

            if(OnDashUpdate != null)
            {
                OnDashUpdate(newData);
            }
            
        }

        public bool Stop()
        {
            return true;
        }
    }
}
