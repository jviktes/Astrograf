using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cAstroCalc
{
    public class cBasicAstro
    {
        private int _zone = -1;//???
        private int _dst = -1;//???
        public const int epoch = 2000;//???

        private Double lat; //= 50.76777777777777;
        private Double lon;// = 15.079166666666666;

        //vstupy:
        public static Double ra = 19.84611111111111; //19:50
        public static Double dec = 8.870277777777778; //8:52

        public static Double az = 9.50; //9:30
        public static Double al = 87.66;//87:40
        public static Double lst = 0;
        public static int ut_flag = 0;
        public static DateTime dte;

        public cBasicAstro(Double userLatitude, Double userLongtitude, int zone = -1, int dst = -1)
        {
            lat = userLatitude;
            lon = userLongtitude;
            _zone = zone;
            _dst = dst;
        }


        //public static void Calculate()
        //{
        //    //vstupy:
        //    int day = 6;
        //    int month = 9;
        //    int year = 2021;
        //    int hr = 13;
        //    int mn = 00;
        //    int sc = 00;

        //    dte = DateTime.Now;//new DateTime(year, month, day, hr, mn, sc);

        //    //az_al(dte, zone + dst, lon, lat, ra, dec);

        //    ra_dec(dte, _zone + _dst, lon, lat,az,al);

        //    Double _az = az;
        //    Double _al = al;
        //    Double _ra = ra;
        //    Double _dec =dec;

        //}

        public ALT_AZIM_Values az_al(DateTime dte, Double ra, Double dec)
        {

            Double j, s;

            //TODO: if (epoch != 2000) nutation(dte)

            //něco tu je špatně:

            s = sideral(dte, this._zone+this._dst, lon) - ra;
            if (s < 0) { s = s + 24; }

            s = 15 * s;
            //j = dsin(dec) * dsin(lat) + dcos(dec) * dcos(lat) * dcos(s);

            j = Math.Sin(degreeToRadian(dec)) * Math.Sin(degreeToRadian(lat)) + Math.Cos(degreeToRadian(dec)) * Math.Cos(degreeToRadian(lat)) * Math.Cos(degreeToRadian(s));

            //al = dasn(j);
            al = radianToDegree(Math.Asin(j));

            //j = (dsin(dec) - dsin(lat) * dsin(al)) / (dcos(lat) * dcos(al));
            j = (Math.Sin(degreeToRadian(dec)) - Math.Sin(degreeToRadian(lat)) * Math.Sin(degreeToRadian(al))) / (Math.Cos(degreeToRadian(lat)) * Math.Cos(degreeToRadian(al)));


            //az = dacs(j);
            az = radianToDegree(Math.Acos(j));

            //j = dsin(s);
            j = Math.Sin(degreeToRadian(s));

            if (j > 0) { az = 360 - az; }

            //Výsledky:
            ALT_AZIM_Values alt_Azim_Values = new ALT_AZIM_Values();
            alt_Azim_Values.ALt = al;
            alt_Azim_Values.Azim = az;
            return alt_Azim_Values;

        }
        
        /// <summary>
        /// Metoda vrací pro daný čas (TimeNow) + AZ-ALT souřadnice --> RA DEC souřadnice
        /// </summary>
        /// <param name="dte"></param>
        /// <param name="az"></param>
        /// <param name="al"></param>
        /// <returns></returns>
        public Ra_Dec_Values ra_dec(DateTime dte, Double az, Double al)
        {
            Double j, s;
            int zone_dst = this._zone + this._dst;
            //j = dsin(al) * dsin(lat) + dcos(al) * dcos(lat) * dcos(az);
            j = Math.Sin(degreeToRadian(al)) * Math.Sin(degreeToRadian(this.lat)) + Math.Cos(degreeToRadian(al)) * Math.Cos(degreeToRadian(lat)) * Math.Cos(degreeToRadian(az));

            //dec = dasn(j);
            dec = radianToDegree(Math.Asin(j));

            //j = (dsin(al) - dsin(lat) * dsin(dec)) / (dcos(lat) * dcos(dec));
            j = (Math.Sin(degreeToRadian(al)) - (Math.Sin(degreeToRadian(lat))) * (Math.Sin(degreeToRadian(dec)))) / (Math.Cos(degreeToRadian(lat)) * Math.Cos(degreeToRadian(dec)));

            //s = dacs(j);
            s = radianToDegree(Math.Acos(j));

            //j = dsin(az);
            j = Math.Sin(degreeToRadian(az));

            if (j > 0) { s = 360 - s; }
            ra = sideral(dte, zone_dst, lon) - s / 15;
            if (ra < 0) { ra = ra + 24; }

            Ra_Dec_Values ra_Dec_Values = new Ra_Dec_Values();
            ra_Dec_Values.DEC = dec;
            ra_Dec_Values.RA = ra;
            return ra_Dec_Values;
        }

        public static Double sideral(DateTime dte, int zone, Double lon)
        {

            Double j, uct, lst, gst;
            ut_flag = 0;

            uct = ut(dte, zone);

            gst = (doy(dte) + ut_flag) * .0657098 - precess(dte) + uct * 1.002738;

            if (gst > 24) { gst = gst - 24; };
            if (gst < 0) { gst = gst + 24; };
            lst = gst + lon / 15;
            if (lst > 24) { lst = lst - 24; }
            if (lst < 0) { lst = lst + 24; }

            return lst;
        }

        public static double precess(DateTime dte)
        {

            Double p, r, s, t, y;

            y = dte.Year;
            p = y - 1;
            r = intr(p / 100);
            s = 2 - r + intr(r / 4);
            t = intr(365.25 * p);
            r = (s + t - 693597.5) / 36525;
            s = 6.646 + 2400.051 * r;

            return 24 - s + (24 * (y - 1900));
        }

        //
        //    This function returns the universal time, given the date and time zone.
        //
        public static Double ut(DateTime dte, Double zone)
        {

            Double ut = 0.000000;

            //ut = dte.getHours() + dte.getMinutes() / 100 + dte.getSeconds() / 10000;
            ut = dte.Hour + (Double)dte.Minute / 100 + (Double)dte.Second / 10000;

            ut = deg(ut) + zone;

            ut_flag = 0;
            if (ut > 24)
            {
                ut = ut - 24;
                ut_flag = 1;
            }

            return Math.Round(ut, 4);
        }

        public static Double deg(Double a) {

            Double a1, a2, a3, mm, sgn0, ss;
            sgn0 = 1;

            if (a < 0)
            {
                a = -1 * a;
                sgn0 = -1;
            }

            a1 = intr(a);
            mm = (a - a1) * 100;
            mm = Math.Round(mm, 6);// rnd(mm, 6);
            a2 = intr(mm);
            ss = (mm - a2) * 100;
            ss = Math.Round(ss, 6);
            a3 = ss;
            Double res = sgn0 * (a1 + a2 / 60 + a3 / 3600);
            return res;



        }

        //    This function returns the integer of a number.
        //
        public static int intr(Double num)
        {

            //int n = floor(abs(num)); if (num < 0) n = n * -1;

            Math.Abs(num);

            int n = (int)Math.Floor(Math.Abs(num));


            if (num < 0) n = n * -1;

            return n;
        }

        public static double degreeToRadian(double degreeAngle)
        {
            return degreeAngle * (Math.PI / 180);
        }
        public static double radianToDegree(double degreeRadians)
        {
            return degreeRadians * 180 / (Math.PI); //Math.PI /180
        }

        //This function returns the day of the year given the date.
        //
        public static int doy(DateTime dte)
        {

            return dte.DayOfYear;

        }

    }

    public class Ra_Dec_Values {
        public Double RA { get; set; }
        public Double DEC { get; set; }
    }

    public class ALT_AZIM_Values
    {
        public Double ALt { get; set; }
        public Double Azim { get; set; }
    }

}
