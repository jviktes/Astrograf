#include <SoftwareSerial.h>

//**********************************************
// Config your sensor pins and location here!
//**********************************************

// Define the number of pulses that your encoder (1 and 2) gives by turn, and multiply by 4
// In my case: 600 x 15 x 4  (600 pulses by turn; 15 is the gear ratio), so:
long TOTAL_PULSES_PER_ROUND_ENC_1 = 36000;              
long TOTAL_PULSES_PER_ROUND_ENC_2 = 36000;             

// Define DUE's pins
#define ENC_1A 2   // define DUE pin to encoder 1-channel A                     
#define ENC_1B 3   // define DUE pin to encoder 1-channel B  
#define ENC_2A 4   // define DUE pin to encoder 2-channel A  
#define ENC_2B 5   // define DUE pin to encoder 2-channel B  

// enter your latitude (example: North 40Âº33'20'') 
//Longitude will be calculated through AR and H.
int POSITION_LATITUDE_HH = 50;    // this means 40Âº North
int POSITION_LATITUDE_MM = 45;
int lPOSITION_LATITUDE_SS = 45;

// enter Pole Star right ascention (AR: HH:MM:SS)
int POLARKA_AR_HH = 2;    // this means 2 hours, 52 minutes and 16 seconds
int POLARKA_AR_MM = 58;
int POLARKA_AR_SS = 30;

// enter Pole Star hour angle (H: HH:MM:SS)
int POLARKA_HOUR_CLOCK_HH = 15;
int POLARKA_HOUR_CLOCK_MM = 33;
int POLARKA_HOUR_CLOCK_SS = 24;

SoftwareSerial mySerial(7, 8); // RX, TX - toto slouzi je pro vypis, co posilam na stelarium

unsigned long SEG_SIDERAL = 1003;
const double      CONST_PI = 3.14159265358979324;

volatile int lastEncoded1 = 0;
volatile long encoderValue1 = 0;

volatile int lastEncoded2 = 0;
volatile long encoderValue2 = 0;

char _input_from_Stelarium[20];
//vystupem jsou _AR_Formated_Stelarium a _DEC_Formated_Stelarium = naformatovane hodnoty souradnic v EQ
char _AR_Formated_Stelarium[10];
char _DEC_Formated_Stelarium[11];

long TSL;
unsigned long t_ciclo_acumulado = 0, time_running_arduino;

long Az_tel_s, Alt_tel_s;

long AR_tel_s, DEC_tel_s;

double cos_phi, sin_phi;

//--------------------------------------------------------------------------------------------------------------------------------------------------------
void setup()
{
  Serial.begin(9600);
  
  pinMode(ENC_1A, INPUT_PULLUP);
  pinMode(ENC_1B, INPUT_PULLUP);
  pinMode(ENC_2A, INPUT_PULLUP);
  pinMode(ENC_2B, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(ENC_1A), Encoder_Interrupt_1, CHANGE);
  attachInterrupt(digitalPinToInterrupt(ENC_1B), Encoder_Interrupt_1, CHANGE);
  attachInterrupt(digitalPinToInterrupt(ENC_2A), Encoder_Interrupt_2, CHANGE);
  attachInterrupt(digitalPinToInterrupt(ENC_2B), Encoder_Interrupt_2, CHANGE);

  cos_phi = cos((((POSITION_LATITUDE_HH * 3600) + (POSITION_LATITUDE_MM * 60) + lPOSITION_LATITUDE_SS) / 3600.0) * CONST_PI / 180.0);
  sin_phi = sin((((POSITION_LATITUDE_HH * 3600) + (POSITION_LATITUDE_MM * 60) + lPOSITION_LATITUDE_SS) / 3600.0) * CONST_PI / 180.0);

  TSL = POLARKA_AR_HH * 3600 + POLARKA_AR_MM * 60 + POLARKA_AR_SS + POLARKA_HOUR_CLOCK_HH * 3600 + POLARKA_HOUR_CLOCK_MM * 60 + POLARKA_HOUR_CLOCK_SS;
  while (TSL >= 86400) TSL = TSL - 86400;


  mySerial.begin(9600);
  
  Serial.println("setup ok");
  mySerial.write("setup ok");

  
}

//--------------------------------------------------------------------------------------------------------------------------------------------------------
void loop()
{
  time_running_arduino = millis(); //cas od spusteni arduina
  if (t_ciclo_acumulado >= SEG_SIDERAL) {
    TSL++;
    t_ciclo_acumulado = t_ciclo_acumulado - SEG_SIDERAL;
    if (TSL >= 86400) {
      TSL = TSL - 86400;
    }
  }

  read_sensors();
  AZ_to_EQ();

  if (Serial.available() > 0) 
  {communication();}

  time_running_arduino = millis() - time_running_arduino;
  t_ciclo_acumulado = t_ciclo_acumulado + time_running_arduino;
  
}

//--------------------------------------------------------------------------------------------------------------------------------------------------------
void communication()
{
  mySerial.write("communication:");

  int i = 0;

  _input_from_Stelarium[i++] = Serial.read();
  delay(5);


  //While connected to Arduino, periodically Stellarium sends 2 strings: ":GR#" - ready to receive RA and ":GD#" - to receive DEC.

  while ((_input_from_Stelarium[i++] = Serial.read()) != '#') {
    delay(5);
  }
  _input_from_Stelarium[i] = '\0';

    mySerial.write("_AR_Formated_Stelarium");
    mySerial.write(_AR_Formated_Stelarium);
    mySerial.write("_DEC_Formated_Stelarium");
    mySerial.write(_DEC_Formated_Stelarium);
    
  if (_input_from_Stelarium[1] == ':' && _input_from_Stelarium[2] == 'G' && _input_from_Stelarium[3] == 'R' && _input_from_Stelarium[4] == '#') {
    Serial.print(_AR_Formated_Stelarium);

  }

  if (_input_from_Stelarium[1] == ':' && _input_from_Stelarium[2] == 'G' && _input_from_Stelarium[3] == 'D' && _input_from_Stelarium[4] == '#') {
    Serial.print(_DEC_Formated_Stelarium);
  }
}

//--------------------------------------------------------------------------------------------------------------------------------------------------------
void read_sensors() {

    //fake:
    encoderValue2= encoderValue2+200;
    delay(1000);

  if (encoderValue2 >= TOTAL_PULSES_PER_ROUND_ENC_2 || encoderValue2 <= -TOTAL_PULSES_PER_ROUND_ENC_2) {
    encoderValue2 = 0;
  }
  
  //Alta teleskop:
  int _enc_1 = encoderValue1 / 1500;
  long encoder1_temp = encoderValue1 - (_enc_1 * 1500);
  long map1 = _enc_1 * map(1500, 0, TOTAL_PULSES_PER_ROUND_ENC_1, 0, 324000); //map(value, fromLow, fromHigh, toLow, toHigh)
  Alt_tel_s = map1 + map(encoder1_temp, 0, TOTAL_PULSES_PER_ROUND_ENC_1, 0, 324000);

  //Azimut teleskop:
  int _enc_2 = encoderValue2 / 1500;
  long encoder2_temp = encoderValue2 - (_enc_2 * 1500);

  long map2 = _enc_2 * map(1500, 0, TOTAL_PULSES_PER_ROUND_ENC_2, 0, 1296000);
  Az_tel_s  = map2 + map (encoder2_temp, 0, TOTAL_PULSES_PER_ROUND_ENC_2, 0, 1296000);

  if (Az_tel_s < 0) Az_tel_s = 1296000 + Az_tel_s;
  if (Az_tel_s >= 1296000) Az_tel_s = Az_tel_s - 1296000 ;

}

//Metoda volaná pri zmene v pulzu, preruseni:
void Encoder_Interrupt_1() {
  int encoded1 = (digitalRead(ENC_1A) << 1) | digitalRead(ENC_1B);
  int sum  = (lastEncoded1 << 2) | encoded1;
  if (sum == 0b1101 || sum == 0b0100 || sum == 0b0010 || sum == 0b1011) encoderValue1 ++;
  if (sum == 0b1110 || sum == 0b0111 || sum == 0b0001 || sum == 0b1000) encoderValue1 --;
  lastEncoded1 = encoded1;
}

//Metoda volaná pri zmene v pulzu, preruseni:
void Encoder_Interrupt_2() {
  int encoded2 = (digitalRead(ENC_2A) << 1) | digitalRead(ENC_2B);
  int sum  = (lastEncoded2 << 2) | encoded2;

  if (sum == 0b1101 || sum == 0b0100 || sum == 0b0010 || sum == 0b1011) encoderValue2 ++;
  if (sum == 0b1110 || sum == 0b0111 || sum == 0b0001 || sum == 0b1000) encoderValue2 --;
  lastEncoded2 = encoded2;
}

//--------------------------------------------------------------------------------------------------------------------------------------------------------
//vystupem jsou _AR_Formated_Stelarium a _DEC_Formated_Stelarium = naformatovane hodnoty souradnic v EQ
void AZ_to_EQ()
{
  double delta_tel, sin_h, cos_h, sin_A, cos_A, sin_DEC, cos_DEC;
  double H_telRAD, h_telRAD, A_telRAD;
  long H_tel;
  long arHH, arMM, arSS;
  long decDEG, decMM, decSS;
  char sDEC_tel;

  A_telRAD = (Az_tel_s / 3600.0) * CONST_PI / 180.0;
  h_telRAD = (Alt_tel_s / 3600.0) * CONST_PI / 180.0;
  sin_h = sin(h_telRAD);
  cos_h = cos(h_telRAD);
  sin_A = sin(A_telRAD);
  cos_A = cos(A_telRAD);
  delta_tel = asin((sin_phi * sin_h) + (cos_phi * cos_h * cos_A));
  sin_DEC = sin(delta_tel);
  cos_DEC = cos(delta_tel);
  DEC_tel_s = long((delta_tel * 180.0 / CONST_PI) * 3600.0);

  while (DEC_tel_s >= 324000) {
    DEC_tel_s = DEC_tel_s - 324000;
  }
  while (DEC_tel_s <= -324000) {
    DEC_tel_s = DEC_tel_s + 324000;
  }

  H_telRAD = acos((sin_h - (sin_phi * sin_DEC)) / (cos_phi * cos_DEC));
  H_tel = long((H_telRAD * 180.0 / CONST_PI) * 240.0);

  if (sin_A >= 0) {
    H_tel = 86400 - H_tel;
  }
  AR_tel_s = TSL - H_tel;

  while (AR_tel_s >= 86400) {
    AR_tel_s = AR_tel_s - 86400;
  }
  while (AR_tel_s < 0) {
    AR_tel_s = AR_tel_s + 86400;
  }

  arHH = AR_tel_s / 3600;
  arMM = (AR_tel_s - arHH * 3600) / 60;
  arSS = (AR_tel_s - arHH * 3600) - arMM * 60;
  decDEG = abs(DEC_tel_s) / 3600;
  decMM = (abs(DEC_tel_s) - decDEG * 3600) / 60;
  decSS = (abs(DEC_tel_s) - decDEG * 3600) - decMM * 60;
  (DEC_tel_s < 0) ? sDEC_tel = 45 : sDEC_tel = 43;

  //vystupem jsou _AR_Formated_Stelarium a _DEC_Formated_Stelarium = naformatovane hodnoty souradnic v EQ
  sprintf(_AR_Formated_Stelarium, "%02d:%02d:%02d#", int(arHH), int(arMM), int(arSS));
  sprintf(_DEC_Formated_Stelarium, "%c%02d%c%02d:%02d#", sDEC_tel, int(decDEG), 223, int(decMM), int(decSS));
  
}
