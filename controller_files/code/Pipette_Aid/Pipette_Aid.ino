/**
 * Code for Pipette Aid custom controller.
 * 
 * All of the Bluetooth functionality comes from the "Client" example in
 * the Arduino "BLE" library
 * 
 * @author Samuel Gerstner
*/

#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEServer.h>
#include <Adafruit_LSM6DS3TRC.h>

// See the following for generating UUIDs:
// https://www.uuidgenerator.net/

#define SERVICE_UUID        "4fafc201-1fb5-459e-8fcc-c5c9c331914b"
#define CHARACTERISTIC_UUID "beb5483e-36e1-4688-b7f5-ea07361b26a8"

// Inputs/Outputs for the button sensors and rumble motor
#define BTN_1 A9
#define BTN_2 A8
#define MOTOR A10

// For SPI mode, we need a CS pin
#define LSM_CS 10
// For software-SPI mode we need SCK/MOSI/MISO pins
#define LSM_SCK 13
#define LSM_MISO 12
#define LSM_MOSI 11

int t1 = 0;
int state = 0;

BLECharacteristic *pCharacteristic;

Adafruit_LSM6DS3TRC lsm6ds3trc;

String BLE_MESSAGE = "";

// Acceleration & orientation values, not actually used sadly
float accel_x;
float accel_y;
float accel_z;
float R;
float roll;
float pitch;

// Values for finding the minimum and maximum values for a button input
float minVal = 4000;
float maxVal = 0;

// The maximum and minimum values for button 1's analog input
// Minimum is slight below the actual minimum, maximum slightly higher than actual maximum
float btn1_max = 2200;
float btn1_min = 800;

// The maximum and minimum values for button 2's analog input
// Minimum is slight below the actual minimum, maximum slightly higher than actual maximum
float btn2_max = 2350;
float btn2_min = 880;

void setup() {
  Serial.begin(115200);
  //Serial.println("Starting BLE work!");

  // Configure the output pins for the motor and LED
  pinMode(MOTOR, OUTPUT);
  pinMode(LED_BUILTIN, OUTPUT);

  // Pulse the motor to signal that the controller is on
  digitalWrite(MOTOR, LOW);
  delay(100);
  digitalWrite(MOTOR, HIGH);
  delay(100);
  digitalWrite(MOTOR, LOW);
  delay(100);
  digitalWrite(MOTOR, HIGH);
  delay(100);
  digitalWrite(MOTOR, LOW);


  // Bluetooth setup taken from Bluetooth server example

  BLEDevice::init("Long name works now");
  BLEServer *pServer = BLEDevice::createServer();
  BLEService *pService = pServer->createService(SERVICE_UUID);
  pCharacteristic =
    pService->createCharacteristic(CHARACTERISTIC_UUID, BLECharacteristic::PROPERTY_READ | BLECharacteristic::PROPERTY_WRITE);

  pCharacteristic->setValue("");
  pService->start();
  // BLEAdvertising *pAdvertising = pServer->getAdvertising();  // this still is working for backward compatibility
  BLEAdvertising *pAdvertising = BLEDevice::getAdvertising();
  pAdvertising->addServiceUUID(SERVICE_UUID);
  pAdvertising->setScanResponse(true);
  pAdvertising->setMinPreferred(0x06);  // functions that help with iPhone connections issue
  pAdvertising->setMinPreferred(0x12);
  BLEDevice::startAdvertising();
  //Serial.println("Characteristic defined! Now you can read it in your phone!");

  // Initialize the IMU for orientation sensing
  lsm6ds3trc.begin_I2C();
  
}

void loop() {
  // Get the inputs from the buttons
  // The ADC is 12-bit so 0-4095
  float button1 = analogRead(BTN_1);
  float button2 = analogRead(BTN_2);

  // Map the input from the min-max range to 8-30 for aspiration speed
  button1 = map(button1, btn1_min, btn1_max, 8, 30);
  button2 = map(button2, btn2_min, btn2_max, 8, 30);

  // Output button values, for testing. Uncomment to use
  /**
  Serial.print("Button 1: ");
  Serial.println(button1);
  Serial.print("Button 2: ");
  Serial.println(button2);
  Serial.println();
  */



  // Uncomment this and change the button to find the min and max values
  /**
  if (button2 > maxVal) {
    maxVal = button2;
  }
  if (button2 < minVal) {
    minVal = button2;
  }

  Serial.print("Maximum: ");
  Serial.println(maxVal);
  Serial.print("Minimum: ");
  Serial.println(minVal);
  Serial.println();
  */

  // Rumble motor should turn on if input after mapping is greater than 10
  if (button1 > 10) {
    
    // Turn on the rumble motor, rumble intensity depends on button input
    // analogWrite will PWM the pin at a frequency based on input 0-255
    // Motor really doesn't turn on below ~128
    analogWrite(MOTOR, map(button1, 8, 30, 175, 255));
    
    //Serial.print("Button 1: ");
    //Serial.println(button1);
  } else if (button2 > 10) {
    
    // Turn on the rumble motor, rumble intensity depends on button input
    // analogWrite will PWM the pin at a frequency based on input 0-255
    // Motor really doesn't turn on below ~128
    analogWrite(MOTOR, map(button2, 8, 30, 175, 255));
    
    //digitalWrite(LED_BUILTIN, LOW);
    //Serial.println("Button 2: ");
    //Serial.println(button2);
  } else {

    // If neither button pressed, turn off rumble motor
    analogWrite(MOTOR, 0);
    
    //digitalWrite(LED_BUILTIN, HIGH);
  }

  // Get the values from the IMU for orientation
  sensors_event_t accel;
  sensors_event_t gyro;
  sensors_event_t temp;
  lsm6ds3trc.getEvent(&accel, &gyro, &temp);

  // Calculate acceleration values
  accel_z = accel.acceleration.x / 9.8;
  accel_y = accel.acceleration.y / 9.8;
  accel_x = accel.acceleration.z / 9.8;

  /**
  Serial.print("Accel x: ");
  Serial.println(accel_x);
  Serial.print("Accel y: ");
  Serial.println(accel_y);
  Serial.print("Accel z: ");
  Serial.println(accel_z);
  */

  // Fix orientation issues
  if (accel_x > 1) {
    accel_x = 1;
  } else if (accel_x < -1) {
    accel_x = -1;
  }

  if (accel_y > 1) {
    accel_y = 1;
  } else if (accel_y < -1) {
    accel_y = -1;
  }

  if (accel_z > 1) {
    accel_z = 1;
  } else if (accel_z < -1) {
    accel_z = -1;
  }


  // Calculate the roll and pitch of the controller
  R =  sqrt(pow(accel_x,2)+pow(accel_y,2)+pow(accel_z,2));
  pitch = atan2(accel_y, accel_z) * 180 / 3.14;
  roll = atan2(-accel_x, sqrt(accel_y*accel_y + accel_z*accel_z)) * 180 / 3.14;

  // Format the bluetooth method as a string in the following format:
  // "S, (button1), (button2), (roll), (pitch), E"
  BLE_MESSAGE = "S,";
  BLE_MESSAGE += button1;
  BLE_MESSAGE += ",";
  BLE_MESSAGE += button2;
  BLE_MESSAGE += ",";
  BLE_MESSAGE += (int)roll;
  BLE_MESSAGE += ",";
  BLE_MESSAGE += (int)pitch;
  BLE_MESSAGE += ",E";

  // Update the bluetooth value sent out by the controller
  //BLEDevice::startAdvertising();
  pCharacteristic->setValue(BLE_MESSAGE);
  delay(10);
}
