using System.Collections.Generic;
using System.Diagnostics;

namespace TSSDataLogger
{
    public enum MachineStates
    {
        UNDEFINED,
        STARTUP,
        AUTO,
        MANUAL,
        LATCH,
        SPLIT
    }

    public class Machine
    {
        private MainPage mainPage;
        
        public string name;
        public string processCode;
        public Dictionary<MachineStates, bool> states;

        public bool hasLatched;



        public Machine(MainPage mainPage) 
        {
            Debug.WriteLine("Machine");

            this.mainPage = mainPage;
            
            // Initialize machine variables
            name = Storage.GetSetting<string>("MachineName");
            processCode = Storage.GetSetting<string>("MachineProcessCode");
            states = new Dictionary<MachineStates, bool>();
            
            // Add states to machine
            addState(MachineStates.STARTUP, false);
            addState(MachineStates.AUTO, false);
            addState(MachineStates.MANUAL, false);
            addState(MachineStates.LATCH, false);
            addState(MachineStates.SPLIT, false);
        }

        public void addState(MachineStates state, bool value)
        {
            Debug.WriteLine("Machine.addState");

            states.Add(state, value);
        }

        public void updateState(MachineStates state, bool value)
        {
            Debug.WriteLine("Machine.updateState");

            if(state != MachineStates.UNDEFINED)
            {
                states[state] = value;
                mainPage.updateGUI();
                mainPage.logic.machineStateChange(state);
            }
                
        }

        public bool getState(MachineStates state)
        {
            //Debug.WriteLine("Machine.getState");

            return states[state];
        }
    }
}
