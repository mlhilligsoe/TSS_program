using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TSSDataLogger.ContentDialogs;
using TSSDataLogger.Data;
using Windows.UI.Xaml;

namespace TSSDataLogger
{
    public class Logic
    {
        MainPage mainPage;
        MySqlConnector sql;
        Machine machine;
        Order order;
        Process process;
        ObservableCollection<Event> events;

        Timer splitTimer;

        public Logic(MainPage mainPage, MySqlConnector sql, Machine machine, Order order, Process process, ObservableCollection<Event> events)
        {
            Debug.WriteLine("Logic");

            this.mainPage = mainPage;
            this.sql = sql;
            this.machine = machine;
            this.order = order;
            this.process = process;
            this.events = events;


            //  DispatcherTimer setup
            splitTimer = new Timer(splitTimer_Tick, null, Timeout.Infinite, Timeout.Infinite);
        }
        
        public void machineStateChange(MachineStates state)
        {
            Debug.WriteLine("Logic.machineStateChange");

            switch (state)
            {
                case MachineStates.STARTUP:

                    // If neither STARTUP or MANUAL states are active, machine must be running in AUTO
                    if (!machine.getState(MachineStates.STARTUP) && !machine.getState(MachineStates.MANUAL))
                        machine.updateState(MachineStates.AUTO, true);
                    else
                        machine.updateState(MachineStates.AUTO, false);

                    break;

                case MachineStates.AUTO:

                    break;

                case MachineStates.MANUAL:
                    
                    // If neither STARTUP or MANUAL states are active, machine must be running in AUTO
                    if (!machine.getState(MachineStates.STARTUP) && !machine.getState(MachineStates.MANUAL))
                        machine.updateState(MachineStates.AUTO, true);
                    else
                        machine.updateState(MachineStates.AUTO, false);
                    break;

                case MachineStates.LATCH:

                    // If LATCH occours while machineState.AUTO is true and a Process is loaded, then hasLatched = true
                    if (machine.getState(MachineStates.LATCH) && machine.getState(MachineStates.AUTO) && process.isLoaded())
                        machine.hasLatched = true;
                    
                    break;

                case MachineStates.SPLIT:

                    if (machine.getState(MachineStates.SPLIT) && machine.getState(MachineStates.AUTO))
                    {
                        // If SPLIT occours while machineState.AUTO is true and process is loaded, then process should be changed accordingly.
                        if (process.isLoaded())
                        {
                            if (machine.hasLatched)
                            {
                                process.quantity++;
                                machine.hasLatched = false;
                            }
                            else
                                process.waste++;
                            
                            process.change = DateTime.Now;
                            order.change = DateTime.Now;

                            // Restart timer
                            Debug.WriteLine("Logic.machineStateChange->splitTimer.Change(5000, 5000)");
                            mainPage.hideProcessStopContentDialog();
                            splitTimer.Change(5000, 5000);
                        }

                        // If SPLIT occours while machineState.AUTO is true and events are present, then previous event must be completed.
                        if (events.Count > 0)
                        {
                            events[events.Count - 1].change = DateTime.Now;
                            events[events.Count - 1].complete = true;
                        }
                    }
                    
                    break;
            }
            
            mainPage.updateGUI();
        }

        public void newBarEvent()
        {
            events.Add(new Event("NyStang", DateTime.Now, DateTime.Now, true));
            sql.createEvent(events[events.Count-1]);
        }

        public void completeOrder()
        {
            order.complete = true;
            saveAll();
            clearAll();
            mainPage.updateGUI();
        }

        public void pauseEvent()
        {
            events.Add(new Event("Pause", DateTime.Now, DateTime.Now, false));
            sql.createEvent(events[events.Count - 1]);
        }

        public void error1Event()
        {
            events.Add(new Event("FejlType1", DateTime.Now, DateTime.Now, false));
            sql.createEvent(events[events.Count - 1]);
        }

        public void error2Event()
        {
            events.Add(new Event("FejlTyp2", DateTime.Now, DateTime.Now, false));
            sql.createEvent(events[events.Count - 1]);
        }



        private async void splitTimer_Tick(object state)
        {
            Debug.WriteLine("Logic.splitTimer_Tick");

            splitTimer.Change(Timeout.Infinite, Timeout.Infinite);
            saveAll();
            
            mainPage.showProcessStopContentDialog();
            
            mainPage.updateGUI();
        }

        private void loadAllFromOrderCode(string orderCode)
        {
            Debug.WriteLine("Logic.loadAllFromOrderCode");

            //mainPage.hideLoadOrderContenDialog();

            // Try to load order from DB
            Debug.WriteLine("Loader ordre fra DB");
            sql.loadOrder(orderCode);

            // If order is still null, create new order
            if (!order.isLoaded())
            {
                Debug.WriteLine("Ordre er null, opretter ny");
                mainPage.setStatus("Ordre er null, opretter ny");
                order.load(-1, orderCode, new DateTime(), new DateTime(), false);
                sql.createOrder();
            }
            // Else try to load process
            else
            {
                Debug.WriteLine("Loader process fra DB");
                mainPage.setStatus("Loader process fra DB");
                sql.loadProcessFromOrder(order.id, machine.processCode);
            }

            // If process is still null, create new process
            if (!process.isLoaded())
            {
                Debug.WriteLine("Process er null, opretter ny");
                mainPage.setStatus("Process er null, opretter ny");
                process.load(-1, machine.processCode, DateTime.Now, DateTime.Now, false);
                sql.createProcess();
            }
            // Else try to load events
            else
            {
                Debug.WriteLine("Loader events fra DB");
                mainPage.setStatus("Loader events fra DB");
                sql.loadEventsFromProcess(process.id);
            }
        }

        private void saveAll()
        {
            Debug.WriteLine("Logic.saveAll");

            // Save stuff to DB
            sql.updateOrder();
            sql.updateProcess();
            sql.updateEvents();
            mainPage.setStatus("ORDRE GEMT I DATABASE");
        }

        private void clearAll()
        {
            Debug.WriteLine("Logic.clearAll");

            // Reset Stuff
            splitTimer.Change(Timeout.Infinite, Timeout.Infinite);
            order.unload();
            process.unload();
            events.Clear();

            mainPage.setStatus("INGEN ORDRE LOADET - Skan ny ordre");
        }

        // Pasrse Commands received from keyboard/Barcode scanner
        public void parseCommand(string input)
        {
            Debug.WriteLine("Logic.parseCommand");
            
            String[] cmd = input.Split(' ');

            switch (cmd[0])
            {
                case "CONFIG":
                    mainPage.Frame.Navigate(typeof(Config));
                    break;
                case "SHOWTOGGLES":
                    mainPage.showToggleSwitches();
                    break;
                case "HIDETOGGLES":
                    mainPage.hideToggleSwitches();
                    break;
                case "DEBUG":
                    mainPage.showToggleSwitches();
                    parseCommand("1234567890123");
                    mainPage.updateGUI();
                    break;
                case "CLEAR":
                    mainPage.setStatus("");
                    break;
                default:
                    if (cmd[0].Length == 13)
                    {
                        string ordrenr = cmd[0];

                        saveAll();
                        clearAll();
                        loadAllFromOrderCode(ordrenr);
                        if (order.isLoaded())
                            mainPage.setStatus("ORDRE LOADET FRA DATABASE");
                        mainPage.updateGUI();

                    }
                    break;


            }

        }
    }
}
