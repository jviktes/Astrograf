﻿using ArduinoSerialLink;
using cAstroCalc;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AstroCalc
{
    public class Program
    {

        //GSM poloha = konstanty
        //LBC:
        public static double LAT_degree = 50.7751814;
        public static double LONG_degree = 15.005;

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


            ArduinoWork arduinoWork = new ArduinoWork("COM3");
            Thread thread1 = new Thread(ArduinoWork.LoadingData);
            thread1.Start();


            //vstupy: pozice objektu na hvezdne obloze:
            //Polárka:
            //double OBJECT_RA_deg = 37.963976;
            //double OBJECT_DEC_deg = 89.264298;

            //Venuse:
            //double OBJECT_RA_deg = 126.804315;
            //double OBJECT_DEC_deg = 20.814702;

            //Hvezda CAS Schedir:
            double OBJECT_RA_deg = 10.127361*15;
            double OBJECT_DEC_deg = 56.537339;

            CoordinatesObject _object = new CoordinatesObject(OBJECT_RA_deg, OBJECT_DEC_deg, LAT_degree, LONG_degree);

            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)) {
                //cas je v UTC a se započtením letního času --> lokální čas je 20:35, ale nastavuju 18:35
                //ve Stellariu: mám čas 19:35, bez daylight saving a +1UTC
                DateTime localDateTime = new DateTime(2021, 7, 5, 18, 35, 0);//DateTime.UtcNow;//new DateTime(1998,8,10,23,10,0) ;//DateTime.UtcNow;
                _object.GetCurrentAstroData(localDateTime);
                Console.WriteLine(DateTime.Now);
                Console.WriteLine($"Souradnice objektu jsou ALT= {_object.Alt_H}:{_object.Alt_M}:{_object.Alt_S}");
                Console.WriteLine($"Souradnice objektu jsou Azim= {_object.Azim_H}:{_object.Azim_M}:{_object.Azim_S}");
                Thread.Sleep(1000);
                
            }

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

            //Fuknční načítání dat z Arduina:
            //String _azimutValueFromArduino = ArduinoWork.AzimutActualValueFromArduino;
            //Double.TryParse(_azimutValueFromArduino.Replace('.',','), out Double azimutVal);
            //String _altValueFromArduino = ArduinoWork.AltActualValueFromArduino;
            //Double.TryParse(_altValueFromArduino.Replace('.', ','), out Double altVal);


            //simulace nějakého natočení teleskopu v nějaký čas:
            Double azimutVal = 319;
            Double altVal = 20;

            //zde nějak získám hodnoty azimutu a alt z arduino potenciometrů:
            //=> potrebuju prevod z alt-azim na ra-dec souřadnice:

            //double azimut_arduino_degree = 283.271028;
            //double alt_arduino_degree = 19;

            double userLatitude = 50.76777777777777;
            double userLongtitude = 15.079166666666666;
            int zone = -1;
            int dst = -1;

            LAT_degree = 52;

            cAstroCalc.cBasicAstro cBasicAstroData = new cAstroCalc.cBasicAstro(userLatitude, userLongtitude, zone, dst);
            Ra_Dec_Values ra_Dec_Values = cBasicAstroData.ra_dec(DateTime.Now, azimutVal, altVal); //CoordinatesObject.Get_Delta_from(CoordinatesObject.degreeToRadian(alt_arduino_degree), CoordinatesObject.degreeToRadian(azimut_arduino_degree), CoordinatesObject.degreeToRadian(LAT_degree));
            
            //var dec_vel_to_Stelarium_degree = CoordinatesObject.radianToDegree(_deltaTest);

            //double HA = CoordinatesObject.Get_HA_from(CoordinatesObject.degreeToRadian(alt_arduino_degree), CoordinatesObject.degreeToRadian(LAT_degree), CoordinatesObject.degreeToRadian(azimut_arduino_degree), _deltaTest, userLongtitude, DateTime.Now);
            //double tt = CoordinatesObject.radianToDegree(HA);

            double ha_ = ra_Dec_Values.RA;
            double _dec = ra_Dec_Values.DEC;

            string _ra_StelariumFormat_Telecope = "";// 06:38:00#";//toto se mění --> toto bych měl načítat z arduina
            string _dec_StelariumFormat_Telescope = "";//+54"+(char)223+"55:18#";//toto by mělo být stejné 

            //arduinoValue = arduinoValue / 30; //15 = pro 1:1, ale ted mám převod že potencometr je 2x vic otáček než ozubené kolo (ozub. kolo se 2x otočí)
            //_ra_StelariumFormat_Telecope = $"{CoordinatesObject.getHoures(arduinoValue).ToString("00")}:{CoordinatesObject.getMinutes(arduinoValue).ToString("00")}:{CoordinatesObject.getSeconds(arduinoValue).ToString("00")}#";

            //string _dec_StelariumFormat_Telescope = ra_Dec_Values.DEC;//"+54"+(char)223+"55:18#";//toto by mělo být stejné 

            bool messageProcessed = false;
            if (res.Contains("#:GR#"))
            {

                var hhh = CoordinatesObject.getHoures(ha_);
                var mmm = CoordinatesObject.getMinutes(ha_);
                var ss = CoordinatesObject.getSeconds(ha_);

                //format dat  "06:38:00#";
                _ra_StelariumFormat_Telecope =$"{CoordinatesObject.getHoures(ha_).ToString("00")}:{CoordinatesObject.getMinutes(ha_).ToString("00")}:{CoordinatesObject.getSeconds(ha_).ToString("00")}#";

                sp.Write(_ra_StelariumFormat_Telecope);
                Console.WriteLine(_ra_StelariumFormat_Telecope);
                messageProcessed = true;
            }

            if (res.Contains(":GD#"))
            {
                //Format dat: +54"+(char)223+"55:18#"
                _dec_StelariumFormat_Telescope = $"+{CoordinatesObject.getHoures(_dec).ToString("00")}{(char)223}{CoordinatesObject.getMinutes(_dec).ToString("00")}:{CoordinatesObject.getSeconds(_dec).ToString("00")}#"; 
                Console.WriteLine(_dec_StelariumFormat_Telescope);
                sp.Write(_dec_StelariumFormat_Telescope);
                messageProcessed = true;
            }

            if (res.Contains("Sr"))
            {
                Console.WriteLine("1");
                sp.Write("0"); //"#:Q#:Sr05:16:42#"
                messageProcessed = true;
            }

            if (res.Contains("Sd"))
            {
                Console.WriteLine("1"); //":Sd+45*59:43#"
                sp.Write("0");
                messageProcessed = true;
            }
            if (res.Contains(":MS#"))
            {
                Console.WriteLine("0");
                sp.Write("1");
                messageProcessed = true;
            }

            if (res.Contains(":CM#"))
            {
                Console.WriteLine("Objects Coordinated#");
                sp.Write("Objects Coordinated#");
                messageProcessed = true;
            }

            if (!messageProcessed) {
                //uknown message:
                Console.WriteLine(res);
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


    }

    public class CoordinatesObject
    {

        #region Varible and properties
        public double ALT_Degree;
        public double AZIMUT_degree;

        public double RA_Degree;
        public double DEC_degree;

        public double LST;

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


        private double _lst_H;
        private double _lst_M;
        private double _lst_S;

        public double LST_H
        {
            get { return getHoures(this.LST); }
            set { _lst_H = value; }
        }

        public double LST_M
        {
            get { return getMinutes(this.LST); }
            set { _lst_M = value; }
        }


        public double LST_S
        {
            get { return getSeconds(this.LST); }
            set { _lst_S = value; }
        }

        public double Alt_H
        {
            get { return getHoures(this.ALT_Degree); }
            set { _alt_H = value; }
        }

        public double Alt_M
        {
            get { return getMinutes(this.ALT_Degree); }
            set { _alt_M = value; }
        }

        public double Alt_S
        {
            get { return getSeconds(this.ALT_Degree); }
            set { _alt_S = value; }
        }

        public double Azim_H
        {
            get { return getHoures(this.AZIMUT_degree); }
            set { _azim_H = value; }
        }

        public double Azim_M
        {
            get { return getMinutes(this.AZIMUT_degree); }
            set { _azim_M = value; }
        }

        public double Azim_S
        {
            get { return getSeconds(this.AZIMUT_degree); }
            set { _azim_S = value; }
        }

        public double RA_H
        {
            get { return getHoures(this.RA_Degree); }
            set { _ra_H = value; }
        }

        public double RA_M
        {
            get { return getMinutes(this.RA_Degree); }
            set { _ra_M = value; }
        }

        public double RA_S
        {
            get { return getSeconds(this.RA_Degree); }
            set { _ra_S = value; }
        }

        public double DEC_H
        {
            get { return getHoures(this.DEC_degree); }
            set { _dec_H = value; }
        }

        public double DEC_M
        {
            get { return getMinutes(this.DEC_degree); }
            set { _dec_M = value; }
        }

        public double DEC_S
        {
            get { return getSeconds(this.DEC_degree); }
            set { _dec_S = value; }
        }


        public double OBJECT_RA_deg { get; set; }
        public double OBJECT_DEC_deg { get; set; }


        public double LAT_degree;
        public double LONG_degree; 
        #endregion

        public CoordinatesObject(double _OBJECT_RA_deg, double _OBJECT_DEC_deg, double _LAT_degree, double _LONG_degree)
        {
            this.OBJECT_RA_deg = _OBJECT_RA_deg;
            this.OBJECT_DEC_deg = _OBJECT_DEC_deg;
            this.LAT_degree = _LAT_degree;
            this.LONG_degree = _LONG_degree;

        }

        public CoordinatesObject(double _ALT_Degree_deg, double _AZIMUT_degree, double _LAT_degree, double _LONG_degree, bool isAltAzim)
        {
            this.ALT_Degree = _ALT_Degree_deg;
            this.AZIMUT_degree = _AZIMUT_degree;
            this.LAT_degree = _LAT_degree;
            this.LONG_degree = _LONG_degree;
        }

        public static double Get_HA_from(Double starAltitude, double userLatitude, double azimut, double delta, double longtitude, DateTime DTF)
        {
            double r = 0;
            double H = 0;
            double cosHA = (Math.Sin(starAltitude) -Math.Sin(userLatitude) * Math.Sin(delta)) / (Math.Cos(userLatitude) * Math.Cos(delta));
            //var nn = Math.Sin(azimut);
            H = CoordinatesObject.radianToDegree(Math.Acos(cosHA));
            if (Math.Sin(azimut)>0)
            {
                H = 360 - H;
            }

            //r = LST(DTF, longtitude) - H;
            r = getLST(DTF, longtitude)-(H);

            if (r < 0) { r = r + 360; }

            return r;

        }
        public static double Get_Delta_from(double altitude, double azimut, double latitude)
        {
            //where φ is the observer’s geographical latitude.
            //sinδ = sina*sinφ + cosa*cosφ cosA
            double deltaDegree = 0;
            deltaDegree = Math.Asin(Math.Sin(altitude) * Math.Sin(latitude) + Math.Cos(altitude) * Math.Cos(latitude) * Math.Cos(azimut));
            return deltaDegree;
        }


        public static double getLST(DateTime localDateTime, double _LONG_degree)
        {
            DateTime J2000 = new DateTime(2000, 1, 1, 12, 0, 0);

            double localTime = localDateTime.Hour + ((double)localDateTime.Minute / 60) + ((double)localDateTime.Second / 3600);

            //Local siderical time:
            double LST = 100.46 + 0.985647 * (localDateTime.ToOADate() - J2000.ToOADate()) + _LONG_degree + 15 * (localTime);
            double correctedLST = LST;
            while (correctedLST > 360)
            {
                correctedLST = correctedLST - 360;
            }

            if (correctedLST<0)
            {
                correctedLST=correctedLST + 360;
            }

            LST = correctedLST; //asi OK po vydeleni 15 dostanu hodiny a ty odpovídají
            return LST;
        }


        //double CalcSidrealTime(ref DateTime time, double longitude)
        //{
        //    int year = time.Year;
        //    int month = time.Month;
        //    int day = time.Day;
        //    int hour = time.Hour;
        //    int min = time.Minute;
        //    int second = time.Second;

        //    if (month == 1 || month == 2)
        //    {
        //        year--;
        //        month += 12;
        //    }

        //    int a, b, c, d;
        //    a = (int)System.Math.Floor(year / 100.0);
        //    b = 2 - a + (int)System.Math.Floor(a / 4.0);
        //    c = (int)System.Math.Floor(365.25 * year);
        //    d = (int)System.Math.Floor(30.6001 * (month + 1));

        //    double jd, jt, mst = 0;

        //    // calc the number of Julian days since J2000.0
        //    jd = b + c + d - 730550.5 + day + HourMinSecToDouble(hour, min, second) / 24.0;

        //    // calc the number of Julian centuries since J2000.0
        //    jt = jd / 36525.0;

        //    return mst;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_OBJECT_RA_deg"></param>
        /// <param name="_OBJECT_DEC_deg"></param>
        /// <param name="localDateTime"></param>
        /// <returns></returns>
        public void GetCurrentAstroData(DateTime localDateTime)
        {

            double _LST = CoordinatesObject.getLST(localDateTime, LONG_degree);

            double LST_H = _LST / 15;
            double LST_M = (LST_H - Math.Truncate(LST_H)) * 60;
            double LST_S = (LST_M - Math.Truncate(LST_M)) * 60;

            double HA = _LST - this.OBJECT_RA_deg;
            if (HA < 0) { HA = HA + 360; };

            double ALT = Math.Sin(degreeToRadian(this.OBJECT_DEC_deg)) * Math.Sin(degreeToRadian(LAT_degree)) + Math.Cos(degreeToRadian(this.OBJECT_DEC_deg)) * Math.Cos(degreeToRadian(LAT_degree)) * Math.Cos(degreeToRadian(HA));
            double ATL_rad = Math.Asin(ALT);
            double ALT_Degree = radianToDegree(ATL_rad);

            double AZIMUT = ((Math.Sin(degreeToRadian(OBJECT_DEC_deg))) - Math.Sin(degreeToRadian(ALT_Degree)) * Math.Sin(degreeToRadian(LAT_degree))) / (Math.Cos(degreeToRadian(ALT_Degree)) * Math.Cos(degreeToRadian(LAT_degree)));

            double AZIMUT_rad = Math.Acos(AZIMUT);
            double AZIMUT_degree = radianToDegree(AZIMUT_rad);

            if (Math.Sin(degreeToRadian(HA)) > 0)
            {
                AZIMUT_degree = 360 - AZIMUT_degree;
            }

            //CoordinatesObject coordinatesObject = new CoordinatesObject();
            this.ALT_Degree = ALT_Degree;
            this.AZIMUT_degree = AZIMUT_degree;

            this.RA_Degree = HA/15;

            this.DEC_degree = OBJECT_DEC_deg;
            this.LST = _LST;
            this.LST_H = getHoures(_LST/15);
            this.LST_M = getMinutes(_LST/15);
            this.LST_S = getSeconds(_LST/15);

            //return coordinatesObject;
        }

        /// <summary>
        /// vstupy jsou alt a azimut, výstupy jsou 
        /// </summary>
        public void TransformHorizontalToEquitorial()
        {

        }

        public static double getHoures(double _degree)
        {
            return Math.Floor(_degree);
        }

        public static double getMinutes(double _degree)
        {
            return Math.Floor((_degree - Math.Truncate(_degree)) * 60);
        }

        public static double getSeconds(double _degree)
        {
            return Math.Floor(((_degree - Math.Truncate(_degree)) * 60 - Math.Truncate((_degree - Math.Truncate(_degree)) * 60)) * 60);
        }

        //vraci radiany:
        public static double degreeToRadian(double degreeAngle)
        {
            return degreeAngle * (Math.PI / 180);
        }

        //vraci stupně:
        public static double radianToDegree(double degreeRadians)
        {

            return degreeRadians * 180 / (Math.PI); //Math.PI /180
        }
    }
}
