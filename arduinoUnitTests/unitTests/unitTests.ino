#define pir 2
#define MQ2pin 0
#define TEMP 0
#define HUD 1
#define RELAY 3
#define SWITCH 9
#define CO2E 1
#define CO2T 0

//STEP of co2
float V1 = 0.9060;
float V2 = 0.8345;
float V3 = 0.7557;
int STEP_status = 0;
float STEP_CO2 = 0.0;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  //pinMode(pir, INPUT);
  pinMode(RELAY, OUTPUT);
  pinMode(SWITCH, INPUT_PULLUP);

  Serial.println("Sensor ready");
}

void loop() {
 digitalWrite(RELAY, HIGH);
}

void motionDetectChecker()
{
  if(digitalRead(pir) == HIGH)
  {
    Serial.println("Motion Detected");    
  }
  else
  {
    Serial.println("No one is there");
  } 
  delay(100);
}

// https://lastminuteengineers.com/mq2-gas-senser-arduino-tutorial/
void MQ2SmokeDetect()
{
float sensorVal = analogRead(MQ2pin);
  Serial.print("Sensor Value: ");
  Serial.println(sensorVal);
  delay(1500);
}

// https://docs.k-allsensing.com/t_h:eth-01dv
void EHT_Sensor()
{
  float temp = analogRead(TEMP);
  float hud = analogRead(HUD);
  temp = -66.875 + 218.75 * (temp / 1024);
  hud = -12.5 + 125 * (hud / 1024);
  Serial.print("Temp: ");
  Serial.print(temp);
  Serial.println("'C");

  Serial.print("Humidity: ");
  Serial.print(hud);
  Serial.println("%");
  delay(2000);
}