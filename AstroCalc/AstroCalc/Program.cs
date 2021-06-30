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
            GetAstroData();
            Console.ReadKey();
            mySerialPort.Close();
            
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
            //vstupy: pozice objektu na hvezdne obloze:
            //Polárka:
            double RA_deg = 37.963976; //TODO: prevod stupen na double
            double DEC_deg = 89.264298;

            //venuse:
            RA_deg = 126.804315;
            DEC_deg = 20.814702;

            //GSM poloha = konstanty
            //LBC:
            double LAT = 50.7620031; 
            double LONG = 15.0973567;

            //tento prevod už je hotovy:
            //RA_deg = RA_deg * 15; // prevod na stupne

            DateTime localDateTime = DateTime.UtcNow;//new DateTime(1998, 8, 10, 23, 10, 0);
            DateTime J2000 = new DateTime(2000, 1, 1, 12, 0, 0);
  
            double localTime =  localDateTime.Hour + ((double)localDateTime.Minute / 60) + ((double)localDateTime.Second / 3600);
            //double localTime = 23.1666667;//TODO prevod UT na decimal a bude to vstupni parametr

            var re = localDateTime.ToOADate() - J2000.ToOADate();

            //Local siderical time:
            double LST = 100.46 + 0.985647 * (localDateTime.ToOADate() - J2000.ToOADate()) + LONG + 15 * (localTime);
            double correctedLST = LST;
            while (correctedLST>360)
            {
                correctedLST = correctedLST - 360;
            }

            LST = correctedLST; //asi OK po vydeleni 15 dostanu hodiny a ty odpovídají

            double HA = LST - RA_deg;
            if (HA < 0) { HA = HA + 360; };

            double ALT = Math.Sin(degreeToRadian(DEC_deg)) * Math.Sin(degreeToRadian(LAT)) + Math.Cos(degreeToRadian(DEC_deg)) * Math.Cos(degreeToRadian(LAT)) * Math.Cos(degreeToRadian(HA));
            double ATL_rad = Math.Asin(ALT);
            double ALT_Degree = radianToDegree(ATL_rad);

            double AZIMUT = ((Math.Sin(degreeToRadian(DEC_deg))) - Math.Sin(degreeToRadian(ALT_Degree)) * Math.Sin(degreeToRadian(LAT))) / (Math.Cos(degreeToRadian(ALT_Degree)) * Math.Cos(degreeToRadian(LAT)));

            double AZIMUT_rad = Math.Acos(AZIMUT);
            double AZIMUT_degree = radianToDegree(AZIMUT_rad)-360; // toto je nějaký nouzový výpočet...
        }

        //vraci stupně:
        public static double degreeToRadian(double radianAngle)
        {
            return radianAngle * (Math.PI / 180);
        }

        //vraci radiany:
        private static double radianToDegree (double degreeAngle)
        {
           
            return degreeAngle * 180 / (Math.PI);
        }

    }
}
