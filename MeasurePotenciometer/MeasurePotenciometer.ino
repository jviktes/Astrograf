double val = 0;       // variable to store the value coming from the sensor

void setup() {
  Serial.begin(9600);
}

void loop() {
  val = analogRead(A0);    // read the value from the sensor
  //Serial.println(val);
  double uhel = (450*val)/1024;
  Serial.println(analogRead(A0));
  delay(100);
}

void ExtractDecimalPart(float Value) {
  int IntegerPart = (int)(Value);
  int DecimalPart = 10000 * (Value - IntegerPart); //10000 b/c my float values always have exactly 4 decimal places
  Serial.println (DecimalPart);
}
