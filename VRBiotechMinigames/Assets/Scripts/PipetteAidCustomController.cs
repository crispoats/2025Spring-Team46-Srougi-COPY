using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO.Ports;
using System.IO;
using System.Threading.Tasks;

/*
 * 
 * @author Siri Mudunuri
 * 
 * Script to take in input from pipette aid custom controller and trigger pipette aid input event to pass input.
 * 
 */
public class PipetteAidCustomController : MonoBehaviour
{
    // The COM port the base station is connected to
    string port_name = "";

    // The baud rate of the COM port
    int port_speed = 115200;

    // The data stream for the base station
    SerialPort data_stream;

    // Value for the input from button 1
    public static float custom_button_1;

    // Value for the input from button 2
    public static float custom_button_2;

    // Flag for if the serial port is openf
    bool serial_is_open = false;

    List<string> invalid_ports = new List<string>();


    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    // Function to connect to the serial port
    void serial_connect()
    {
        // Establish the connection, open it, and set the connection boolean

        try{
            data_stream = new SerialPort(port_name, port_speed);
            data_stream.Open();
            serial_is_open = true;
        } catch (IOException){
            Debug.Log("Unable to connect");
            invalid_ports.Add(port_name);
            data_stream.Close();
            serial_is_open = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Get the list of currently available ports
        string[] ports = SerialPort.GetPortNames();
        int ports_length = ports.Length;

        // TESTING
        // if (!serial_is_open)
        // {
        //     // Simulate serial input
        //     ProcessSimulatedData();
        //     return;
        // }

        // If the port is not open, check if it's available and connect if so
        if (!serial_is_open && ports_length > 0) {
            port_name = ports[ports.Length - 1];
            if (!invalid_ports.Contains(port_name)) {
                try {
                    serial_connect();
                } catch (Exception) {
                    invalid_ports.Add(port_name);
                    data_stream.Close();
                    serial_is_open = false;
                }
            }
        } else if (serial_is_open) {
            // If the serial port is open, get the input from the port
            try {
                // Check if the port has closed; if so, close the port
                if (data_stream.BytesToRead > 0) {
                    // Get the input from the port
                    string returned_string = Serial_Data_Reading();
                    string[] values = returned_string.Split(',');

                    // If the input is valid, get & set the values for the buttons
                    if (values.Length >= 6) {
                        if (values[0] == "S") {
                            custom_button_1 = float.Parse(values[1]) / 10;
                            custom_button_2 = float.Parse(values[2]) / 10;
                            Debug.Log(custom_button_1);
                            set_values();
                        }
                    } else
                    {
                        data_stream.DiscardInBuffer();
                    }
                }
            } catch (IOException) {
                // IOException is thrown if the base station is disconnected; if so, close the data stream
                serial_is_open = false;
                data_stream.Close();
            }
        }
    }

    // TESTING
    // private void ProcessSimulatedData()
    // {
    //     string returned_string = Serial_Data_Reading();
        
    //     if (string.IsNullOrEmpty(returned_string)) return;

    //     string[] values = returned_string.Split(',');

    //     if (values.Length >= 6 && values[0] == "S")
    //     {
    //         custom_button_1 = float.Parse(values[1]) / 10;
    //         custom_button_2 = float.Parse(values[2]) / 10;
    //         Debug.Log($"Simulated Line: {returned_string}");
    //         Debug.Log($"Button 1: {custom_button_1}, Button 2: {custom_button_2}");
    //         set_values();
    //     }
    // }


    // Function to set the values for aspiration speed, as well as button inputs
    void set_values()
    {
        // If button 1 is pressed, invoke event and pass "Aspiration" and aspiration (button 1) speed
        if (custom_button_1 > 1) {
            //Debug.Log($"Invoking Aspiration: {custom_button_1}");
            PipetteAidInputEvent.GetInstance().Invoke("Aspiration", custom_button_1);
        } else if (custom_button_2 > 1) {
        // If button 2 is pressed, invoke event and pass "Dispersion" and dispersion (button 1) speed
            //Debug.Log($"Invoking Dispersion: {custom_button_2}");
            PipetteAidInputEvent.GetInstance().Invoke("Dispersion", custom_button_2);
        // If neither button 1 or button 2 is pressed, pass "None" and a the default liquid flow rate of 0.05
        } else {
           //Debug.Log("Invoking None");
           PipetteAidInputEvent.GetInstance().Invoke("None", 0.05f);
        }
    }
    
    // TESTING
    // private List<string> simulatedLines = new List<string>();
    // private int currentLineIndex = 0;

    string Serial_Data_Reading()
    {
        // TESTING
        // if (simulatedLines.Count == 0)
        // {
        //     string filePath = Path.Combine(Application.streamingAssetsPath, "customInputTest.txt");
        //     string fullText = File.ReadAllText(filePath);
        //     simulatedLines = new List<string>(fullText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries));
        // }

        // // Return next line or empty string
        // if (currentLineIndex < simulatedLines.Count)
        // {
        //     return simulatedLines[currentLineIndex++];
        // }
        // else
        // {
        //     currentLineIndex = 0; // Reset for looping
        //     return string.Empty;
        // }
        return data_stream.ReadLine();
    }

}
