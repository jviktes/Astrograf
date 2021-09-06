using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArduinoSerialLink
{
        

    public class ArduinoWork
    {
        public static String AzimutActualValueFromArduino;
        public static String AltActualValueFromArduino;

        static SerialPort mySerialPort;
        
        public ArduinoWork(string portName)
        {
            Console.WriteLine("Arduino serial link starting...");
            
            mySerialPort = new SerialPort("COM3");

            string[] seznamPortu = SerialPort.GetPortNames();

            mySerialPort.BaudRate = 9600;
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 8;
            mySerialPort.Handshake = Handshake.None;
            mySerialPort.Open();
        }

        public static void LoadingData()
        {
 

            while (true)
            {
                int bytes = mySerialPort.BytesToRead;
                byte[] buffer = new byte[bytes];
                mySerialPort.Read(buffer, 0, bytes);

                string res = Encoding.UTF8.GetString(buffer);
                
                //vystup z arduina:
                //az:127.44|al:42.52
                
                //parsing value:
                string[] parVal = res.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

				try
				{
					if (parVal.Length > 0)
					{
						//buffer obsahuje plno hodnot (arduino posílá po 100ms), beru tu poslední
						//Console.WriteLine($"{DateTime.Now}: {parVal.Last()}");

						string[] azAltRawData = parVal.Last().Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
						string azimutRaw = azAltRawData[0].Replace("az:", "");


						string altRaw = azAltRawData[1].Replace("al:", "");

						AzimutActualValueFromArduino = azimutRaw;
						AltActualValueFromArduino = altRaw;

					}
					else
					{

					}
				}
				catch (Exception ex)
				{
                    //když se něco pos*, tak pokračuju, nějak to dopadne
					//throw;
				}

                Thread.Sleep(200);

            }

            Console.WriteLine("Press any key to continue...");

            mySerialPort.Write("Setup OK");

            Console.WriteLine();
        }
        
    }

    /// <summary>
    /// Tato aplikace čte hodnoty z Arduno, ze seriove linky, 
    /// </summary>
    class Arduino
    {

        public static void Main()
        {
            ArduinoWork arduinoWork = new ArduinoWork("COM3");
            Thread thread1 = new Thread(ArduinoWork.LoadingData);
            thread1.Start();

            while(true)
            {
                String _value = ArduinoWork.AzimutActualValueFromArduino;
                //Console.WriteLine(_value);
                Thread.Sleep(500);
            }
            Console.ReadKey();
        }


        //static void Main(string[] args)
        //{
        //    String rr;
        //    NewMethod(out rr);

        //    string returnValue = null;
        //    new Thread(
        //       () =>
        //       {
        //           returnValue = test();
        //       }).Start();
        //    Console.WriteLine(returnValue);
        //    Console.ReadKey();

        //}

        private static void NewMethod(out String tt)
        {
            Console.WriteLine("Hello World!");
            SerialPort mySerialPort;
            mySerialPort = new SerialPort("COM3");

            string[] seznamPortu = SerialPort.GetPortNames();

            mySerialPort.BaudRate = 9600;
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 8;
            mySerialPort.Handshake = Handshake.None;
            mySerialPort.Open();

            while (true)
            {
                int bytes = mySerialPort.BytesToRead;
                byte[] buffer = new byte[bytes];
                mySerialPort.Read(buffer, 0, bytes);

                string res = Encoding.UTF8.GetString(buffer);

                //parsing value:
                string[] parVal = res.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                if (parVal.Length > 0)
                {
                    //buffer obsahuje plno hodnot (arduino posílá po 100ms), beru tu poslední
                    //Console.WriteLine($"{DateTime.Now}: {parVal.Last()}");
                    tt =parVal.Last();
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
    }
}
