using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Security.Policy;

namespace ProjectCool_BETA_v1._0
{

    //Serial driver, ver 5.1
    public class SerialCommunicator
    { 

        SerialPort MainPort = new SerialPort();
        private string[] ports;
        Queue<string> outbound = new Queue<string>();
        Queue<string> incoming = new Queue<string>();
        private byte resend_delay;
        private bool wait_for_reply = false;


        public void SerialWatchdog()
        {
            if (this.PortOpen())
            {
                if (!this.IsBusyTX())
                {
                    if (MainPort.BytesToRead > 0)
                    {
                        incoming.Enqueue(this.ReceiveData());//RECEIVE DATA
                        wait_for_reply = false;
                    }
                }
                    if (!this.IsBusyRX() && !wait_for_reply)
                {
                    if (outbound.Count > 0)
                    {
                        this.SendData(outbound.Dequeue());//SEND DATA TO DEVICE
                        wait_for_reply = true;
                    }
                }
                    if(resend_delay++ > 20)
                {
                    wait_for_reply=false;
                }
            }
        }

        public void GetVersion()
        {
            this.SubmitPacket("V");
        }

        public void GetStats()
        {
            this.SubmitPacket("S");
        }
        public void GetLEDpacket()
        {
            this.SubmitPacket("l");
        }
        public void GetFANpacket()
        {
            this.SubmitPacket("f");
        }

        public string GetPacket()
        {
            if (incoming.Count > 0)
            {
                return incoming.Dequeue();
            }
            return "";
        }

        public void SubmitPacket(string packet)
        {  
                if (!outbound.Contains(packet))
                {
                    outbound.Enqueue(packet);
                }
        }

        public void CreateSerial() {
            
        }

        public string[] AvailablePorts
        {
            get {
                ports = SerialPort.GetPortNames(); 
                return ports; }
        }

        public string CreateSerial(int BaudRates, string port_name)
        {
            try
            {
                MainPort.PortName = port_name;
                MainPort.BaudRate = BaudRates;
                MainPort.Open();
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.ToString();   
            }
        }

        public int StopSerial() {
            if (MainPort.IsOpen)
            {
                this.ResetBuffer();
                MainPort.Close();
                return 0;          
            }
            else
            {
                return 1;   
            }
        }

        public bool PortOpen()
        {
            return MainPort.IsOpen;
        }

       private  void SendData(string data)
        {
            if (MainPort.IsOpen)
            {
                MainPort.Write(data);
            }
        }

       private  bool IsBusyRX()
        {
            if (MainPort.IsOpen)
            {
                if (MainPort.BytesToRead > 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        private bool IsBusyTX()
        {
            if (MainPort.IsOpen)
            {
                if (MainPort.BytesToWrite > 0)
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        private void ResetBuffer()
        {

            if (MainPort.IsOpen)
            {
                MainPort.ReadExisting();
                MainPort.DiscardInBuffer();
                MainPort.DiscardOutBuffer();
            }
        }

        private string ReceiveData()
        {
            try
            {
                return MainPort.ReadLine();
            }
            catch(Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
