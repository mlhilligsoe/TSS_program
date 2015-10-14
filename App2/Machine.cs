using Microsoft.Maker.RemoteWiring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App2
{
    public class Machine
    {
        public string name;
        public int nChannels;
        public Dictionary<int, bool> inputs;
        //private Dictionary<int, bool> old;

        public Machine(string machineName)
        {
            this.name = machineName;
            this.nChannels = 0;
            inputs = new Dictionary<int, bool>();
            //old = new Dictionary<int, bool>();
        }

        public void addInput(int inputPin) //(Arduino arduino, int pin, string name)
        {
            inputs.Add(inputPin, false);
            //old.Add(inputPin, false);

            nChannels++;

            // Set up arduino input listener

        }

        public void update(int inputPin, bool inputValue)
        {
            //old[inputPin] = inputs[inputPin];
            inputs[inputPin] = inputValue;
        }

        public void update(int inputPin, PinState inputValue)
        {
            //old[inputPin] = inputs[inputPin];
            if (inputValue == PinState.HIGH)
                inputs[inputPin] = true;
            else
                inputs[inputPin] = false;
        }

        public bool getState(int inputPin)
        {
            return inputs[inputPin];
        }

        public bool isStateChanged(int inputPin)
        {
            //return old[inputPin] != inputs[inputPin];
            return false;
        }
    }
}
