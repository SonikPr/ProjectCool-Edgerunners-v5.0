using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCool_BETA_v1._0
{
    class Fan
    {
        private int main_fan_speed;
        private int slave_fan_speed;
        private int main_fan_mode;
        private int slave_fan_mode;
        private int main_manual_fan_speed;
        private int slave_manual_fan_speed;
        private double hysteresis;
        private int manual_mintemp1;
        private int manual_maxtemp1;
        private int manual_mintemp2;
        private int manual_maxtemp2;
        private int manual_starttemp1;
        private int manual_starttemp2;
        public void CreateFans() { }

        public int MainfanSpeed
        {
            get { return main_fan_speed; }
            set { main_fan_speed = value; }
        }

        public int SlavefanSpeed
        {
            get { return slave_fan_speed; }
            set { slave_fan_speed = value; }
        }

        public double Hysteresis
        {
            get { return hysteresis; }
            set { hysteresis = value; }
        }

        public int MainFanMode
        {
            get { return main_fan_mode; }
            set { main_fan_mode = value; }
        }

        public int SlaveFanMode
        {
            get { return slave_fan_mode; }
            set { slave_fan_mode = value; }
        }

        public int MainManualFanSpeed
        {
            get { return main_manual_fan_speed; }
            set { main_manual_fan_speed = value; }
        }

        public int SlaveManualFanSpeed
        {
            get { return slave_manual_fan_speed; }
            set { slave_manual_fan_speed = value; }
        }

        public int MainMinTemp
        {
            get { return manual_mintemp1; }
            set { manual_mintemp1 = value; }
        }

        public int MainMaxTemp
        {
            get { return manual_maxtemp1; }
            set { manual_maxtemp1 = value; }
        }
        public int SlaveMinTemp
        {
            get { return manual_mintemp2; }
            set { manual_mintemp2 = value; }
        }
        public int SlaveMaxTemp
        {
            get { return manual_maxtemp2; }
            set { manual_maxtemp2 = value; }
        }
        public int MainStartTemp
        {
            get { return manual_starttemp1; }
            set { manual_starttemp1 = value; }
        }
        public int SlaveStartTemp
        {
            get { return manual_starttemp2; }
            set { manual_starttemp2 = value; }
        }


        private int map(int x, int in_min, int in_max, int out_min, int out_max)
        {
            return (x - in_min) * (out_max - out_min + 1) / (in_max - in_min + 1) + out_min;
        }
    }
}
