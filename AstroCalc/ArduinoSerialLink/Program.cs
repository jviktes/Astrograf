using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArduinoSerialLink
{

    /// <summary>
    /// Tato aplikace čte hodnoty z Arduno, ze seriove linky, 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            SerialPort mySerialPort = new SerialPort("COM10");

            string[] seznamPortu = SerialPort.GetPortNames();

            mySerialPort.BaudRate = 9600;
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 8;
            mySerialPort.Handshake = Handshake.None;
           // mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            mySerialPort.Open();


            while (true)
            {
                //string a = mySerialPort.ReadExisting();
                //Console.WriteLine(a);
                //Thread.Sleep(200);

                int bytes = mySerialPort.BytesToRead;
                byte[] buffer = new byte[bytes];
                mySerialPort.Read(buffer, 0, bytes);

                string res = Encoding.UTF8.GetString(buffer);

                //parsing value:
                
                string[] parVal = res.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (parVal.Length>0)
                {
                    //buffer obsahuje plno hodnot (arduino posílá po 100ms), beru tu poslední
                    Console.WriteLine($"{DateTime.Now}: {parVal.Last()}");
                }
                else
                {

                }

                Thread.Sleep(200);
                
            }

            Console.WriteLine("Press any key to continue...");

            mySerialPort.Write("Setup OK");

            Console.WriteLine();
        }
        private static void DataReceivedHandler(object sender,SerialDataReceivedEventArgs e)
        {

            try
            {
                SerialPort sp = (SerialPort)sender;

                if (!sp.IsOpen) return;
                int bytes = sp.BytesToRead;
                byte[] buffer = new byte[bytes];
                sp.Read(buffer, 0, bytes);

                string res = Encoding.UTF8.GetString(buffer);

                Console.WriteLine(res);
            }
            catch (Exception ex)
            {

                throw;
            }


        }
    }
}
