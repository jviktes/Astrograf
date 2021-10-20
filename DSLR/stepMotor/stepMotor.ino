#include <SoftwareSerial.h>

#define LF 0x0A 

//test prepisu
//https://lastminuteengineers.com/a4988-stepper-motor-driver-arduino-tutorial/
// Define pin connections & motor's steps per revolution

const int dirPin_AzimutMotor = 2;
const int stepPin_AzimutMotor = 3;
const int enPin_AzimutMotor=4;

const int stepsPerRevolution = 1600; //200 = pro nastaveni MS1=0, MS2=0, MS3=0
const double transmittionRate = 1;//76/16; //toto je pro prevod na vystupu 

const int delayPause = 200;//3800; //polovina 54000 = bez převodu ozub koly, 7600 s prevodem 76/16
int counter = 0;

SoftwareSerial myBluetooth(7, 8); // RX, TX

void setup()
{
  Serial.begin(9600);
  myBluetooth.begin(9600);
  
  // Declare pins as Outputs
  pinMode(stepPin_AzimutMotor, OUTPUT);
  pinMode(dirPin_AzimutMotor, OUTPUT);
  pinMode(enPin_AzimutMotor, OUTPUT); //Enable power (nebo co)
  digitalWrite(enPin_AzimutMotor,LOW);   

  Serial.println("Starting...");
  myBluetooth.println("Start bluettoh terminal ");

  digitalWrite(dirPin_AzimutMotor, HIGH);
  
  Serial.println("End setup...");
  delay(200); // Wait a second
  
}



void loop()
{
  // Spin motor slowly
  //for(int x = 0; x < stepsPerRevolution; x++)
  //{
  Serial.println("loop:");
  
    digitalWrite(stepPin_AzimutMotor, HIGH);
    delay(delayPause);
    digitalWrite(stepPin_AzimutMotor, LOW);
    //delayMicroseconds(stepDelay);
    delay(delayPause);
    counter++;
    Serial.println(String(counter));
  //}
  //delay(1000); // Wait a second 
  Serial.println("end loop:");
}

int GetNumberOfSteps (double actualAzimutAngle,double destinationAngle ) {
   int numberSteps;
 
  double uhlovyRozdil = destinationAngle-actualAzimutAngle;

  numberSteps = uhlovyRozdil*stepsPerRevolution*transmittionRate/360; //TODO umi to prevest na int?
  return abs(numberSteps);
}
