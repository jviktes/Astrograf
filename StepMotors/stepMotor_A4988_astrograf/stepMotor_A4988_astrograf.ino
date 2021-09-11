//test prepisu
//https://lastminuteengineers.com/a4988-stepper-motor-driver-arduino-tutorial/
// Define pin connections & motor's steps per revolution
const int dirPin = 2;
const int stepPin = 3;

const int stepsPerRevolution = 1600; //200 = pro nastaveni MS1=0, MS2=0, MS3=0
const int transmittionRate = 1; //toto je pro prevod na vystupu

const int stepDelay = 5400; //5.4s = 5400ms = 5400 000 milisec
const int delayPause = 2700000;

int incomingByte = 0; // for incoming serial data
String a;

double actualAngle;

void setup()
{
  Serial.begin(9600);
  // Declare pins as Outputs
  pinMode(stepPin, OUTPUT);
  pinMode(dirPin, OUTPUT);
  pinMode(6, OUTPUT); //Enable
  digitalWrite(6,LOW);   

  Serial.println("Start...");

  //aktualni azimut uhlu je např.
  actualAngle = 45.50;

  Serial.println("End...");
  delay(1000); // Wait a second
  
}

int GetNumberOfSteps (double actualAngle,double destinationAngle ) {
   int numberSteps;
 
  double uhlovyRozdil = destinationAngle-actualAngle;

  numberSteps = uhlovyRozdil*stepsPerRevolution*transmittionRate/360; //TODO umi to prevest na int?
  return abs(numberSteps);
}

void MoveTo(int numberSteps, int delaySpeed, bool isClockWiseDirection) {
  Serial.println("MoveTo start:");
  
  if (isClockWiseDirection) {
      digitalWrite(dirPin, HIGH);
  }
  else {
    digitalWrite(dirPin, LOW);  
  }
  
  for(int x = 0; x < numberSteps; x++)
  {
    //Serial.println(x);
    digitalWrite(stepPin, HIGH);
    delay(delaySpeed); //TODO: mozna tohle bude lepsi pouzit? delayMicroseconds(delayPause);
    digitalWrite(stepPin, LOW);
    delay(delaySpeed);
  }
  Serial.println("MoveTo end");
}

void loop()
{
  
int delaySpeed = 10;

while(Serial.available()) {
  a= Serial.readString();// read the incoming data as string
  Serial.println(a);
  double requiredAngle = a.toDouble();

  
  double destinationAngle = requiredAngle;
  bool isClockWiseDirection = true;
  if ((destinationAngle-actualAngle)<0) {
    isClockWiseDirection = false;
  }
  int numberSteps = GetNumberOfSteps(actualAngle,requiredAngle);
  Serial.println("Parametry pohybu: numberSteps"+String(numberSteps)+"|actualAngle:"+String(actualAngle)+"|requiredAngle:"+String(requiredAngle)+"|isClockWiseDirection:"+String(isClockWiseDirection));
  
  MoveTo(numberSteps,delaySpeed,isClockWiseDirection);
  
  actualAngle=destinationAngle;
  Serial.println("Moving succesfull: actualangle = "+String(actualAngle));
}

//delay(1000); // Wait a second
}
