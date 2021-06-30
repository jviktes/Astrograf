using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroCalc
{
    public class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Hello World!");
            SerialPort mySerialPort = new SerialPort("COM1");

            string[] seznamPortu = SerialPort.GetPortNames();

            mySerialPort.BaudRate = 9600;
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 8;
            mySerialPort.Handshake = Handshake.None;
            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            mySerialPort.Open();

            Console.WriteLine("Press any key to continue...");

            mySerialPort.Write("Setup OK");

            Console.WriteLine();

            //smycka pro posílání dat, každou sekundu:
            //int counter = 0;
            //while (true)
            //{
            //    mySerialPort.Write($"Smycka:{counter}");
            //    counter++;
            //    Thread.Sleep(1000);
            //}

            Console.ReadKey();
            mySerialPort.Close();
            GetAstroData();
        }

        private static void DataReceivedHandler(
            object sender,
            SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;


            if (!sp.IsOpen) return;
            int bytes = sp.BytesToRead;
            byte[] buffer = new byte[bytes];
            sp.Read(buffer, 0, bytes);

            //https://www.meade.com/support/LX200CommandSet.pdf
            //https://astro-physics.info/tech_support/mounts/command_lang.htm

            string res = Encoding.UTF8.GetString(buffer);

            Console.WriteLine(res);

            //:GR# Get Telescope RA
            //Returns: HH: MM.T# or HH:MM:SS#
            //Depending which precision is set for the telescope

            //:GD# Get Telescope Declination.
            // Returns: sDD*MM# or sDD*MM’SS#
            //Depending upon the current precision setting for the telescope. 
            //AR03: 36:55#txDEC+86?08:57#

            string _ra = "02:30:00#";
            string _dec = "+86"+(char)223+"25:57#";

            if (res.Contains("#:GR#"))
            {
                sp.Write(_ra);
                Console.WriteLine(_ra);
                //sp.Write(Environment.NewLine);
            }

            if (res.Contains(":GD#"))
            {
                Console.WriteLine(_dec);
                sp.Write(_dec);
            }

            if (res.Contains("Sr"))
            {
                Console.WriteLine("1");
                sp.Write("1");
            }

            if (res.Contains("Sd"))
            {
                Console.WriteLine("1");
                sp.Write("1");
            }
            if (res.Contains(":MS#"))
            {
                Console.WriteLine("0");
                sp.Write("1");
            }

            if (res.Contains(":CM#"))
            {
                Console.WriteLine("Objects Coordinated#");
                sp.Write("Objects Coordinated#");
            }

            //sprintf(_AR_Formated_Stelarium, "%02d:%02d:%02d#", int(arHH), int(arMM), int(arSS));
            //sprintf(_DEC_Formated_Stelarium, "%c %02d %c %02d: %02d #", sDEC_tel, int(decDEG), 223, int(decMM), int(decSS));


            //: sDD*MM#
            //HandleSerialData(buffer);

            //string indata = sp.ReadExisting();
            //Console.WriteLine("Data Received:");
            //Console.Write(indata);
            //allDataReceived = allDataReceived + indata;
            //callmyfunction(indata);
        }

        private static void HandleSerialData(byte[] respBuffer)
        {
            string res = Encoding.UTF8.GetString(respBuffer);


            if (res.Contains("#:GR#")) {
                Console.Write("+86:00:00#");
            }
            if (res.Contains(":GD#"))
            {
                Console.Write("00:00:00");
            }
                //  sprintf(_AR_Formated_Stelarium, "%02d:%02d:%02d#", int(arHH), int(arMM), int(arSS));
                //sprintf(_DEC_Formated_Stelarium, "%c%02d%c%02d:%02d#", sDEC_tel, int(decDEG), 223, int(decMM), int(decSS));
                // Console.Write(res);

        }

        private static void GetAstroData()
        {
            //vstupy:
            double RA = 16.695; //TODO: prevod stupen na double
            double DEC = 36.466667;
            double localTime = 23.1666667;//TODO prevod UT na decimal a bude to vstupni parametr

            double LAT = 52.5;
            double LONG = -1.9166667;

            RA = RA * 15; // prevod na stupne

            DateTime dateTime = new DateTime(1998, 8, 10, 23, 10, 0);
            DateTime J2000 = new DateTime(2000, 1, 1, 12, 0, 0);
            TimeSpan timeSpan = dateTime - J2000;

            var re = dateTime.ToOADate() - J2000.ToOADate();
            //Local siderical time:

            double LST = 100.46 + 0.985647 * (dateTime.ToOADate() - J2000.ToOADate()) + LONG + 15 * (localTime);
            double HA = LST - RA;
            if (HA < 0) { HA = HA + 360; };

            double ALT = Math.Sin(DEC * Math.PI / 180) * Math.Sin(LAT * Math.PI / 180) + Math.Cos(DEC * Math.PI / 180) * Math.Cos(LAT * Math.PI / 180) * Math.Cos(HA * Math.PI / 180);
            double ATL_rad = Math.Asin(ALT);
            double ALT_Degree = ATL_rad * 180 / (Math.PI);


            double AZIMUT = ((Math.Sin(DEC * Math.PI / 180)) - Math.Sin(ALT_Degree * Math.PI / 180) * Math.Sin(LAT * Math.PI / 180)) / (Math.Cos(ALT_Degree * Math.PI / 180) * Math.Cos(LAT * Math.PI / 180));
            double AZIMUT_rad = Math.Acos(AZIMUT);
            double AZIMUT_degree = AZIMUT_rad * 180 / (Math.PI);
        }

        public double degreeToRadian(double radianAngle)
        {
            return radianAngle *180 / (Math.PI);
        }

        private double radianToDegree (double degreeAngle)
        {
            return degreeAngle * (Math.PI / 180);
        }

    }
}
