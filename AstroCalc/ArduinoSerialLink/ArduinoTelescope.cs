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

        public static Double AzimutTelescopeValue = 0.0000;
        public static Double AltTelescopeValue = 0.0000;

        static SerialPort mySerialPort;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Program");

        public ArduinoTelescope(string portName)
        {
            Console.WriteLine($"ArduinoTelescope serial link starting on port: {portName}...");
            //Connecting to port:
			try
			{
				mySerialPort = new SerialPort(portName);
				mySerialPort.BaudRate = 9600;
				mySerialPort.Parity = Parity.None;
				mySerialPort.StopBits = StopBits.One;
				mySerialPort.DataBits = 8;
				mySerialPort.Handshake = Handshake.None;
				mySerialPort.Open();
				mySerialPort.Write("Connection with ArduinoTelescope potenciometr OK.");
                mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedFromArduinoPotenciometer);

            }
			catch (Exception ex)
			{
                log.Error(ex);
				//throw;

			}
        }

		private void DataReceivedFromArduinoPotenciometer(object sender, SerialDataReceivedEventArgs e)
		{
            SerialPort _stelariumVirtualPort = (SerialPort)sender;
            if (!_stelariumVirtualPort.IsOpen) 
            { 
            return; 
            }
            int bytes = _stelariumVirtualPort.BytesToRead;
            byte[] buffer = new byte[bytes];
            _stelariumVirtualPort.Read(buffer, 0, bytes);
            string dataFromTelescope = Encoding.UTF8.GetString(buffer);
            Console.WriteLine($"DataReceived:{dataFromTelescope}");

            //parsing value:
            string[] parVal = dataFromTelescope.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

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
                    AzimutTelescopeValue = 0.0000;
                    AltTelescopeValue = 0.0000;
                    Double.TryParse(AzimutActualValueFromArduino.Replace('.', ','), out AzimutTelescopeValue);
                    Double.TryParse(AltActualValueFromArduino.Replace('.', ','), out AltTelescopeValue);

                }
                else
                {

                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                //throw;
            }
        }

		/// <summary>
		/// Smyčka neustále načítající data z Telescope Arduina a zapisuje je do statických proměných AzimutActualValueFromArduino a AltActualValueFromArduino
		/// </summary>
        [Obsolete]
		public void LoadingData()
        {

            while (true)
            {
                int bytes = mySerialPort.BytesToRead;
                byte[] buffer = new byte[bytes];
                mySerialPort.Read(buffer, 0, bytes);

                
                string res = Encoding.UTF8.GetString(buffer);
                
                //format vystup z arduina:
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
        //public void SettingData(Double userLatitude, Double userLongtitude, int zone, int dst , Double ra , Double dec) {

        //    while(true) {
        //        cAstroCalc.cBasicAstro cBasicAstroData = new cAstroCalc.cBasicAstro(userLatitude, userLongtitude, zone, dst);
        //        ALT_AZIM_Values aLT_AZIM_Values = cBasicAstroData.az_al(DateTime.Now, ra, dec);
        //        String stepMottorCmd = $"AZ:{Math.Round((Double)aLT_AZIM_Values.Azim, 4)}|ALT:{Math.Round((Double)aLT_AZIM_Values.ALt, 4)}";
        //        mySerialPort.WriteLine(stepMottorCmd);
             
        //        //TODO: zde by se mělo asi čekat, až se příkaz dokončí...
        //        Console.WriteLine(stepMottorCmd);
        //        log.Debug(stepMottorCmd);
        //        Thread.Sleep(1000);
        //    }

        //}

    }
}
