/**
 * Code for the base station that the custom controller connects to.
 * 
 * All of the Bluetooth functionality comes from the "Server" example in
 * the Arduino "BLE" library
 * 
 * @author Samuel Gerstner
 */

#include "BLEDevice.h"
//#include "BLEScan.h"

// Inputs/outputs for the buttons and display LEDs
#define MP_LED A9
#define PA_LED A10
#define MP_BUTTON 44
#define PA_BUTTON 7

// Timer for blinking the LED when controller is not connected
int blink_timer = 0;

uint8_t is_connected = 0;

// State for whether the Micropipette is connected
uint8_t MP_STATE = 0;

// State for whether the Pipette Aid is connected
uint8_t PA_STATE = 0;

BLEScan *pBLEScan;

// Controller to connect to. 0 = Pipette Aid, 1 = Micropipette
uint8_t selected_controller = 0;

// The remote service we wish to connect to.
static BLEUUID serviceUUID("4fafc201-1fb5-459e-8fcc-c5c9c331914b");
// The characteristic of the remote service we are interested in.
static BLEUUID charUUID("beb5483e-36e1-4688-b7f5-ea07361b26a8");

static boolean doConnect = false;
static boolean connected = false;
static boolean doScan = false;
static BLERemoteCharacteristic *pRemoteCharacteristic;
static BLEAdvertisedDevice *myDevice;

static void notifyCallback(BLERemoteCharacteristic *pBLERemoteCharacteristic, uint8_t *pData, size_t length, bool isNotify) {
  //Serial.print("Notify callback for characteristic ");
  //Serial.print(pBLERemoteCharacteristic->getUUID().toString().c_str());
  //Serial.print(" of data length ");
  //Serial.println(length);
  //Serial.print("data: ");
  //Serial.write(pData, length);
  Serial.println();
}

class MyClientCallback : public BLEClientCallbacks {
  void onConnect(BLEClient *pclient) {}

  void onDisconnect(BLEClient *pclient) {
    connected = false;
    Serial.println();
  }
};

bool connectToServer() {
  //Serial.print("Forming a connection to ");
  //Serial.println(myDevice->getAddress().toString().c_str());

  BLEClient *pClient = BLEDevice::createClient();
  //Serial.println(" - Created client");

  pClient->setClientCallbacks(new MyClientCallback());

  // Connect to the remove BLE Server.
  pClient->connect(myDevice);  // if you pass BLEAdvertisedDevice instead of address, it will be recognized type of peer device address (public or private)
  //Serial.println(" - Connected to server");
  pClient->setMTU(517);  //set client to request maximum MTU from server (default is 23 otherwise)

  // Obtain a reference to the service we are after in the remote BLE server.
  BLERemoteService *pRemoteService = pClient->getService(serviceUUID);
  if (pRemoteService == nullptr) {
    //Serial.print("Failed to find our service UUID: ");
    //Serial.println(serviceUUID.toString().c_str());
    pClient->disconnect();
    return false;
  }
  //Serial.println(" - Found our service");

  // Obtain a reference to the characteristic in the service of the remote BLE server.
  pRemoteCharacteristic = pRemoteService->getCharacteristic(charUUID);
  if (pRemoteCharacteristic == nullptr) {
    //Serial.print("Failed to find our characteristic UUID: ");
    //Serial.println(charUUID.toString().c_str());
    pClient->disconnect();
    return false;
  }
  //Serial.println(" - Found our characteristic");

  // Read the value of the characteristic.
  if (pRemoteCharacteristic->canRead()) {
    String value = pRemoteCharacteristic->readValue();
    //Serial.print("The characteristic value was: ");
    //Serial.println(value.c_str());
  }

  if (pRemoteCharacteristic->canNotify()) {
    pRemoteCharacteristic->registerForNotify(notifyCallback);
  }

  connected = true;
  return true;
}
/**
 * Scan for BLE servers and find the first one that advertises the service we are looking for.
 */
class MyAdvertisedDeviceCallbacks : public BLEAdvertisedDeviceCallbacks {
  /**
   * Called for each advertising BLE server.
   */
  void onResult(BLEAdvertisedDevice advertisedDevice) {
    //Serial.print("BLE Advertised Device found: ");
    //Serial.println(advertisedDevice.toString().c_str());

    // We have found a device, let us now see if it contains the service we are looking for.
    if (advertisedDevice.haveServiceUUID() && advertisedDevice.isAdvertisingService(serviceUUID)) {

      BLEDevice::getScan()->stop();
      myDevice = new BLEAdvertisedDevice(advertisedDevice);
      doConnect = true;
      doScan = true;

    }  // Found our server
  }  // onResult
};  // MyAdvertisedDeviceCallbacks

void setup() {
  // Begin Serial port at baud rate of 115200
  Serial.begin(115200);
  
  //Serial.println("Starting Arduino BLE Client application...");
  BLEDevice::init("");

  // Retrieve a Scanner and set the callback we want to use to be informed when we
  // have detected a new device.  Specify that we want active scanning and start the
  // scan to run for 5 seconds.
  pBLEScan = BLEDevice::getScan();
  pBLEScan->setAdvertisedDeviceCallbacks(new MyAdvertisedDeviceCallbacks());
  pBLEScan->setInterval(1349);
  pBLEScan->setWindow(449);
  pBLEScan->setActiveScan(true);
  pBLEScan->start(1, false);

  // Configure Input/Output pins
  pinMode(MP_LED, OUTPUT);
  pinMode(PA_LED, OUTPUT);
  pinMode(MP_BUTTON, INPUT);
  pinMode(PA_BUTTON, INPUT);

  is_connected = 0;
  selected_controller = 0;
}  // End of setup.

// This is the Arduino main loop function.
void loop() {

  // Check if user has input to change which controller to connect to
  if (digitalRead(PA_BUTTON) && selected_controller == 1) {
    //Serial.println("Switch to PA");
    selected_controller = 0;
  } else if (digitalRead(MP_BUTTON) && selected_controller == 0) {
    //Serial.println("Switch to MP");
    selected_controller = 1;
  }

  // Turn on Pipette Aid LED if connected
  if (connected && selected_controller == 0) {
    analogWrite(PA_LED, 255);
    PA_STATE = 255;

    // Turn on Micropipette LED if connected
  } else if (connected && selected_controller == 1) {
    analogWrite(MP_LED, 255);
    PA_STATE = 255;

    // If neither connected, blink the associated LED
  } else if (selected_controller == 0) {
    if (millis() - blink_timer >= 500) {
      if (PA_STATE == 255) {
        analogWrite(PA_LED, 0);
        PA_STATE = 0;
      } else if (PA_STATE == 0) {
        analogWrite(PA_LED, 255);
        PA_STATE = 255;
      }
      blink_timer = millis();
    }
    MP_STATE = 0;
    
  } else if (selected_controller == 1) {
    if (millis() - blink_timer >= 500) {
      if (MP_STATE == 255) {
        analogWrite(MP_LED, 0);
        MP_STATE = 0;
      } else if (MP_STATE == 0) {
        analogWrite(MP_LED, 255);
        MP_STATE = 255;
      }
      blink_timer = millis();
    }
    PA_STATE = 0;
    
  }

  // Turn off the LED of the controller that's not selected
  if (selected_controller == 0) {
    analogWrite(MP_LED, 0);
    MP_STATE = 0;
  } 
  if (selected_controller == 1) {
    analogWrite(PA_LED, 0);
    PA_STATE = 0;
  }

  // If the flag "doConnect" is true then we have scanned for and found the desired
  // BLE Server with which we wish to connect.  Now we connect to it.  Once we are
  // connected we set the connected flag to be true.
  if (doConnect == true) {
    if (connectToServer()) {
      //Serial.println("We are now connected to the BLE Server.");
      Serial.println();
    } else {
      //Serial.println("We have failed to connect to the server; there is nothing more we will do.");
      Serial.println();
    }
    doConnect = false;
  }

  // If we are connected to a peer BLE Server, update the characteristic each time we are reached
  // with the current time since boot.
  if (connected) {

    // Get the most recent value output by the controller then write to the Serial Port
    if (pRemoteCharacteristic->canRead()) {
      String value = pRemoteCharacteristic->readValue();
      Serial.println(value.c_str());
    }

    if (pRemoteCharacteristic->canNotify()) {
      pRemoteCharacteristic->registerForNotify(notifyCallback);
    }
  } else {
    // If controller not connected, output zeros and search for controller
    Serial.println("S,0,0,0,0,E");
    pBLEScan->start(1, false);
  }

  delay(10);  // Delay a second between loops.
}  // End of loop
