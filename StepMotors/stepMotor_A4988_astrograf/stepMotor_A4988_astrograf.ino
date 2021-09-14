#include <SoftwareSerial.h>

#define LF          0x0A 

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

int incomingByte = 0; // for incoming serial data
String stelariumCmd;

double actualAzumutAngle;

char angle_str[10]; 
int idx; 

String inputString = "";         // a string to hold incoming data
boolean stringComplete = false;  // whether the string is complete
String commandString = "";

SoftwareSerial mySerial(7, 8); // RX, TX

void setup()
{
  Serial.begin(9600);
  mySerial.begin(9600);
  // Declare pins as Outputs
  pinMode(stepPin_AzimutMotor, OUTPUT);
  pinMode(dirPin_AzimutMotor, OUTPUT);
  pinMode(enPin_AzimutMotor, OUTPUT); //Enable power (nebo co)
  digitalWrite(enPin_AzimutMotor,LOW);   

  Serial.println("Start...");
  mySerial.println("Start bluettoh terminal ");
  
  //aktualni azimut uhlu je např.
  actualAzumutAngle = 45.50;

  Serial.println("End...");
  delay(1000); // Wait a second
  
}

int GetNumberOfSteps (double actualAzumutAngle,double destinationAngle ) {
   int numberSteps;
 
  double uhlovyRozdil = destinationAngle-actualAzumutAngle;

  numberSteps = uhlovyRozdil*stepsPerRevolution*transmittionRate/360; //TODO umi to prevest na int?
  return abs(numberSteps);
}

void MoveTo(int numberSteps, int delaySpeed, bool isClockWiseDirection) {
  Serial.println("MoveTo start:");
  
  if (isClockWiseDirection) {
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
  Serial.println("MoveTo end");
}

void loop()
{


  //2 režimy: pohybuju s dalekhohele a na Stelariu koukám, na co koukám nebo zadám na co chci koukat a z konzolovky každou sekundu posílám příkazy na polohu motorků
  //SET - toto nastavuje motorky: AZ:{aLT_AZIM_Values.Azim}|ALT:{aLT_AZIM_Values.ALt}
  //GET -> meco bude odpovidat na soucasnou polohu dalekohledu

  
//podle dat se nastavi rotace na krokových motorech:
if(stringComplete)
{
    stringComplete = false;
    mySerial.println("Data:");
    mySerial.println(inputString);

  
    double requiredAngle = inputString.toDouble();
    
    //parsing dat:
    // $"AZ:{Math.Round((Double)aLT_AZIM_Values.Azim, 4)}|ALT:{Math.Round((Double)aLT_AZIM_Values.ALt, 4)}";

    int ind1 = inputString.indexOf('|');  //finds location of first ,
  
    String azRaw = inputString.substring(0, ind1); //AZ:aLT_AZIM_Values.Azim 
    String azValueRaw;
    azValueRaw = azRaw.substring(3, azRaw.length());
  
     String altRaw = inputString.substring(ind1+1, inputString.length());
    String altValueRaw;
    altValueRaw = altRaw.substring(4, altRaw.length());
    
    //upravy:
    azValueRaw.replace(',', '.');
    altValueRaw.replace('-','+');
    altValueRaw.replace(',', '.');
    
    double requiredAzimutAngle = azValueRaw.toDouble();
    double altitudeAngle = altValueRaw.toDouble();
//
//    mySerial.println("azimutAngle:");
//    mySerial.println(azimutAngle);
//
//    mySerial.println("requiredAzimutAngle:");
//    mySerial.println(requiredAzimutAngle);
    
    //pojezd motorku:
    int delaySpeed = 10;
    double destinationAzimutAngle = requiredAzimutAngle;
    bool isClockWiseDirection = true;
    if ((destinationAzimutAngle-actualAzumutAngle)<0) {
      isClockWiseDirection = false;
    }
    int numberSteps = GetNumberOfSteps(actualAzumutAngle,requiredAzimutAngle);
    if (numberSteps>0) {
          mySerial.println("Parametry azimut pohybu: numberSteps"+String(numberSteps)+" | actualAzimutAngle:"+String(actualAzumutAngle)+" | requiredAngle:"+String(requiredAzimutAngle)+" | isClockWiseDirection:"+String(isClockWiseDirection));
          MoveTo(numberSteps,delaySpeed,isClockWiseDirection);
          actualAzumutAngle=requiredAzimutAngle;
      }

    
    inputString = "";
}


//smycka pro port ze Stelaria:
//while(Serial.available()) {
//  stelariumCmd= Serial.readString();// read the incoming data as string toto načítá 
//
//  //TODO: parsing prikazu!
//  Serial.println(String(stelariumCmd));
// 
//  int ind1 = stelariumCmd.indexOf('|');  //finds location of first ,
//
//  String azRaw = stelariumCmd.substring(0, ind1); //AZ:aLT_AZIM_Values.Azim 
//  String azValueRaw;
//  azValueRaw = azRaw.substring(3, azRaw.length());
//
//    String altRaw = stelariumCmd.substring(ind1+1, stelariumCmd.length()); //ALT:aLT_AZIM_Values.ALt 
//  String altValueRaw;
//  altValueRaw = altValueRaw.substring(4, altValueRaw.length());
//  
//  mySerial.println("azValueRaw:");
//   mySerial.println(azValueRaw);
//  mySerial.println("altValueRaw:");
//   mySerial.println(altValueRaw);

//}

//delay(1000); // Wait a second
}

void serialEvent() {
  while (Serial.available()) {
    //mySerial.println("serialEvent");//loguje každý prijaty znak
    // get the new byte:
    char inChar = (char)Serial.read();
    // add it to the inputString:
    inputString += inChar;
    // if the incoming character is a newline, set a flag
    // so the main loop can do something about it:
    if (inChar == '\n') {
      stringComplete = true;
    }
  }
}
