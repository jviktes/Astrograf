using cAstroCalc;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArduinoSerialLink
{

    public class ArduinoTelescope
    {
        public static String AzimutActualValueFromArduino;
        public static String AltActualValueFromArduino;

        static SerialPort mySerialPort;

        public ArduinoTelescope(string portName)
        {
            Console.WriteLine($"Arduino serial link starting on port: {portName}...");

            mySerialPort = new SerialPort(portName);
            mySerialPort.BaudRate = 9600;
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 8;
            mySerialPort.Handshake = Handshake.None;
            mySerialPort.Open();

        }

        /// <summary>
        /// Smyčka neustále načítající data z Telescope Arduina:
        /// </summary>
        public void LoadingData()
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
        
        /// <summary>
        /// Smyčka neustále nastavující uhly pro MotorArdino:
        /// </summary>
        /// <param name="userLatitude"></param>
        /// <param name="userLongtitude"></param>
        /// <param name="zone"></param>
        /// <param name="dst"></param>
        /// <param name="ra"></param>
        /// <param name="dec"></param>
        public void SettingData(Double userLatitude, Double userLongtitude, int zone, int dst , Double ra , Double dec) {

            while(true) {
                cAstroCalc.cBasicAstro cBasicAstroData = new cAstroCalc.cBasicAstro(userLatitude, userLongtitude, zone, dst);
                ALT_AZIM_Values aLT_AZIM_Values = cBasicAstroData.az_al(DateTime.Now, ra, dec);
                String stepMottorCmd = $"AZ:{Math.Round((Double)aLT_AZIM_Values.Azim, 4)}|ALT:{Math.Round((Double)aLT_AZIM_Values.ALt, 4)}";
                mySerialPort.WriteLine(stepMottorCmd);

                //fakeAzimutAngle++;
                //if (fakeAzimutAngle>360) {
                //    fakeAzimutAngle = 1;
                //}
                //mySerialPort.WriteLine($"{fakeAzimutAngle}");

                Console.WriteLine(stepMottorCmd);
                
                Thread.Sleep(1000);
            }

            

        }

    }
}
