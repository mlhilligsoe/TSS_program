using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maker.Serial;
using Microsoft.Maker.RemoteWiring;
using Windows.UI.Xaml;
using System.Diagnostics;
using TSSDataLogger.Data;
using System.Threading;

namespace TSSDataLogger.Connectors
{
    class IOConnector
    {
        // Default pin configuration
        public const byte STARTUP_PIN = 3;
        public const byte MANUAL_PIN = 5;
        public const byte LATCH_PIN = 6;
        public const byte SPLIT_PIN = 7;

        const int ARDUINO_ALIVE_TIMER_PERIOD = 1000;
        
        // Arduino Connection
        UsbSerial usb;
        RemoteDevice arduino;
        bool arduinoAlive = false;
        bool arduinoRebooting = false;
        Timer arduinoSignalTimer;

        // Link to mainPage and machine
        private MainPage mainPage;
        private Machine machine;

        public IOConnector(MainPage mainPage, Machine machine)
        {
            Debug.WriteLine("IOConnector");

            this.mainPage = mainPage;
            this.machine = machine;

            //  Init arduinoAlive Timer
            arduinoSignalTimer = new Timer(arduinoSignal_Tick, null, Timeout.Infinite, Timeout.Infinite);
            
            openArduinoConnection();
        }
        
        // Configure and open Arduino connection
        private void openArduinoConnection()
        {
            Debug.WriteLine("IOConnector.openArduinoConnection");

            // Configure USB connection
            usb = new UsbSerial("VID_" + Storage.GetSetting<string>("VID"), "PID_" + Storage.GetSetting<string>("PID"));
            arduino = new RemoteDevice(usb);

            // Setup callback functions
            usb.ConnectionEstablished += Usb_ConnectionEstablished;
            usb.ConnectionFailed += Usb_ConnectionFailed;
            usb.ConnectionLost += Usb_ConnectionLost;
            arduino.DeviceReady += Arduino_DeviceReady;
            arduino.DeviceConnectionFailed += Arduino_DeviceConnectionFailed;
            arduino.DeviceConnectionLost += Arduino_DeviceConnectionLost;
            
            // Begin arduino connection
            usb.begin(57600, SerialConfig.SERIAL_8N1);

            arduinoSignalTimer.Change(10*ARDUINO_ALIVE_TIMER_PERIOD, ARDUINO_ALIVE_TIMER_PERIOD);
        }

        /* Incomming data callbacks */
        private void Arduino_AnalogPinUpdated(string pin, ushort value)
        {
            //Debug.WriteLine("IOConnector.Arduino_AnalogPinUpdated");
            arduinoAlive = true;
        }

        private void Arduino_DigitalPinUpdated(byte pin, PinState pinValue)
        {
            //Debug.WriteLine("IOConnector.Arduino_DigitalPinUpdated");
            arduinoAlive = true;

            MachineStates state = MachineStates.UNDEFINED;
            switch (pin)
            {
                case STARTUP_PIN:
                    state = MachineStates.STARTUP;
                    break;
                case MANUAL_PIN:
                    state = MachineStates.MANUAL;
                    break;
                case LATCH_PIN:
                    state = MachineStates.LATCH;
                    break;
                case SPLIT_PIN:
                    state = MachineStates.SPLIT;
                    break;
            }
            
            if (pinValue == PinState.HIGH)
                machine.updateState(state, false);
            else
                machine.updateState(state, true);
        }

        /* Connection callbacks */
        private void Usb_ConnectionLost(string message)
        {
            Debug.WriteLine("IOConnector.Usb_ConnectionLost\n" + message);
            mainPage.setStatus("USB Connection Lost");
            arduinoAlive = false;
        }

        private void Usb_ConnectionFailed(string message)
        {
            Debug.WriteLine("IOConnector.Usb_ConnectionFailed\n" + message);
            mainPage.setStatus("USB Connection Failed");
            arduinoAlive = false;
        }

        private void Usb_ConnectionEstablished()
        {
            Debug.WriteLine("IOConnector.Usb_ConnectionEstablished");
            mainPage.setStatus("USB Connection Established");
            arduinoAlive = true;
        }

        private void Arduino_DeviceConnectionLost(string message)
        {
            Debug.WriteLine("IOConnector.Arduino_DeviceConnectionLost\n" + message);
            mainPage.setStatus("Microcontroller Connection Lost");
            arduinoAlive = false;
        }

        private void Arduino_DeviceConnectionFailed(string message)
        {
            Debug.WriteLine("IOConnector.Arduino_DeviceConnectionFailed\n" + message);
            mainPage.setStatus("Microcontroller Connection Failed");
            arduinoAlive = false;
        }

        private void Arduino_DeviceReady()
        {
            Debug.WriteLine("IOConnector.Arduino_DeviceReady");
            mainPage.setStatus("Microcontroller Connection Ready");

            arduinoAlive = true;

            arduino.DigitalPinUpdated += Arduino_DigitalPinUpdated;
            arduino.AnalogPinUpdated += Arduino_AnalogPinUpdated;

            arduino.pinMode(STARTUP_PIN, PinMode.INPUT);
            arduino.pinMode(MANUAL_PIN, PinMode.INPUT);
            arduino.pinMode(LATCH_PIN, PinMode.INPUT);
            arduino.pinMode(SPLIT_PIN, PinMode.INPUT);
            arduino.pinMode("A7", PinMode.ANALOG);
        }

        /* Timer callback */
        private async void arduinoSignal_Tick(object state)
        {
            //Debug.WriteLine("IOConnector.arduinoSignal_Tick");
            
            if (!arduinoAlive && !arduinoRebooting)
            {
                Debug.WriteLine("IOConnector.arduinoSignal_Tick-> Connection down. Trying to reboot connection.");
                arduinoSignalTimer.Change(Timeout.Infinite, Timeout.Infinite);

                arduinoRebooting = true;
                usb.end();
                await Task.Delay(2000);
                //usb.Dispose();
                //Debug.WriteLine("arduinoSignal_Tick-> Wait 3 sec");
                //Task.Delay(3000);
                openArduinoConnection();
                arduinoRebooting = false;
            }
            else
            {
                // Return arduinoAlive signal to default
                arduinoAlive = false;
            }
        }

        public bool isMicroControllerConnectionAlive()
        {
            return arduinoAlive;
        }
    }
}
