//test prepisu
//https://lastminuteengineers.com/a4988-stepper-motor-driver-arduino-tutorial/
// Define pin connections & motor's steps per revolution
const int dirPin = 2;
const int stepPin = 3;
 int stepsPerRevolution = 1600; //200 = pro nastaveni MS1=0, MS2=0, MS3=0
const int transmittionRate = 1; //toto je pro prevod na vystupu
const int stepDelay = 5400; //5.4s = 5400ms = 5400 000 milisec
const int delayPause = 2700000;

void setup()
{
  Serial.begin(9600);
  // Declare pins as Outputs
  pinMode(stepPin, OUTPUT);
  pinMode(dirPin, OUTPUT);
  pinMode(6, OUTPUT); //Enable
  digitalWrite(6,LOW);   

    // Set motor direction clockwise
  digitalWrite(dirPin, HIGH);

  Serial.println("Start...");

  // Spin motor slowly

  //potrebuju natočit motor o několik stupnu:
  //vstupy: aktualni uhel, rychlost, smer
  
  for(int x = 0; x < stepsPerRevolution*transmittionRate; x++)
  {
    Serial.println(x);
    digitalWrite(stepPin, HIGH);
    //delayMicroseconds(delayPause);
    delay(10);
    digitalWrite(stepPin, LOW);
    //delayMicroseconds(delayPause);delay(2700);
    delay(10);
  }
  Serial.println("End...");
  delay(1000); // Wait a second
  
}

void MoveTo(int numberSteps) {

  for(int x = 0; x < stepsPerRevolution*transmittionRate; x++)
  {
    Serial.println(x);
    digitalWrite(stepPin, HIGH);
    //delayMicroseconds(delayPause);
    delay(10);
    digitalWrite(stepPin, LOW);
    //delayMicroseconds(delayPause);delay(2700);
    delay(10);
  }
  
}

void loop()
{
//  // Set motor direction clockwise
//  digitalWrite(dirPin, HIGH);
//
//  // Spin motor slowly
//  stepsPerRevolution=200;
//  for(int x = 0; x < stepsPerRevolution*transmittionRate; x++)
//  {
//    Serial.println("Krok c.:"+x);
//    digitalWrite(stepPin, HIGH);
//    //delayMicroseconds(delayPause);
//    delay(100);
//    digitalWrite(stepPin, LOW);
//    //delayMicroseconds(delayPause);delay(2700);
//    delay(100);
//  }
//  delay(1000); // Wait a second
  
  // Set motor direction counterclockwise
  //digitalWrite(dirPin, LOW);

  // Spin motor quickly
//  for(int x = 0; x < stepsPerRevolution; x++)
//  {
//    digitalWrite(stepPin, HIGH);
//    delayMicroseconds(500);
//    digitalWrite(stepPin, LOW);
//    delayMicroseconds(500);
//  }
  //delay(1000); // Wait a second
}
