double val_azimut = 0;       // variable to store the value coming from the sensor
double val_alt = 0;  

void setup() {
  Serial.begin(9600);
}

void loop() {
  val_azimut = analogRead(A0);    // read the value from the sensor
  double uhel_azimut = (450*val_azimut)/1024;
  //Serial.println("Azimut:");
  //Toto funguje pro cela cisla:
  //Serial.println(analogRead(A0));//TODO - toto surove funguje, stelarium neumi desetiine carky atd.

  val_alt = analogRead(A1)-590;    // read the value from the sensor
  double uhel_alt = (450*val_alt)/1024;
  uhel_alt = 42.52;
//  Serial.println("Alt:");
//  Serial.println(analogRead(A1));
//  Serial.println(uhel_alt);

  Serial.println("az:"+String(uhel_azimut)+"|al:"+String(uhel_alt));
  
  //Serial.println(uhel_azimut);
//

//  
  delay(100);
}

void ExtractDecimalPart(float Value) {
  int IntegerPart = (int)(Value);
  int DecimalPart = 10000 * (Value - IntegerPart); //10000 b/c my float values always have exactly 4 decimal places
  Serial.println (DecimalPart);
}
