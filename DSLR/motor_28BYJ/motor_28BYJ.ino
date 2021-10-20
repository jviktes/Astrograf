//Written By Nikodem Bartnik - nikodembartnik.pl
//https://github.com/NikodemBartnik/ArduinoTutorials/tree/master/28BYJ-48

#define STEPPER_PIN_1 9
#define STEPPER_PIN_2 10
#define STEPPER_PIN_3 11
#define STEPPER_PIN_4 12

int inner_step_number = 0;
int stepCount = 0;
double anglePerStep = 5.625;
int gearRatio = 32; //převod přímo v motoru

int totalNumberStepsForOneRevolution = 2048; 
// 360:5,625 = 64 (=počet kroků na 360stupnu bez převodu, 64*32(převod) = 2048 kroků na 1 celou otočku

// * převod 76/16
double extTransmittionGera = 1; //=9728
int delayStepMotor;


void setup() {
  Serial.begin(9600);
  Serial.println("Starting...");
pinMode(STEPPER_PIN_1, OUTPUT);
pinMode(STEPPER_PIN_2, OUTPUT);
pinMode(STEPPER_PIN_3, OUTPUT);
pinMode(STEPPER_PIN_4, OUTPUT);
extTransmittionGera = 76.000/16.000; //=9728
delayStepMotor =8881;//25;//8881 ;//(60*1000)/(totalNumberStepsForOneRevolution*extTransmittionGera);
Serial.println("delayStepMotor:");
Serial.println(delayStepMotor);
}

void loop() {

    OneStep(true);
    
    inner_step_number++;
    if(inner_step_number > 3){
    inner_step_number = 0;
    }
    
    //delay(delayStepMotor);
    delay(delayStepMotor);
    Serial.println("loop");
}


void OneStep(bool dir){
    if(dir){
switch(inner_step_number){
  case 0:
  digitalWrite(STEPPER_PIN_1, HIGH);
  digitalWrite(STEPPER_PIN_2, LOW);
  digitalWrite(STEPPER_PIN_3, LOW);
  digitalWrite(STEPPER_PIN_4, LOW);
  break;
  case 1:
  digitalWrite(STEPPER_PIN_1, LOW);
  digitalWrite(STEPPER_PIN_2, HIGH);
  digitalWrite(STEPPER_PIN_3, LOW);
  digitalWrite(STEPPER_PIN_4, LOW);
  break;
  case 2:
  digitalWrite(STEPPER_PIN_1, LOW);
  digitalWrite(STEPPER_PIN_2, LOW);
  digitalWrite(STEPPER_PIN_3, HIGH);
  digitalWrite(STEPPER_PIN_4, LOW);
  break;
  case 3:
  digitalWrite(STEPPER_PIN_1, LOW);
  digitalWrite(STEPPER_PIN_2, LOW);
  digitalWrite(STEPPER_PIN_3, LOW);
  digitalWrite(STEPPER_PIN_4, HIGH);
  break;
} 
  }else{
    switch(inner_step_number){
  case 0:
  digitalWrite(STEPPER_PIN_1, LOW);
  digitalWrite(STEPPER_PIN_2, LOW);
  digitalWrite(STEPPER_PIN_3, LOW);
  digitalWrite(STEPPER_PIN_4, HIGH);
  break;
  case 1:
  digitalWrite(STEPPER_PIN_1, LOW);
  digitalWrite(STEPPER_PIN_2, LOW);
  digitalWrite(STEPPER_PIN_3, HIGH);
  digitalWrite(STEPPER_PIN_4, LOW);
  break;
  case 2:
  digitalWrite(STEPPER_PIN_1, LOW);
  digitalWrite(STEPPER_PIN_2, HIGH);
  digitalWrite(STEPPER_PIN_3, LOW);
  digitalWrite(STEPPER_PIN_4, LOW);
  break;
  case 3:
  digitalWrite(STEPPER_PIN_1, HIGH);
  digitalWrite(STEPPER_PIN_2, LOW);
  digitalWrite(STEPPER_PIN_3, LOW);
  digitalWrite(STEPPER_PIN_4, LOW);
 
  
} 
  }

}
