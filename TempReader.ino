#include <Adafruit_Sensor.h>
//#include <DHT.h>
#include <DHT_U.h>
#include <LiquidCrystal.h>

const int ldrPin = A0;
const int rs = 12, en = 11, d4 = 5, d5 = 4, d6 = 3, d7 = 2;
LiquidCrystal lcd(rs, en, d4, d5, d6, d7);

#define DHTPIN 6
#define DHTTYPE DHT11

DHT_Unified dht(DHTPIN, DHTTYPE);

void setup() {
  Serial.begin(9600);
  lcd.begin(16, 2);
  dht.begin();
}

void loop() {
  sensors_event_t temp;
  sensors_event_t humid;

  dht.temperature().getEvent(&temp);
  dht.humidity().getEvent(&humid);

  int ldrValue = analogRead(ldrPin);

  // Inverting the LDR value for proper readings
  ldrValue = 1023 - ldrValue;

  Serial.print(temp.temperature);
  Serial.print(",");
  Serial.print(humid.relative_humidity);
  Serial.print(",");
  Serial.print(ldrValue);
  Serial.println();

  lcd.setCursor(0, 0);
  lcd.print(F("T:"));
  lcd.print(temp.temperature);

  lcd.print(F("|H:"));
  lcd.print(humid.relative_humidity);
  lcd.print(F("%"));

  lcd.setCursor(0, 1);
  lcd.print(F("LDR: "));
  lcd.print(ldrValue);
  
  delay(10000);
}
