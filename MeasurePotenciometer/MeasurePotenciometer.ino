double val_azimut = 0;       // variable to store the value coming from the sensor
double val_alt = 0;  
int ratioAzimut = 720; //celkový pocet stupnu velkeho kola, nutný na 10x (celek) na otáčky potencimetru. Potencioemtr se musí otočit 10x (=1024), pomer ozubených kol je 1:5 => 2 otáčky velkého kola = 10 otáček malého; 720 st = 10*360 malé kolo
int ratioAlt = 450; // 1 otáčka malého kola = 45st velkého kola, 10 otáček pro celkové potenciometr = 450st velkého kola
int altShift = 535; //pocet jednotek, ktere ukazuje 

void setup() {
  Serial.begin(9600);
}

void loop() {
  val_azimut = analogRead(A0);    // read the value from the sensor
  double uhel_azimut = (ratioAzimut*val_azimut)/1024;
  
  //Serial.println("Azimut:");
  //Toto funguje pro cela cisla:
  //Serial.println(analogRead(A0));//TODO - toto surove funguje, stelarium neumi desetiine carky atd.

  val_alt = analogRead(A1)-altShift;    // read the value from the sensor
  double uhel_alt = (ratioAlt*val_alt)/1024;
  if (uhel_alt>360) {
      uhel_alt = uhel_alt-360;
  }
  uhel_azimut = 360-uhel_azimut;
//   if (uhel_azimut>360) {
//      uhel_azimut = uhel_azimut-360;
//  } 
  
  //uhel_azimut=uhel_azimut*(-1);
  //Serial.println(analogRead(A1)); //toto pouzit pro kalibraci ALT, dat do presne vodorovne polohy a cislo pak dat do altShift
  Serial.println("az:"+String(uhel_azimut)+"|al:"+String(uhel_alt));;
   
  delay(100);
}

void ExtractDecimalPart(float Value) {
  int IntegerPart = (int)(Value);
  int DecimalPart = 10000 * (Value - IntegerPart); //10000 b/c my float values always have exactly 4 decimal places
  Serial.println (DecimalPart);
}
