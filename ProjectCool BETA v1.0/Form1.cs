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
    //Like me and Dashka <3
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
        int[] DeviceData = new int[20];
        int[] DeviceData2 = new int[20];
        string[] DeviceDataString = new string[20];
        string[] DeviceDataString2 = new string[20];


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

        private void SerialWatchdogTimer_Tick(object sender, EventArgs e)
        {
            DeviceSerial.SerialWatchdog(); //Watchdog for RX/TX
        }

        DeviceTemp systemps = new DeviceTemp();
        private void ProjectCool_Load(object sender, EventArgs e)
        {
            PortSelect.Items.AddRange(DeviceSerial.AvailablePorts);
            sysleds.CreateLed();
            sysfans.CreateFans();
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

            switch (mode)
            {
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
                        ChangeLocation(MainPanel, new Point(327, 12), new Size(870, 530));
                        ReverseColors(Monitoring, DevicePortLabel, 1);
                        break;
                    case 2:
                        RenderMenu(0);
                        ChangeLocation(FanControl, new Point(327, 12), new Size(870, 530));
                        ReverseColors(FanTweak, DevicePortLabel, 1);
                        break;
                    case 3:
                        RenderMenu(0);
                        ChangeLocation(LEDcontrol, new Point(327, 12), new Size(870, 530));
                        ReverseColors(LedTweak, DevicePortLabel, 1);
                        break;
                }
                MenuEnabled = menu;

            }
            if (MenuEnabled != 1)
            {
                if (DeviceSerial.PortOpen())
                {
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

        private void RenderChart(int value, int maximum, PictureBox chart)
        {
            Pen outline = new Pen(Color.FromArgb(255, 211, 98, 98));
            System.Drawing.SolidBrush bar = new SolidBrush(Color.FromArgb(255, 211, 132, 132));
            Bitmap Chart = new Bitmap(main_fan_speed_graph.Width, main_fan_speed_graph.Height); ;
            using (Graphics formGraphics = Graphics.FromImage(Chart))
            {
                formGraphics.DrawRectangle(outline, new Rectangle(0, 0, main_fan_speed_graph.Width - 1, main_fan_speed_graph.Height - 1));
                formGraphics.FillRectangle(bar, new Rectangle(4, 3, sysleds.map(value, 0, maximum, 0, main_fan_speed_graph.Width - 3), main_fan_speed_graph.Height - 6));
                outline.Dispose();
                bar.Dispose();

                formGraphics.Dispose();
            }
            chart.Image = Chart;
        }


        //Stupid buttons
        private void FanTweak_Click(object sender, EventArgs e)
        {
            RenderMenu(2);
            CommitChanges(1);
        }

        private void Monitoring_Click(object sender, EventArgs e)
        {
            RenderMenu(1);
        }

        private void LedTweak_Click(object sender, EventArgs e)
        {
            RenderMenu(3);
            CommitChanges(0);
        }

        private void PortSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            string result = DeviceSerial.CreateSerial(9600, PortSelect.Text);
            if (result != "OK")
            {
                MessageBox.Show(result, "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
            }
            else
            {
                DevicePooling.Start();
            }
        }

        byte errors_count = 0;
        void UpdateStats()
        {
            DeviceSerial.GetStats();
            DeviceDataString = DeviceSerial.GetPacket().Split(';');
            try
            {
                for (int i = 0; i < 4; i++) //Converting each character into 
                {
                    DeviceData[i] = Convert.ToInt32(DeviceDataString[i]);
                }

                systemps.T = DeviceData[0];
                systemps.H = DeviceData[1];
                sysfans.MainfanSpeed = DeviceData[2];
                sysfans.SlavefanSpeed = DeviceData[3];
                RenderChart(sysfans.MainfanSpeed, 2000, main_fan_speed_graph);
                main_fan_speed.Text = sysfans.MainfanSpeed.ToString() + "RPM";
                RenderChart(sysfans.SlavefanSpeed, 2000, sec_fan_speed_graph);
                sec_fan_speed.Text = sysfans.SlavefanSpeed.ToString() + "RPM";
                double systemps_t = systemps.T;
                double systemps_h = systemps.H;
                systemps_t = systemps_t / 10;
                systemps_h = systemps_h / 10;
                CaseTemp.Text = systemps_t.ToString();
                CaseHumidity.Text = systemps_h.ToString();
                Connection.Text = "Online";
            }
            catch (Exception EX)
            {

            }
        }
        void UpdateLEDSettings()
        {
            DeviceSerial.GetLEDpacket();
            string data;
                data = DeviceSerial.GetPacket();
                if (data.Contains("L"))
                {
                    DeviceDataString2 = data.Split(';');
                    try
                    {
                        for (int i = 0; i < 9; i++) //Converting each character into 
                        {
                            DeviceData2[i] = Convert.ToInt32(DeviceDataString2[i]);
                        }
                        sysleds.Mode = (byte)DeviceData2[0];
                        sysleds.setBrightnessFromDevice(DeviceData2[1]);
                        sysleds.Hue = DeviceData2[2];
                        sysleds.Sat = DeviceData2[3];
                        sysleds.ColorChangeSpeed = DeviceData2[4];
                        sysleds.BreatheSpeed = DeviceData2[5];
                        sysleds.VarBrMode = (byte)DeviceData2[6];
                        sysleds.VarBrParam = (byte)DeviceData2[7];
                        sysleds.FanLedMode = (byte)DeviceData2[8];
                        Connection.Text = "Online";
                        led_updated = true;
                    }
                    catch (Exception EX)
                    {
                        //Thread.Sleep(100);
                        //UpdateLEDSettings();
                    }
            }
            
    }
        void UpdateFANSettings()
        {
            DeviceSerial.GetFANpacket();
            string data;
                data = DeviceSerial.GetPacket();
                if (data.Contains("F"))
                {
                    DeviceDataString2 = data.Split(';');
                    try
                    {
                        for (int i = 0; i < 10; i++) //Converting each character into 
                        {
                            DeviceData2[i] = Convert.ToInt32(DeviceDataString2[i]);
                        }

                        sysfans.MainFanMode = DeviceData2[0];
                        sysfans.Hysteresis = DeviceData2[1];
                        sysfans.MainManualFanSpeed = DeviceData2[2];
                        sysfans.SlaveFanMode = (byte)DeviceData2[3];
                        sysfans.MainMinTemp = DeviceData2[4];
                        sysfans.MainMaxTemp = DeviceData2[5];
                        sysfans.SlaveMinTemp = DeviceData2[6];
                        sysfans.SlaveMaxTemp = DeviceData2[7];
                        sysfans.MainStartTemp = DeviceData2[8];
                        sysfans.SlaveStartTemp = DeviceData2[9];
                        Connection.Text = "Online";
                        fan_updated = true;
                    }
                    catch (Exception EX)
                    {

                    }
                }    
        }

        private void UpdateControls()
        {
            main_fan_override_manual_track.Value = sysfans.MainManualFanSpeed;
            slave_fan_override_manual_track.Value = sysfans.SlaveManualFanSpeed;
            main_fan_mode_list.SelectedIndex = sysfans.MainFanMode;
            slave_fan_mode_list.SelectedIndex = sysfans.SlaveFanMode;
            shared_hysteresis.Value = Convert.ToDecimal(sysfans.Hysteresis / 10);
            main_fan_maxtemp.Value = Convert.ToDecimal(sysfans.MainMaxTemp / 10);
            main_fan_mintemp.Value = Convert.ToDecimal(sysfans.MainMinTemp / 10);
            main_fan_starttemp.Value = Convert.ToDecimal(sysfans.MainStartTemp / 10);
            slave_fan_maxtemp.Value = Convert.ToDecimal(sysfans.SlaveMaxTemp / 10);
            slave_fan_mintemp.Value = Convert.ToDecimal(sysfans.SlaveMinTemp / 10);
            slave_fan_starttemp.Value = Convert.ToDecimal(sysfans.SlaveStartTemp / 10);

            main_led_mode.SelectedIndex = sysleds.Mode;
            brightness_manual_track.Value = sysleds.Brightness;
            color_change_track.Value = sysleds.ColorChangeSpeed;
            Breathe_speed_track.Value = sysleds.BreatheSpeed;
            Light_color_track.Value = sysleds.Hue;
            Saturation_track.Value = sysleds.Sat;
            fan_led_mode.SelectedIndex = sysleds.FanLedMode;

            main_fan_mode.Text = main_fan_mode_list.Text;
            sec_fan_mode.Text = main_fan_mode.Text;
            primary_led_mode.Text = main_led_mode.Text;
            sec_led_mode.Text = fan_led_mode.Text;
            LedTweak.Enabled = true;
            FanTweak.Enabled = true;

        }

        bool preferences_updated = false;
        bool fan_updated = false;
        bool led_updated = false;
        private void DevicePooling_Tick(object sender, EventArgs e)
        {
                DevicePooling.Interval = (int)poolingRate.Value;
             if(preferences_updated)
            {
                UpdateStats();
            }
             else
                if (DeviceSerial.PortOpen() && !preferences_updated)
            {
                OpenAndConfigurePort();
            }
        }

        void OpenAndConfigurePort()
        {
                SerialWatchdogTimer.Start();

                DeviceSerial.GetVersion();
                DeviceSerial.SerialWatchdog();// Trigger watchdog manually
                if (DeviceSerial.GetPacket().Contains("5.0"))
                {
                if (!fan_updated)
                {
                    UpdateFANSettings();
                }
                if (fan_updated)
                {
                    UpdateLEDSettings();
                }
                if (led_updated && fan_updated)
                {
                    UpdateControls();
                    preferences_updated = true;
                }
                }
                if (errors_count++ > 20)
                {
                    MessageBox.Show("This hardware is not supported.\r\n Possible reasons:\r\n 1.This is not ProjectCool 2.0 \r\n 2.Your device has firmware older than version 5.0 (Driver update) \r\n 3.This is not ProjectCool device", "Information", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DeviceSerial.StopSerial();
                }
        }

        void CommitChanges(byte packet_type)
        {
            DevicePooling.Stop();
          
                DeviceSerial.SubmitPacket(CreateQueue(packet_type));
        }
 
        string CreateQueue(byte packet_type)
        {
            string queue = "";
            switch (packet_type)
            {
                case 0:
                    sysleds.Mode = (byte)main_led_mode.SelectedIndex;
                    sysleds.Brightness = brightness_manual_track.Value;
                    sysleds.Hue = Light_color_track.Value;
                    sysleds.Sat = Saturation_track.Value;
                    sysleds.ColorChangeSpeed = color_change_track.Value;
                    sysleds.BreatheSpeed = Breathe_speed_track.Value;
                    sysleds.FanLedMode = (byte)fan_led_mode.SelectedIndex;
                    queue = sysleds.Mode + ";" + sysleds.brightness255 + ";" + sysleds.Hue + ";" + sysleds.Sat + ";" + sysleds.ColorChangeSpeed + ";" + sysleds.BreatheSpeed + ";" + "0" + ";" + "0" + ";" + sysleds.FanLedMode + ";" + "L";
                    return queue;
                case 1:
                    sysfans.MainFanMode = main_fan_mode_list.SelectedIndex;
                    //hysteresis
                    sysfans.MainManualFanSpeed = main_fan_override_manual_track.Value;
                    sysfans.SlaveFanMode = slave_fan_mode_list.SelectedIndex;
                    //manual_mintemp1 
                    //manual_maxtemp1
                    //manual_mintemp2 
                    //manual_maxtemp2 
                    //manual_starttemp1 
                    //manual_starttemp2
                    queue = sysfans.MainFanMode + ";" + shared_hysteresis.Value * 10 + ";" + sysfans.MainManualFanSpeed + ";" + ";" + sysfans.SlaveFanMode + ";" + main_fan_mintemp.Value * 10 + ";" + main_fan_maxtemp.Value * 10 + ";" + slave_fan_mintemp.Value * 10 + ";" + slave_fan_maxtemp.Value * 10 + ";" + main_fan_starttemp.Value * 10 + ";" + slave_fan_starttemp.Value * 10 + ";" + "F";
                    return queue;
                default:
                    return null;

            }     
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

        private void LEDcontrol_Paint(object sender, PaintEventArgs e)
        {

        }

        
    }
}
