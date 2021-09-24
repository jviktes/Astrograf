using ArduinoSerialLink;
using cAstroCalc;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

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
        
    
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Program");

        //GSM poloha = konstanty

        //LBC souřadnice:
        public static double USER_LATITUDE = 50.76777777777777;
        public static double USER_LONGTITUDE = 15.079166666666666;

        //Souřadnice objektu, který bude ArduinoStepper sledovat:
        public static double RA_destination;
        public static double DEC_destination;    

        //Nastavení časové zony a Daylight, DST bude nutno měnit podle letního/zimního času:
        public static int ZONE = -1;
        public static int DST = -1;


        // tento port je pro komunikaci mezi Arduinem (vraci hodnoty potencimetru):
        public static ArduinoTelescope arduinoTelescope = new ArduinoTelescope("COM4");

        //Arduino pro rizeni motorku:
        public static ArduinoStepMotorController arduinoStepMottor = new ArduinoStepMotorController("COM3");

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            Console.WriteLine("AstroCalc starting...");
            log.Debug("Starting...");

            SerialPort stelariumVirtualPort = new SerialPort("COM1"); //tento port je pro komunikaci mezi Stelarium a touto konzovou aplikaci

            stelariumVirtualPort.BaudRate = 9600;
            stelariumVirtualPort.Parity = Parity.None;
            stelariumVirtualPort.StopBits = StopBits.One;
            stelariumVirtualPort.DataBits = 8;
            stelariumVirtualPort.Handshake = Handshake.None;

            stelariumVirtualPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedFromStelariumHandler);
            stelariumVirtualPort.Open();

            Console.WriteLine("Stelarium port created.");

            //TODO:
            //zde je nutné udělat logiku, např. pokud ve Stelariu zaměřím objekt, tak se přepne režim a začnu objekt sledovat...atd.
            //nebo mít 2 arduina, které by nezávisle pracovaly na sobě (ArduinoPotencimetr a ArduinoStepMotor, nebo by to šlo najednou?
            //Nastavení typu ovládání:
 
            //Toto je Destination objekt, pokud nebude roven 0, pak se vlaknem bude volat každou sekundu příkaz na pootočení motorků:
            //Double raStar = 14.0000 + 15.0000 / 60 + 38.0000 / 36000;
            //Double decStar = 19.0000 + 10.0000 / 60 + 8.0000 / 3600;

            //Thread thread2 = new Thread(() => arduinoStepMottor.SlewToObject(USER_LATITUDE, USER_LONGTITUDE, ZONE, DST, raStar, decStar));
            //thread2.Start();

            while (true) {

				try
				{

					foreach (KeyValuePair<eCalcSyncTaskTypes, eTaskStatus> task in arduinoStepMottor.TelescopeStepMotorTasks.ToList())
					{
						//toto má největší prioritu, vyruší vše ostatní a začne se vykonávat, může být jen jedna
						if (task.Value == eTaskStatus.WaitingForProceed)
						{
							if (task.Key == eCalcSyncTaskTypes.Slew)
							{
								if (RA_destination != 0 & DEC_destination != 0)
								{
									arduinoStepMottor.SlewToObject(USER_LATITUDE, USER_LONGTITUDE, ZONE, DST, RA_destination, DEC_destination);
								}
							}

							// do something with entry.Value or entry.Key
							if (task.Key == eCalcSyncTaskTypes.Slew)
							{
								if (RA_destination != 0 & DEC_destination != 0)
								{
									arduinoStepMottor.SlewToObject(USER_LATITUDE, USER_LONGTITUDE, ZONE, DST, RA_destination, DEC_destination);
									//po nasměrování na objekt by se měla spustit úloha na jeho pronásledování:
									arduinoStepMottor.FollowObject(USER_LATITUDE, USER_LONGTITUDE, ZONE, DST, RA_destination, DEC_destination);
								}
								else
								{
									continue;
								}
							}
							if (task.Key == eCalcSyncTaskTypes.Follow)
							{

							}
						}

						if (task.Value == eTaskStatus.Running)
						{
							//Follow ==> bude se pokračovat:
							if (task.Key == eCalcSyncTaskTypes.Follow)
							{
								if (RA_destination != 0 & DEC_destination != 0)
								{
									arduinoStepMottor.FollowObject(USER_LATITUDE, USER_LONGTITUDE, ZONE, DST, RA_destination, DEC_destination);
								}
							}
						}
						if (task.Value == eTaskStatus.Nothing)
						{
							continue;
						}

					}
				}
				catch (Exception ex)
				{
                    log.Error(ex);
					//throw;
				}
                Thread.Sleep(200);
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

            Console.WriteLine($"{DateTime.Now} DataFromStelarium:{requestFromStelarium}");
            log.Debug($"{DateTime.Now} DataFromStelarium:{requestFromStelarium}");

            //Fuknční načítání dat (natočení uhlů potenciometrů) z Arduina, pokud neni pripojen Telescope, pak to vraci 0.0000.
            Double azimutTelescopeValue = ArduinoTelescope.AzimutTelescopeValue;
            Double altTelescopeValue = ArduinoTelescope.AltTelescopeValue;

            //log.Debug($"Data from arduinoPotenciometres:azimutTelescopeValue({azimutTelescopeValue})|altTelescopeValue({altTelescopeValue})");

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
                //Console.WriteLine(_ra_StelariumFormat_Telecope);
                //log.Debug("_ra_StelariumFormat_Telecope:"+_ra_StelariumFormat_Telecope);
                messageProcessed = true;
            }

            if (requestFromStelarium.Contains(":GD#"))
            {
                string _dec_StelariumFormat_Telescope = get_dec_StelariumFormat(_dec);
                //Console.WriteLine(_dec_StelariumFormat_Telescope);
                _stelariumVirtualPort.Write(_dec_StelariumFormat_Telescope);
                //log.Debug("_dec_StelariumFormat_Telescope:"+_dec_StelariumFormat_Telescope);
                messageProcessed = true;
            }

            //Toto se nastaví při zmáčknutí synchronizace s objektem ve Stelariu:
            //2021-09-18 15:30:31,857 [11] DEBUG Program - 18.09.2021 15:30:31 DataFromStelarium:#:Q#:Sr18:36:57#
            //2021 - 09 - 18 15:30:31,922[9] DEBUG Program -18.09.2021 15:30:31 DataFromStelarium::Sd + 38 * 47:09#
            
            //Set ra:
            if (requestFromStelarium.Contains("Sr"))
            {
                //Console.WriteLine("0");
                RA_destination = getRaCoordinatesFromSr(requestFromStelarium);
                _stelariumVirtualPort.Write("0"); //po skončení zapíšu "0"
                messageProcessed = true;
            }

            //Set declination:
            if (requestFromStelarium.Contains("Sd"))
            {
                //Console.WriteLine("0"); //":Sd+45*59:43#"
                DEC_destination = getDECCoordinatesFromSd(requestFromStelarium);
                _stelariumVirtualPort.Write("0");
                messageProcessed = true;
            }

            if (requestFromStelarium.Contains(":MS#"))
            {
                //Console.WriteLine("0");
                _stelariumVirtualPort.Write("1");
                if (RA_destination != 0 & DEC_destination != 0)
                {
                    arduinoStepMottor.TelescopeStepMotorTasks[eCalcSyncTaskTypes.Slew] = eTaskStatus.WaitingForProceed;
                }
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

		private static double getDECCoordinatesFromSd(string requestFromStelarium)
		{
            //":Sd+45*59:43#"
            String Q_SD = ":Sd";
            if (requestFromStelarium.Contains(Q_SD))
            {
                requestFromStelarium=requestFromStelarium.Replace(Q_SD, "").Replace("#", "");
                //TODO oddelovac je * a : ...
                String[] vals = requestFromStelarium.Split('*');

                Double.TryParse(vals[0], out double _dec_H);

                String[] vals_min_sec = vals[1].Split(':');

                Double.TryParse(vals_min_sec[0], out double _dec_Min);
                Double.TryParse(vals_min_sec[1], out double _dec_S);
                Double decStar = _dec_H + _dec_Min / 60 + _dec_S / 36000;
                return decStar;
            }
            else
            {
                return 0;
            }
        }

		private static double getRaCoordinatesFromSr(string requestFromStelarium)
		{
            //"#:Q#:Sr05:16:42#"
            String Q_SR = "#:Q#:Sr";
            if (requestFromStelarium.Contains(Q_SR)) {
                requestFromStelarium = requestFromStelarium.Replace(Q_SR, "").Replace("#","");
                String[] vals = requestFromStelarium.Split(':');
                Double.TryParse(vals[0], out double _ra_H);
                Double.TryParse(vals[1], out double _ra_Min);
                Double.TryParse(vals[2], out double _ra_S);
                Double raStar = _ra_H + _ra_Min / 60 + _ra_S / 36000;
                return raStar;
            }
            else {
                return 0;
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
