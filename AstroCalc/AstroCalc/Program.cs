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

        //GSM poloha = konstanty
        //LBC:
        public static double LAT_degree = 50.7620031;
        public static double LONG_degree = 15.0973567;

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

            //vstupy: pozice objektu na hvezdne obloze:
            //Polárka:
            double OBJECT_RA_deg = 37.963976; //TODO: prevod stupen na double
            double OBJECT_DEC_deg = 89.264298;

            //Venuse:
            OBJECT_RA_deg = 126.804315;
            OBJECT_DEC_deg = 20.814702;

            //Hvezda CAS Schedir:
            OBJECT_RA_deg = 10.127361;
            OBJECT_DEC_deg = 56.537339;

            CoordinatesObject _object =GetAstroData(OBJECT_RA_deg, OBJECT_DEC_deg);

            Console.Write($"Souradnice objektu jsou ALT= {_object.Alt_H}:{_object.Alt_M}:{_object.Alt_S}");
            Console.Write($"Souradnice objektu jsou Azim= {_object.Alt_H}:{_object.Alt_M}:{_object.Azim_S}");

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

        private static CoordinatesObject GetAstroData(double _OBJECT_RA_deg, double _OBJECT_DEC_deg)
        {


            DateTime localDateTime = DateTime.UtcNow;//new DateTime(1998, 8, 10, 23, 10, 0);
            DateTime J2000 = new DateTime(2000, 1, 1, 12, 0, 0);
  
            double localTime =  localDateTime.Hour + ((double)localDateTime.Minute / 60) + ((double)localDateTime.Second / 3600);
            //double localTime = 23.1666667;//TODO prevod UT na decimal a bude to vstupni parametr

            var re = localDateTime.ToOADate() - J2000.ToOADate();

            //Local siderical time:
            double LST = 100.46 + 0.985647 * (localDateTime.ToOADate() - J2000.ToOADate()) + LONG_degree + 15 * (localTime);
            double correctedLST = LST;
            while (correctedLST>360)
            {
                correctedLST = correctedLST - 360;
            }

            LST = correctedLST; //asi OK po vydeleni 15 dostanu hodiny a ty odpovídají
            double LST_H = LST / 15;
            double LST_M = (LST_H - Math.Truncate(LST_H))*60;
            double LST_S = (LST_M - Math.Truncate(LST_M))*60;


            double HA = LST - _OBJECT_RA_deg;
            if (HA < 0) { HA = HA + 360; };

            double ALT = Math.Sin(degreeToRadian(_OBJECT_DEC_deg)) * Math.Sin(degreeToRadian(LAT_degree)) + Math.Cos(degreeToRadian(_OBJECT_DEC_deg)) * Math.Cos(degreeToRadian(LAT_degree)) * Math.Cos(degreeToRadian(HA));
            double ATL_rad = Math.Asin(ALT);
            double ALT_Degree = radianToDegree(ATL_rad);

            double AZIMUT = ((Math.Sin(degreeToRadian(_OBJECT_DEC_deg))) - Math.Sin(degreeToRadian(ALT_Degree)) * Math.Sin(degreeToRadian(LAT_degree))) / (Math.Cos(degreeToRadian(ALT_Degree)) * Math.Cos(degreeToRadian(LAT_degree)));

            double AZIMUT_rad = Math.Acos(AZIMUT);
            double AZIMUT_degree = radianToDegree(AZIMUT_rad); // nějaká chyba: má být 307st, ale je to 53st

            if (Math.Sin(degreeToRadian(HA)) > 0)
            {
                AZIMUT_degree = 360-AZIMUT_degree;
            }
            CoordinatesObject coordinatesObject = new CoordinatesObject();
            coordinatesObject.ALT_Degree = ALT_Degree;
            coordinatesObject.AZIMUT_degree = AZIMUT_degree;
            coordinatesObject.RA_Degree = _OBJECT_RA_deg;
            coordinatesObject.DEC_degree = _OBJECT_DEC_deg;
            return coordinatesObject;
        }

        //vraci radiany:
        public static double degreeToRadian(double degreeAngle)
        {
            return degreeAngle * (Math.PI / 180);
        }

        //vraci stupně:
        public static double radianToDegree (double degreeRadians)
        {
           
            return degreeRadians * 180 / (Math.PI);
        }

    }

    public class CoordinatesObject
    {
        public double ALT_Degree;
        public double AZIMUT_degree;

        public double RA_Degree;
        public double DEC_degree;

        private double _alt_H;
        private double _alt_M;
        private double _alt_S;

        private double _azim_H;
        private double _azim_M;
        private double _azim_S;



        private double _ra_H;
        private double _ra_M;
        private double _ra_S;

        private double _dec_H;
        private double _dec_M;
        private double _dec_S;



        public double Alt_H
        {
            get { return Math.Floor(this.ALT_Degree);}
            set { _alt_H = value; }
        }

        public double Alt_M
        {
            get { return Math.Floor((Alt_H - Math.Truncate(Alt_H)) * 60); }
            set { _alt_M = value; }
        }
        
        public double Alt_S
        {
            get { return Math.Floor((Alt_M - Math.Truncate(Alt_M)) * 60); }
            set { _alt_S = value; }
        }



        public double Azim_H
        {
            get { return Math.Floor(this.AZIMUT_degree); }
            set { _azim_H = value; }
        }

        public double Azim_M
        {
            get { return Math.Floor((Azim_H - Math.Truncate(Azim_H)) * 60); }
            set { _azim_M = value; }
        }

        public double Azim_S
        {
            get { return Math.Floor((Azim_M - Math.Truncate(Azim_M)) * 60); }
            set { _azim_S = value; }
        }



        ////
        ///

        public double RA_H
        {
            get { return Math.Floor(this.RA_Degree); }
            set { _ra_H = value; }
        }

        public double RA_M
        {
            get { return Math.Floor((RA_H - Math.Truncate(RA_H)) * 60); }
            set { _ra_M = value; }
        }

        public double RA_S
        {
            get { return Math.Floor((RA_M - Math.Truncate(RA_M)) * 60); }
            set { _ra_S = value; }
        }

        public double DEC_H
        {
            get { return Math.Floor(this.DEC_degree); }
            set { _dec_H = value; }
        }

        public double DEC_M
        {
            get { return Math.Floor((DEC_H - Math.Truncate(DEC_H)) * 60); }
            set { _dec_M = value; }
        }

        public double DEC_S
        {
            get { return Math.Floor((DEC_M - Math.Truncate(DEC_M)) * 60); }
            set { _dec_S = value; }
        }


    }
}
