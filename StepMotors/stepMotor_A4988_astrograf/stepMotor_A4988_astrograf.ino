#include <SoftwareSerial.h>

#define LF 0x0A 

//test prepisu
//https://lastminuteengineers.com/a4988-stepper-motor-driver-arduino-tutorial/
// Define pin connections & motor's steps per revolution

const int dirPin_AzimutMotor = 2;
const int stepPin_AzimutMotor = 3;
const int enPin_AzimutMotor=4;

const int stepsPerRevolution = 1600; //200 = pro nastaveni MS1=0, MS2=0, MS3=0
const int transmittionRate = 1; //toto je pro prevod na vystupu

const int stepDelay = 5400; //5.4s = 5400ms = 5400 000 milisec
const int delayPause = 2700000;

double actualAzimutAngle;

String originalInputString = "";         // a string to hold incoming data
boolean stringComplete = false;  // whether the string is complete

static const unsigned long REFRESH_INTERVAL = 1000; // ms
static unsigned long lastRefreshTime = 0;
    
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
  
  //aktualni azimut uhlu je např.
  actualAzimutAngle =248.00; //45.50;  //toto musím nějak inicializovat, např, Jested = 248 st

  Serial.println("End setup...");
  delay(200); // Wait a second
  
}

int GetNumberOfSteps (double actualAzimutAngle,double destinationAngle ) {
   int numberSteps;
 
  double uhlovyRozdil = destinationAngle-actualAzimutAngle;

  numberSteps = uhlovyRozdil*stepsPerRevolution*transmittionRate/360; //TODO umi to prevest na int?
  return abs(numberSteps);
}

void MoveTo(int numberSteps, int delaySpeed, bool isClockWiseDirection) {
  Serial.println("MoveTo.Start(numberSteps):"+numberSteps);
  myBluetooth.println("MoveTo.Start(numberSteps):"+numberSteps);
  delay(10);
  myBluetooth.println("MoveTo.Start(isClockWiseDirection):"+isClockWiseDirection);
  delay(10);
  
  if (isClockWiseDirection==true) {
      digitalWrite(dirPin_AzimutMotor, HIGH);
  }
  else {
     digitalWrite(dirPin_AzimutMotor, LOW);  
  }
  
  for(int x = 0; x < numberSteps; x++)
  {
    //Serial.println(x);
    digitalWrite(stepPin_AzimutMotor, HIGH);
    delay(delaySpeed); //TODO: mozna tohle bude lepsi pouzit? delayMicroseconds(delayPause);
    digitalWrite(stepPin_AzimutMotor, LOW);
    delay(delaySpeed);
  }
  Serial.println("MoveTo.End");
  myBluetooth.println("MoveTo.End");
  delay(10);
}

void loop()
{

   if(millis() - lastRefreshTime >= REFRESH_INTERVAL)
    {
        lastRefreshTime += REFRESH_INTERVAL;
        myBluetooth.println("actualAzimutAngle:"+String(actualAzimutAngle));
        delay(10);
    }
    
        //podle dat se nastavi rotace na krokových motorech:
        if(stringComplete)
        {
                stringComplete = false;
                myBluetooth.println("Data:");
                myBluetooth.println(originalInputString);
            
                //parsing dat:
                // $"AZ:{Math.Round((Double)aLT_AZIM_Values.Azim, 4)}|ALT:{Math.Round((Double)aLT_AZIM_Values.ALt, 4)}";
                //AZ:191,254|ALT:47,6243
                
                int index_of_delimiter = originalInputString.indexOf('|');  //finds location of first ,
              
                String azRaw = originalInputString.substring(0, index_of_delimiter); //
                String azValueRaw = azRaw.substring(3);
            
                myBluetooth.println("azValueRaw:");
                myBluetooth.println(azValueRaw);
              
                String altRaw = originalInputString.substring(index_of_delimiter+1);
                String altValueRaw = altRaw.substring(4);
                
                delay(10);
            
                myBluetooth.println("altValueRaw:");
                myBluetooth.println(altValueRaw);
              
                //upravy:
                azValueRaw.replace(',', '.');
                altValueRaw.replace('-','+');
                altValueRaw.replace(',', '.');
                
                double requiredAzimutAngle = azValueRaw.toDouble();
                double altitudeAngle = altValueRaw.toDouble();
            
                //pojezd motorku:
                int delaySpeed = 10;
            
                bool isClockWiseDirection = true; //defaultní je ve smeru hod. rucicek, to je i ve smeru azimutu.
            
                // (requiredAzimutAngle-actualAzimutAngle)
                if ((requiredAzimutAngle-actualAzimutAngle)<0) {
                  isClockWiseDirection = false;
                  
                }
            
            
            
                  int numberSteps = GetNumberOfSteps(actualAzimutAngle,requiredAzimutAngle);
                  if (numberSteps>0) {
                        myBluetooth.println("Parametry azimut pohybu: numberSteps"+String(numberSteps)+" | actualAzimutAngle:"+String(actualAzimutAngle)+" | requiredAngle:"+String(requiredAzimutAngle)+" | isClockWiseDirection:"+String(isClockWiseDirection));
                        MoveTo(numberSteps,delaySpeed,isClockWiseDirection);
                        actualAzimutAngle=requiredAzimutAngle;
                        Serial.println("RunningTaskSlewFinished");
                  }
                  
                originalInputString = "";
        
        }
}

void serialEvent() {
  while (Serial.available()) {
    //myBluetooth.println("serialEvent");//loguje každý prijaty znak
    // get the new byte:
    char inChar = (char)Serial.read();
    // add it to the originalInputString:
    originalInputString += inChar;
    // if the incoming character is a newline, set a flag
    // so the main loop can do something about it:
    if (inChar == '\n') {
      stringComplete = true;
    }
  }
}
