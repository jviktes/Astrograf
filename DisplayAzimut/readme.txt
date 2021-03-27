V některých knihovnách je problém, že neukazují přímo úhly, ale nějaké přírústky v čase nebo co.

tato knihovna (AHRS) ukazuje rovnou úhly:
AHRS is an acronym for Attitude and Heading Reference System, a system generally used for aircraft of any sort to determine heading, pitch, roll, altitude etc.

https://learn.adafruit.com/ahrs-for-adafruits-9-dof-10-dof-breakout?v

Pro LCD display toto:
https://lastminuteengineers.com/i2c-lcd-arduino-tutorial/


Pokusy s měřením pomocí gyroskoupu:

úhly mají v bytě podivné odchylky

magnetická odchylka pro dané místo
https://www.ngdc.noaa.gov/geomag/calculators/magcalc.shtml?#declination

Zde se počítá azimut - mezi 2 body
https://www.calcmaps.com/map-coordinates/ -> https://www.omnicalculator.com/other/azimuth
Naměřené úhly:
50.76197,15.09745 domov
50.73269,14.98462 Jested

LBC --> komín Rýnovice: 125.88 vzdálenost: 4.4km
LBC -->Ještěd: 247.74, vzdálenost: 8.58km

Odchylka na Ješted je asi 10st, na komín asi 20, stejně tak na Měsíc. Snaha o kalibraci aplikace Kompas v mobilu, ale také ukazuje podivnosti, na Ještě d celkem ok +-2 st, možná i přesněji, na komín ale stále +-10, nerozumím. Je nutná důsledná kalibrace sensoru např. podle https://learn.adafruit.com/adafruit-sensorlab-magnetometer-calibration?view=all
--> opouštím od magnetických sensorů, jsou velice nepřesné a pokusím se udělat to přes motorky a počítání kroků a kalibraci se zaměřením na Ještěd a odpočtu známého azimutu.


