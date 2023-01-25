using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ProjectCool_BETA_v1._0
{
    //Cyberpunk Edgerunners
    //Like me and Dasha
    //This design and layout is an easter egg, is you know - you know
    public partial class ProjectCool : Form
    {
        public ProjectCool()
        {
            InitializeComponent();
        }
        SerialCommunicator DeviceSerial = new SerialCommunicator();
        LED sysleds = new LED();
        Fan sysfans = new Fan();
        int[] DeviceData = new int[13];
        int[] DeviceData2 = new int[13];
        string[] DeviceDataString = new string[13];
        string[] DeviceDataString2 = new string[13];


        public struct DeviceTemp
        {
            private int _t;
            private int _h;

            public DeviceTemp(int t, int h)
            {
                this._t = t;
                this._h = h;
            }

            public int T
            {
                get { return this._t; }
                set { this._t = value; }
            }

            public int H
            {
                get { return this._h; }
                set { this._h = value; }
            }

        }

        DeviceTemp systemps = new DeviceTemp();
        private void ProjectCool_Load(object sender, EventArgs e)
        {
            PortSelect.Items.AddRange(DeviceSerial.AvailablePorts);
            sysleds.CreateLed();
            sysfans.CreateFans();
            GraphicsWatchdog.Start();
        }

        int MenuEnabled;
        private void ChangeLocation(Control cntrl, Point location, Size size)
        {
            cntrl.Location = location;
            cntrl.Size = size;
        }

        private void ReverseColors(Control cntrl, Control reference, byte mode)
        {
            Color reverse = new Color();

            switch (mode) {
                case 0:
                    cntrl.BackColor = reference.BackColor;
                    cntrl.ForeColor = reference.ForeColor;
                    break;
                case 1:
                    reverse = cntrl.BackColor;
                    cntrl.BackColor = cntrl.ForeColor;
                    cntrl.ForeColor = reverse;
                    break;
            }
            
            
        }

        private void RenderMenu(byte menu)
        {
            if (menu != MenuEnabled)
            {
                switch (menu)
                {
                    case 0:
                        ChangeLocation(MainPanel, new Point(12, 453), new Size(86, 14));
                        ChangeLocation(FanControl, new Point(104, 453), new Size(86, 14));
                        ChangeLocation(LEDcontrol, new Point(203, 453), new Size(86, 14));
                        ReverseColors(Monitoring, DevicePortLabel, 0);
                        ReverseColors(FanTweak, DevicePortLabel, 0);
                        ReverseColors(LedTweak, DevicePortLabel, 0);
                        break;
                    case 1:
                        RenderMenu(0);
                        ChangeLocation(MainPanel, new Point(327, 12), new Size(873, 435));
                        ReverseColors(Monitoring, DevicePortLabel, 1);
                        break;
                    case 2:
                        RenderMenu(0);
                        ChangeLocation(FanControl, new Point(327, 12), new Size(873, 435));
                        ReverseColors(FanTweak, DevicePortLabel, 1);
                        break;
                    case 3:
                        RenderMenu(0);
                        ChangeLocation(LEDcontrol, new Point(327, 12), new Size(873, 435));
                        ReverseColors(LedTweak, DevicePortLabel, 1);
                        break;
                }
                MenuEnabled = menu;

            }
            if (MenuEnabled != 1)
            {
                if (DeviceSerial.PortOpen())
                {
                    CommitChanges();
                    DevicePooling.Stop();
                }
            }
            else
                    if (MenuEnabled == 1)
            {
                if (DeviceSerial.PortOpen())
                {
                    DevicePooling.Start();
                }
            }
        }

        private void RenderChart(int maximum)
        {
            Pen outline = new Pen(Color.FromArgb(255, 211, 98, 98));
            System.Drawing.SolidBrush bar = new SolidBrush(Color.FromArgb(255, 211, 132, 132));
            Bitmap Chart = new Bitmap(FanSpeed.Width, FanSpeed.Height); ;
            using (Graphics formGraphics = Graphics.FromImage(Chart))
            {
                formGraphics.DrawRectangle(outline, new Rectangle(0, 0, FanSpeed.Width - 1, FanSpeed.Height - 1));
                formGraphics.FillRectangle(bar, new Rectangle(4, 3, sysleds.map(maximum, 0, 100, 0, FanSpeed.Width - 3), FanSpeed.Height - 6));
                outline.Dispose();
                bar.Dispose();

                formGraphics.Dispose();
            }
            FanSpeed.Image = Chart;
        ;
        }


        //Stupid buttons
        private void FanTweak_Click(object sender, EventArgs e)
        {
            RenderMenu(2);
        }

        private void Monitoring_Click(object sender, EventArgs e)
        {
            RenderMenu(1);
        }

        private void LedTweak_Click(object sender, EventArgs e)
        {
            RenderMenu(3);
        }
        
        private void PortSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            DeviceSerial.StopSerial();
            string result = DeviceSerial.CreateSerial(9600, PortSelect.Text);
            if (result != "OK")
            {
                MessageBox.Show(result, "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                DeviceSerial.SendData("F");
                while (true)
                {
                    Thread.Sleep(500);
                    if (DeviceSerial.ReceiveData().Contains("PC1.0"))
                    {
                        UpdateSettings();
                        DevicePooling.Start();
                        break;
                    }
                    if(errors_count++ > 10)
                    {
                        MessageBox.Show("This hardware is not supported.\r\n Possible reasons:\r\n 1.This is ProjectCool, higher than ver 1.0 \r\n 2.Your device has firmware version 5.0 (Download ProjectCool NT to control it) \r\n 3.This is not ProjectCool device", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        DeviceSerial.StopSerial();
                        break;
                    }
                }
                    
            }
        }

        byte errors_count = 0;

            void UpdateStats()
            {
            DeviceSerial.SendData("S");
            string data = "0";
            if (DeviceSerial.IncomingData())
            {
                data = DeviceSerial.ReceiveData();
                DeviceDataString = data.Split(';');

                try
                {
                    for (int i = 0; i < 4; i++) //Converting each character into 
                    {
                        DeviceData[i] = Convert.ToInt32(DeviceDataString[i]);
                    }

                    systemps.T = DeviceData[0];
                    systemps.H = DeviceData[1];
                    sysfans.CurrentFanSpeed = DeviceData[3];

                    RenderChart(sysfans.CurrentFanSpeed);
                    double systemps_t = systemps.T;
                    double systemps_h = systemps.H;
                    systemps_t = systemps_t / 10;
                    systemps_h = systemps_h / 10;
                    CaseTemp.Text = systemps_t.ToString();
                    CaseHumidity.Text = systemps_h.ToString();
                }
                catch (Exception EX)
                {

                    DeviceSerial.ResetBuffer();
                    DeviceSerial.fail();
                }
            }
        }

        void UpdateSettings() {
        DeviceSerial.SendData("P");
            string data = "0";
            if (DeviceSerial.IncomingData())
            {
                data = DeviceSerial.ReceiveData();
                DeviceDataString2 = data.Split(';');
                preferences_updated = false;
                try
                {
                    for (int i = 0; i < 9; i++) //Converting each character into 
                    {
                        DeviceData2[i] = Convert.ToInt32(DeviceDataString2[i]);
                    }

                    sysfans.CurrentFanMode = DeviceData2[0];
                    sysfans.Hysteresis = DeviceData2[1];
                    sysfans.TargetFanSpeed = DeviceData2[2];
                    sysleds.Mode = (byte)DeviceData2[3];
                    sysleds.setBrightnessFromDevice(DeviceData2[4]);
                    sysleds.Hue = DeviceData2[5];
                    sysleds.Sat = DeviceData2[6];
                    sysleds.ColorChangeSpeed = DeviceData2[7];
                    sysleds.BreatheSpeed = DeviceData2[8];
                    FanSpeed_manual_track.Value = sysfans.TargetFanSpeed;
                    FansMode.SelectedIndex = sysfans.CurrentFanMode;
                    TempHysteresis.Value = Convert.ToDecimal(sysfans.Hysteresis / 10);
                    NewLedMode.SelectedIndex = sysleds.Mode;
                    brightness_manual_track.Value = sysleds.Brightness;
                    color_change_track.Value = sysleds.ColorChangeSpeed;
                    Breathe_speed_track.Value = sysleds.BreatheSpeed;
                    Light_color_track.Value = sysleds.Hue;
                    Saturation_track.Value = sysleds.Sat;
                    FanMode.Text = FansMode.Text;
                    FanMode_2.Text = FanMode.Text;
                    LedMode.Text = NewLedMode.Text;
                    preferences_updated = true;
                }
                catch (Exception EX)
                {

                    DeviceSerial.ResetBuffer();
                    DeviceSerial.fail();
                }
            }
            else
                UpdateSettings();
        }

        bool preferences_updated = false;
        private void DevicePooling_Tick(object sender, EventArgs e)
        {
                DevicePooling.Interval = (int)poolingRate.Value;
            if (preferences_updated)
            {
                UpdateStats();
            }
        }

        int tick = 0;
        private void GraphicsWatchdog_Tick(object sender, EventArgs e)
        {
            switch (DeviceSerial.GetStatus())
            {
                case "DIS":
                    Connection.Text = "NULL";
                    ReverseColors(Connection, DevicePortLabel, 0);
                    break;
                case "CON":
                    Connection.Text = "STABLE";
                    ReverseColors(Connection, DevicePortLabel, 0);
                    break;
                case "TX":
                    GraphicsWatchdog.Interval = 200;
                    Connection.Text = "TRANSMIT";
                    if(tick++%2==0)
                    ReverseColors(Connection, DevicePortLabel, 1);
                    else
                        ReverseColors(Connection, DevicePortLabel, 0);
                    break;
                case "RX":
                    GraphicsWatchdog.Interval = 500;
                    Connection.Text = "RECEIVE";
                    if (tick++ % 2 != 0)
                        ReverseColors(Connection, DevicePortLabel, 1);
                    else
                        ReverseColors(Connection, DevicePortLabel, 0);
                    break;
                case "F":
                    GraphicsWatchdog.Interval = 100;
                    Connection.Text = "ERROR";
                        ReverseColors(Connection, DevicePortLabel, 1);        
                    break;

            }
        }

        void CommitChanges()
        {
            DevicePooling.Stop();
            DeviceSerial.SendData(CreateQueue());
            DeviceSerial.ResetBuffer();            
       }

        string CreateQueue()
        {
        sysfans.CurrentFanMode = FansMode.SelectedIndex;
        sysfans.TargetFanSpeed = FanSpeed_manual_track.Value;
        sysleds.Mode = (byte)NewLedMode.SelectedIndex;
        sysleds.Brightness = brightness_manual_track.Value;
        sysleds.Hue = Light_color_track.Value;
        sysleds.Sat = Saturation_track.Value;
        sysleds.ColorChangeSpeed = color_change_track.Value;
        sysleds.BreatheSpeed = Breathe_speed_track.Value;
        string queue = "";
        queue = sysfans.CurrentFanMode + ";" + TempHysteresis.Value * 10 + ";" + sysfans.TargetFanSpeed + ";" + sysleds.Mode + ";" + sysleds.brightness255 + ";" + sysleds.Hue + ";" + sysleds.Sat + ";" + sysleds.ColorChangeSpeed + ";" + sysleds.BreatheSpeed + ";" + "0" + ";" + "0" + ";" + "E";
        return queue;
        }

        private void MainPanel_Click(object sender, EventArgs e)
        {
            if(MenuEnabled != 1)
            {
                RenderMenu(1);
            }
        }

        private void FanControl_Click(object sender, EventArgs e)
        {
            if (MenuEnabled != 2)
            {
                RenderMenu(2);
            }
        }

        private void LEDcontrol_Click(object sender, EventArgs e)
        {
            if (MenuEnabled != 3)
            {
                RenderMenu(3);
            }
        }
    }
}
