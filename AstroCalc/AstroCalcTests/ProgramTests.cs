using Microsoft.VisualStudio.TestTools.UnitTesting;
using AstroCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroCalc.Tests
{
    [TestClass()]
    public class ProgramTests
    {
        [TestMethod()]
        public void degreeToRadianTest()
        {

            double radiansTest360 = CoordinatesObject.degreeToRadian(360);
            Assert.AreEqual(Math.PI * 2, radiansTest360);

            double degreeTests360 = CoordinatesObject.radianToDegree(Math.PI * 2);
            Assert.AreEqual(360, degreeTests360);

            double radiansTest45 = CoordinatesObject.degreeToRadian(-45);
            Assert.AreEqual(-Math.PI / 4, radiansTest45);

            double degreeTestMinus45 = CoordinatesObject.radianToDegree(-Math.PI / 4);
            Assert.AreEqual(-45, degreeTestMinus45);

        }

        [TestMethod()]
        public void originalEnglishData()
        {
            double LAT_degree = 52.5;
            double LONG_degree = -1.9166667;

            //testovaci objekt Anglican:
            double OBJECT_RA_deg = 16.695 * 15;// (16 + (double)(41.000/60))*15; //16.695*15;
            double OBJECT_DEC_deg = 36.466667;//36+(double)(28.0000/60);//36.466667;

            CoordinatesObject _object = new CoordinatesObject(OBJECT_RA_deg, OBJECT_DEC_deg, LAT_degree, LONG_degree);

            DateTime localDateTime = new DateTime(1998, 8, 10, 23, 10, 0);
            _object.GetCurrentAstroData(localDateTime);

            Assert.AreEqual(304.808, Math.Round(_object.LST, 3)); //vysledek je zaokrouhleny.
            Assert.AreEqual(49.169, Math.Round(_object.ALT_Degree, 3));
            Assert.AreEqual(269.146, Math.Round(_object.AZIMUT_degree, 3));

            Console.WriteLine(DateTime.Now);
            Console.WriteLine($"Souradnice objektu jsou ALT= {_object.Alt_H}:{_object.Alt_M}:{_object.Alt_S}");
            Console.WriteLine($"Souradnice objektu jsou Azim= {_object.Azim_H}:{_object.Azim_M}:{_object.Azim_S}");

        }
        [TestMethod()]
        public void TranformToEqitorial()
        {
            double star_altitude = 18.02;
            double star_azimut = 284.55;
            double userLatitude = 52;
            double userLongtitude = 30;
            //GST is 0h 24m 05s

            //https://frostydrew.org/utilities.dc/convert/tool-he_coordinates/

            //test prevodu alt na delta(deklinace):
            double deltaTest = CoordinatesObject.Get_Delta_from(CoordinatesObject.degreeToRadian(star_altitude), CoordinatesObject.degreeToRadian(star_azimut), CoordinatesObject.degreeToRadian(userLatitude));
            var deltaTestDegree = CoordinatesObject.radianToDegree(deltaTest);
            //Assert.AreEqual(23.219444, Math.Round(deltaTestDegree, 6));

            //test prevodu z azimut na hour
            DateTime dateTime = new DateTime(2021,1,1,20,0,0);
            double HA = CoordinatesObject.Get_HA_from(CoordinatesObject.degreeToRadian(star_altitude), CoordinatesObject.degreeToRadian(userLatitude), CoordinatesObject.degreeToRadian(star_azimut), deltaTest, userLongtitude, dateTime);
            //double tt = CoordinatesObject.radianToDegree(HA); //87.933334
            double tt = HA;
            double ha_ = tt / 15;
            var hhh = CoordinatesObject.getHoures(ha_);
            var mmm = CoordinatesObject.getMinutes(ha_);
            var ss = CoordinatesObject.getSeconds(ha_);
            //Assert.AreEqual(5, Math.Round(hhh, 0));
            //Assert.AreEqual(51, Math.Round(mmm, 0));
            //Assert.AreEqual(44, Math.Round(ss, 0));

        }

        [TestMethod()]
        public void cAstroLib() {
            cAstroCalc.cBasicAstro.Calculate();
        }
    }
}