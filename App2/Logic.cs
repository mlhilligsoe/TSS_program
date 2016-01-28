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
                        process.hasLatched = true;
                    
                    break;

                case MachineStates.SPLIT:

                    if (machine.getState(MachineStates.SPLIT) && machine.getState(MachineStates.AUTO))
                    {
                        // If SPLIT occours while machineState.AUTO is true and process is loaded, then process should be changed accordingly.
                        if (process.isLoaded())
                        {
                            if (process.hasLatched)
                            {
                                process.quantity++;
                                process.hasLatched = false;
                            }
                            else
                                process.waste++;
                            
                            process.change = DateTime.Now;

                            // Restart timer
                            Debug.WriteLine("Logic.machineStateChange->splitTimer.Change(5000, 5000)");
                            splitTimer.Change(5000, 5000);
                        }

                        // If SPLIT occours while machineState.AUTO is true and events are present, then previous event must be completed.
                        if (events.Count > 0)
                        {
                            events[events.Count - 1].change = new DateTime(DateTime.Now.Ticks, DateTimeKind.Local);
                            events[events.Count - 1].complete = true;
                        }
                    }
                    
                    break;
            }
            
            mainPage.updateGUI();
        }

        private void splitTimer_Tick(object state)
        {
            Debug.WriteLine("Logic.splitTimer_Tick");

            splitTimer.Change(Timeout.Infinite, Timeout.Infinite);
            saveAll();


            ProcessStopResult processStopResult = mainPage.processStopContenDialog();
            Debug.WriteLine("Logic.splitTimer_Tick->processStopResult: " + processStopResult);

            switch (processStopResult)
            {
                case ProcessStopResult.NewBar:
                    sql.createEvent("NewBar", "Ny Stang");
                    sql.loadEventsFromProcess(process.id, process.n_events);
                    saveAll();
                    break;
                case ProcessStopResult.Complete:
                    order.complete = true;
                    saveAll();
                    clearAll();
                    // showScanOrderContentDialog();
                    break;
                case ProcessStopResult.Pause:
                    sql.createEvent("Pause", "Process Pause");
                    sql.loadEventsFromProcess(process.id, process.n_events);
                    saveAll();
                    break;
                case ProcessStopResult.Error1:
                    sql.createEvent("Error1", "Fejltype 1");
                    sql.loadEventsFromProcess(process.id, process.n_events);
                    saveAll();
                    break;
                case ProcessStopResult.Error2:
                    sql.createEvent("Error2", "Fejltyp 2");
                    sql.loadEventsFromProcess(process.id, process.n_events);
                    saveAll();
                    break;
            }

            mainPage.updateGUI();
        }
        
        private void loadAllFromOrderCode(string orderCode)
        {
            Debug.WriteLine("Logic.loadAllFromOrderCode");

            // Try to load order from DB
            sql.loadOrder(orderCode);

            // If order is still null, create new order
            if (!order.isLoaded())
            {
                mainPage.setStatus("Order er null, opretter ny");
                order.load(-1, orderCode, machine.processCode, new DateTime(), new DateTime(), 0, false, 0);
                sql.createOrder();
            }
            // Else try to load process
            else
            {
                mainPage.setStatus("Loader process fra DB");
                sql.loadProcessFromOrder(order.id, machine.processCode);
                //textBlockStatus.Text += "\nprocess.n_events: " + process.n_events;
            }

            // If process is still null, create new process
            if (!process.isLoaded())
            {
                mainPage.setStatus("Process er null, opretter ny");
                process.load(-1, machine.processCode, new DateTime(), new DateTime(), 0, false, 0, 0);
                int processId = sql.createProcess();
                
            }
            // Else try to load events
            else
            {
                mainPage.setStatus("Loader Events fra DB");
                sql.loadEventsFromProcess(process.id, process.n_events);
            }

        }

        private void saveAll()
        {
            Debug.WriteLine("Logic.saveAll");

            // Save stuff to DB
            sql.updateOrder();
            sql.updateProcess();
            sql.updateEvents();
        }

        private void clearAll()
        {
            Debug.WriteLine("Logic.clearAll");

            // Reset Stuff
            splitTimer.Change(Timeout.Infinite, Timeout.Infinite);
            order.unload();
            process.unload();
            events.Clear();
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
                case "SAVEPROCESS":
                    Process.Save(process);
                    break;
                case "LOADPROCESS":
                    process = Process.Load();
                    mainPage.updateGUI();
                    break;
                case "CREATEPROCESS":
                    process.load(123, machine.processCode, new DateTime(), new DateTime());
                    mainPage.updateGUI();
                    break;
                case "SHOWTOGGLES":
                    mainPage.showToggleSwitches();
                    break;
                case "HIDETOGGLES":
                    mainPage.hideToggleSwitches();
                    break;
                case "LOADORDERID":
                    if (cmd.Length > 1)
                        sql.loadOrder(int.Parse(cmd[1]));
                    mainPage.updateGUI();
                    break;
                case "LOADORDERCODE":
                    if (cmd.Length > 1)
                        sql.loadOrder(cmd[1]);
                    mainPage.updateGUI();
                    break;
                case "LOADPROCESSID":
                    if (cmd.Length > 1)
                        sql.loadProcess(int.Parse(cmd[1]), "abc");
                    mainPage.updateGUI();
                    break;
                case "LOADPROCESSORDERID":
                    if (cmd.Length > 1)
                        sql.loadProcessFromOrder(int.Parse(cmd[1]), "abc");
                    mainPage.updateGUI();
                    break;
                case "INSERTPROCESS":
                    mainPage.setStatus("Inserted Process: " + sql.createProcess());
                    break;
                case "DEBUG":
                    mainPage.showToggleSwitches();
                    parseCommand("1234567890123");
                    mainPage.updateGUI();
                    break;
                case "Addevent1":
                    if (cmd.Length > 1)
                        //gør så den laver en ny textblock og skriver et event ind
                        mainPage.updateGUI();
                    break;
                case "CLEAR":
                    mainPage.setStatus("");
                    break;
                default:
                    if (cmd[0].Length == 13)
                    {
                        //int ordrenr = int.Parse(cmd[0].Remove(8).Substring(2));
                        string ordrenr = cmd[0].Remove(8).Substring(2);

                        saveAll();
                        clearAll();
                        loadAllFromOrderCode(ordrenr);
                        mainPage.updateGUI();

                    }
                    break;


            }

        }
    }
}
