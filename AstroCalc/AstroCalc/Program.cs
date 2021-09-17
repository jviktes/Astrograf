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
    //Nastavení typu ovládání:
    public enum eTelescopeControlling
    {
        ReadingDataFromTelescope=1, //načítá natočení os z arduina a ty se pak zobrazují ve Stelariu jako telescope
        ControllingTelescope = 2, //pro dané souřadnice objektu (ra,dec) posílá azimutální souřadnice do arduinaStepMotor, které natáčí motorkama
    }

    public class Program
    {

        //GSM poloha = konstanty

        //LBC souřadnice:
        public static double USER_LATITUDE = 50.76777777777777;
        public static double USER_LONGTITUDE = 15.079166666666666;

        //Nastavení časové zony a Daylight, DST bude nutno měnit podle letního/zimního času:
        public static int ZONE = -1;
        public static int DST = -1;

        public static eTelescopeControlling eTelescopeControlling;

        static void Main(string[] args)
        {

            Console.WriteLine("AstroCalc starting...");

            SerialPort stelariumVirtualPort = new SerialPort("COM1"); //tento port je pro komunikaci mezi Stelarium a touto konzovou aplikaci

            stelariumVirtualPort.BaudRate = 9600;
            stelariumVirtualPort.Parity = Parity.None;
            stelariumVirtualPort.StopBits = StopBits.One;
            stelariumVirtualPort.DataBits = 8;
            stelariumVirtualPort.Handshake = Handshake.None;

            stelariumVirtualPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedFromStelariumHandler);
            stelariumVirtualPort.Open();

            Console.WriteLine("Stelarium port created.");

            //Nastavení typu ovládání:
            eTelescopeControlling = eTelescopeControlling.ReadingDataFromTelescope;

            // tento port je pro komunikaci mezi Arduinem (vraci hodnoty potencimetru):
            ArduinoTelescope arduinoTelescope = new ArduinoTelescope("COM3");

            switch (eTelescopeControlling)
            {
                case eTelescopeControlling.ReadingDataFromTelescope:
                    Thread thread1 = new Thread(arduinoTelescope.LoadingData);
                    thread1.Start();
                    break;
                case eTelescopeControlling.ControllingTelescope:

                    //toto jsou souřadnice objektu, na který se bude dalekohled zaměřovat:
                    Double raStar = 14.0000 + 15.0000 / 60 + 38.0000 / 36000;
                    Double decStar = 19.0000 + 10.0000 / 60 + 8.0000 / 3600;

                    Thread thread2 = new Thread(() => arduinoTelescope.SettingData(USER_LATITUDE, USER_LONGTITUDE, ZONE, DST, raStar, decStar));
                    thread2.Start();
                    break;
                default:
                    break;
            }

            Console.ReadKey();
            stelariumVirtualPort.Close();
            
        }

        /// <summary>
        /// Handler pro zpracování došlých dat ze Stelaria, formát je Meade LX200          
        ///https://www.meade.com/support/LX200CommandSet.pdf
        ///https://astro-physics.info/tech_support/mounts/command_lang.htm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void DataReceivedFromStelariumHandler(object sender,SerialDataReceivedEventArgs e){
            
            SerialPort _stelariumVirtualPort = (SerialPort)sender;
            if (!_stelariumVirtualPort.IsOpen) return;
            int bytes = _stelariumVirtualPort.BytesToRead;
            byte[] buffer = new byte[bytes];
            _stelariumVirtualPort.Read(buffer, 0, bytes);

            string requestFromStelarium = Encoding.UTF8.GetString(buffer);

            Console.WriteLine($"DataReceived:{requestFromStelarium}");

			//Fuknční načítání dat (natočení uhlů potenciometrů) z Arduina:
			String _azimutValueFromArduino = ArduinoTelescope.AzimutActualValueFromArduino;
			Double.TryParse(_azimutValueFromArduino.Replace('.', ','), out Double azimutTelescopeValue);
			String _altValueFromArduino = ArduinoTelescope.AltActualValueFromArduino;
			Double.TryParse(_altValueFromArduino.Replace('.', ','), out Double altTelescopeValue);

            //přepočet alt-azimut souřadnic na ekvitoriální pro aktuální čas:
            cAstroCalc.cBasicAstro cBasicAstroData = new cAstroCalc.cBasicAstro(USER_LATITUDE, USER_LONGTITUDE, ZONE, DST);
            Ra_Dec_Values ra_Dec_Values = cBasicAstroData.ra_dec(DateTime.Now, azimutTelescopeValue, altTelescopeValue); 

            double ha_ = ra_Dec_Values.RA;
            double _dec = ra_Dec_Values.DEC;

            bool messageProcessed = false;
            if (requestFromStelarium.Contains("#:GR#"))
            {

                string _ra_StelariumFormat_Telecope = get_ra_StelariumFormat(ha_);
                _stelariumVirtualPort.Write(_ra_StelariumFormat_Telecope);
                Console.WriteLine(_ra_StelariumFormat_Telecope);
                messageProcessed = true;
            }

            if (requestFromStelarium.Contains(":GD#"))
            {
                string _dec_StelariumFormat_Telescope = get_dec_StelariumFormat(_dec);
                Console.WriteLine(_dec_StelariumFormat_Telescope);
                _stelariumVirtualPort.Write(_dec_StelariumFormat_Telescope);
                messageProcessed = true;
            }

            if (requestFromStelarium.Contains("Sr"))
            {
                Console.WriteLine("1");
                _stelariumVirtualPort.Write("0"); //"#:Q#:Sr05:16:42#"
                messageProcessed = true;
            }

            if (requestFromStelarium.Contains("Sd"))
            {
                Console.WriteLine("1"); //":Sd+45*59:43#"
                _stelariumVirtualPort.Write("0");
                messageProcessed = true;
            }
            if (requestFromStelarium.Contains(":MS#"))
            {
                Console.WriteLine("0");
                _stelariumVirtualPort.Write("1");
                messageProcessed = true;
            }

            if (requestFromStelarium.Contains(":CM#"))
            {
                Console.WriteLine("Objects Coordinated#");
                _stelariumVirtualPort.Write("Objects Coordinated#");
                messageProcessed = true;
            }

            if (!messageProcessed) {
                //uknown message:
                Console.WriteLine("Uknown request from Stelarium:");
                Console.WriteLine(requestFromStelarium);
			}

        }

        private static string get_dec_StelariumFormat(double _dec)
        {
            //Format dat: +54"+(char)223+"55:18#"
            return $"+{CoordinatesObject.getHoures(_dec).ToString("00")}{(char)223}{CoordinatesObject.getMinutes(_dec).ToString("00")}:{CoordinatesObject.getSeconds(_dec).ToString("00")}#";
        }

        private static string get_ra_StelariumFormat(double ha_)
        {
            //format dat  "06:38:00#"; 06:38:00#";
            return $"{CoordinatesObject.getHoures(ha_).ToString("00")}:{CoordinatesObject.getMinutes(ha_).ToString("00")}:{CoordinatesObject.getSeconds(ha_).ToString("00")}#";
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
