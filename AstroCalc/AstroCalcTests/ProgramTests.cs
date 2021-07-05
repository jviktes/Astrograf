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
        {;
            //PrivateObject privateObject = new PrivateObject(AstroCalc.Program.);

            double radiansTest360 = AstroCalc.Program.degreeToRadian(360);
            Assert.AreEqual(Math.PI*2, radiansTest360);

            double degreeTests360 = AstroCalc.Program.radianToDegree(Math.PI * 2);
            Assert.AreEqual(360, degreeTests360);

            double radiansTest45 = AstroCalc.Program.degreeToRadian(-45);
            Assert.AreEqual(-Math.PI /4, radiansTest45);

            double degreeTestMinus45 = AstroCalc.Program.radianToDegree(-Math.PI / 4);
            Assert.AreEqual(-45, degreeTestMinus45);


        }
    }
}