//test prepisu
// Define pin connections & motor's steps per revolution
const int dirPin = 2;
const int stepPin = 3;
const int stepsPerRevolution = 3200;
const int transmittionRate = 5;
const int stepDelay = 5400; //5.4s = 5400ms = 5400 000 milisec
const int delayPause = 2700000;

void setup()
{
  // Declare pins as Outputs
  pinMode(stepPin, OUTPUT);
  pinMode(dirPin, OUTPUT);
  pinMode(6, OUTPUT); //Enable
  digitalWrite(6,LOW);   
  
}
void loop()
{
  // Set motor direction clockwise
  digitalWrite(dirPin, HIGH);

  // Spin motor slowly
  for(int x = 0; x < stepsPerRevolution*transmittionRate; x++)
  {
    digitalWrite(stepPin, HIGH);
    //delayMicroseconds(delayPause);
    delay(2700);
    digitalWrite(stepPin, LOW);
    //delayMicroseconds(delayPause);delay(2700);
    delay(2700);
  }
  delay(1000); // Wait a second
  
  // Set motor direction counterclockwise
  digitalWrite(dirPin, LOW);

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
