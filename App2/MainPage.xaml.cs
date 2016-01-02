﻿using System;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Maker.Serial;
using Microsoft.Maker.RemoteWiring;
using Windows.UI.Xaml.Media;
using App2.Data;
using System.Collections.ObjectModel;

namespace App2
{
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;

        // Default pin configuration
        const byte STARTUP = 3;
        const byte NORMAL = 4;
        const byte MANUAL = 5;
        const byte LATCH = 6;
        const byte SPLIT = 7;

        // Access app storage
        ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;
        
        // Arduino Connection
        UsbSerial usb;
        RemoteDevice arduino;

        // SQL Connection
        MySqlConnector sql;

        // Machine, order & process states
        Machine machine;
        Order order;
        Process process;
        ObservableCollection<Event> events = new ObservableCollection<Event>();

        // SplitTimer to handle unexpected events
        DispatcherTimer splitTimer;

        public MainPage()
        {
            // Init App
            this.InitializeComponent();

            // Create MainPage handle
            Current = this;

            // If events are saved, load events
            EventListView.ItemsSource = events;
            //events.Add(new Data.Event() { listId = 0, start = DateTime.Now, code = "evt12", description = "Process 12 started." });

            // Fullscreen
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            
            // Start app once MainPage is loaded (Otherwise, navigations will yield an error)
            this.Loaded += MainPage_Loaded;

        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // Test if application has been configured
            testAppConfiguration();

            // Configure, Open and Test USB/Arduino connection
            openArduinoConnection();

            // Configure, Open and Test MySQL connection
            sql = new MySqlConnector();
            sql.testConnection(textBlockStatus);

            // Init Machine
            initMachine();

            // If order is saved, load order
            if (Storage.SettingExists("order"))
                order = Order.load();

            // If process is saved, load process
            if (Storage.SettingExists("process"))
                process = Process.Load();



            // Update user interface
            parseCommand("LOADORDERID 123");
            parseCommand("CREATEPROCESS");
            parseCommand("SHOWTOGGLES");
            updateGUI();

            //  DispatcherTimer setup
            splitTimer = new DispatcherTimer();
            splitTimer.Tick += splitTimer_Tick;



            
        }

        // Configures, Opens and Tests arduino connection
        private void openArduinoConnection()
        {
            // configure Arduino connection
            usb = new UsbSerial("VID_" + (string)settings.Values["VID"],
                                "PID_" + (string)settings.Values["PID"]);
            arduino = new RemoteDevice(usb);

            // Setup callback functions
            usb.ConnectionEstablished += OnConnectionEstablished;
            usb.ConnectionFailed += Usb_ConnectionFailed;
            usb.ConnectionLost += Usb_ConnectionLost;
            arduino.DeviceReady += Arduino_DeviceReady;
            arduino.DeviceConnectionFailed += Arduino_DeviceConnectionFailed;
            arduino.DeviceConnectionLost += Arduino_DeviceConnectionLost;

            // Begin arduino connection
            usb.begin(115200, SerialConfig.SERIAL_8N1);
        }

        // Configures machine parameters
        private void initMachine()
        {
            machine = new Machine( (string) settings.Values["MachineName"], (string)settings.Values["MachineProcessCode"]);
            machine.addInput(STARTUP);
            machine.addInput(NORMAL);
            machine.addInput(MANUAL);
            machine.addInput(LATCH);
            machine.addInput(SPLIT);

        }

        // Tests if application has been configured
        private void testAppConfiguration()
        {
            // Save pin configuration to storage
            Storage.SetSetting<byte>("StartupPin", STARTUP);
            Storage.SetSetting<byte>("NormalPin", NORMAL);
            Storage.SetSetting<byte>("ManualPin", MANUAL);
            Storage.SetSetting<byte>("LatchPin", LATCH);
            Storage.SetSetting<byte>("SplitPin", SPLIT);
            
            // Test Machine parameters
            if (!Storage.SettingExists("MachineName") 
                || !Storage.SettingExists("MachineProcessCode"))
            {
                Frame.Navigate(typeof(Config));
            }

            // Test USB & Arduino Parameters
            if (!Storage.SettingExists("VID")
                || !Storage.SettingExists("PID"))
            {
                Frame.Navigate(typeof(Config));
            }

            // Test SQL parameters
            if (!Storage.SettingExists("SQLServer")
                || !Storage.SettingExists("SQLDB")
                || !Storage.SettingExists("SQLUser")
                || !Storage.SettingExists("SQLUser")
                )
            {
                Frame.Navigate(typeof(Config));
            }
            
        }
        
        // Pasrse Commands received from keyboard/Barcode scanner
        public void parseCommand(string input)
        {
            String[] cmd = input.Split(' ');

            switch (cmd[0])
            {
                case "CONFIG":
                    Frame.Navigate(typeof(Config));
                    break;
                case "SAVEPROCESS":
                    Process.Save(process);
                    break;
                case "LOADPROCESS":
                    process = Process.Load();
                    updateGUI();
                    break;
                case "CREATEPROCESS":
                    process = new Process(123, "Udstykning");
                    updateGUI();
                    break;
                case "SHOWTOGGLES":
                    toggleSwitches.Visibility = Visibility.Visible;
                    updateGUI();
                    break;
                case "HIDETOGGLES":
                    toggleSwitches.Visibility = Visibility.Collapsed;
                    updateGUI();

                    break;
                case "LOADORDERID":
                    if (cmd.Length > 1)
                        sql.loadOrder(int.Parse(cmd[1]), ref order, textBlockStatus);
                    updateGUI();
                    break;
                case "LOADORDERCODE":
                    if (cmd.Length > 1)
                        sql.loadOrder(cmd[1], ref order, textBlockStatus);
                    updateGUI();
                    break;
                case "LOADPROCESSID":
                    if (cmd.Length > 1)
                        sql.loadProcess(int.Parse(cmd[1]), "abc", ref process, textBlockStatus);
                    updateGUI();
                    break;


                case "LOADPROCESSORDERID":
                    if (cmd.Length > 1)
                        sql.loadProcessFromOrder(int.Parse(cmd[1]), "abc", ref process, textBlockStatus);
                    updateGUI();
                    break;
                case "INSERTPROCESS":
                    textBlockStatus.Text = "Inserted Process: " + sql.createProcess("Udstykning", order, textBlockStatus).ToString();
                    break;
                case "DEBUG":
                    textBlockStatus.Height = 300;
                    break;
                case "Addevent1":
                    if (cmd.Length > 1)
                     //gør så den laver en ny textblock og skriver et event ind
                    updateGUI();
                    break;

                default:
                    if(cmd[0].Length == 13)
                    {
                        //int ordrenr = int.Parse(cmd[0].Remove(8).Substring(2));
                        string ordrenr = cmd[0].Remove(8).Substring(2);

                        saveAll();
                        clearAll();
                        loadAllFromOrderCode(ordrenr);
                        updateGUI();

                    }
                    break;


                }
         
        }
        
        // Called on keyup from command textbox
        private void textBox_Command_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                parseCommand(textBoxCommand.Text);

                textBoxCommand.Text = "";

            }
        }

        // Updates GUI
        private void updateGUI()
        {
            // Update machine
            if (machine.getState(STARTUP))
                textBlockProcStatus.Text = "Startup";
            else if (machine.getState(NORMAL))
                textBlockProcStatus.Text = "Normal";
            else if (machine.getState(MANUAL))
                textBlockProcStatus.Text = "Manual";
            else
                textBlockProcStatus.Text = "Off";

            // Update Process
            if (process != null)
            {
                textBlockProcQuantity.Text = process.quantity.ToString();
                textBlockProcWaste.Text = process.waste.ToString();
                textBlockProcStart.Text = process.start.ToString("H:mm dd/MM");
                textBlockProcComplete.Text = process.change.ToString("H:mm dd/MM");
            }
            else
            {
                textBlockProcQuantity.Text = "-";
                textBlockProcWaste.Text = "-";
                textBlockProcStart.Text = "-";
                textBlockProcComplete.Text = "-";
            }


            // Update Order
            if (order != null)
            {
                textBlockOrderCode.Text = order.code;
                textBlockOrderProduct.Text = order.product;
                textBlockOrderQuantity.Text = order.quantity.ToString();
                textBlockOrderStart.Text = order.start.ToString("H:mm dd/MM");
            }
            else
            {
                textBlockOrderCode.Text = "-";
                textBlockOrderProduct.Text = "-";
                textBlockOrderQuantity.Text = "-";
                textBlockOrderStart.Text = "-";
            }

            // Update input lights
            SolidColorBrush on = new SolidColorBrush(Windows.UI.Colors.Lime);
            SolidColorBrush off = new SolidColorBrush(Windows.UI.Colors.LightGray);
            CircStartup.Fill = machine.getState(STARTUP) ? on : off;
            CircNormal.Fill = machine.getState(NORMAL) ? on : off;
            CircManual.Fill = machine.getState(MANUAL) ? on : off;
            CircLatch.Fill = machine.getState(LATCH) ? on : off;
            CircSplit.Fill = machine.getState(SPLIT) ? on : off;

            // Set focus on input command bar
            textBoxCommand.Focus(FocusState.Keyboard);
        }

        // Called when toggle switches are toggled
        private void toggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;

            switch (toggleSwitch.Name.ToLower())
            {
                case "startup":
                    Arduino_DigitalPinUpdated(STARTUP, toggleSwitch.IsOn ? PinState.HIGH : PinState.LOW);
                    break;
                case "normal":
                    Arduino_DigitalPinUpdated(NORMAL, toggleSwitch.IsOn ? PinState.HIGH : PinState.LOW);
                    break;
                case "manual":
                    Arduino_DigitalPinUpdated(MANUAL, toggleSwitch.IsOn ? PinState.HIGH : PinState.LOW);
                    break;
                case "latch":
                    Arduino_DigitalPinUpdated(LATCH, toggleSwitch.IsOn ? PinState.HIGH : PinState.LOW);
                    break;
                case "split":
                    Arduino_DigitalPinUpdated(SPLIT, toggleSwitch.IsOn ? PinState.HIGH : PinState.LOW);
                    break;

            }

        }

        // Called when digital input of arduino is changes
        private async void Arduino_DigitalPinUpdated(byte pin, PinState pinValue)
                {

                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        machine.update(pin, pinValue);

                        switch(pin)
                        {
                            case STARTUP:

                                break;
                            case NORMAL:
                                // Init timer
                                //splitTimer = new DispatcherTimer();

                                break;
                            case MANUAL:
                        
                                break;
                            case LATCH:

                                break;
                            case SPLIT:
                                
                                if(machine.getState(NORMAL) == true && machine.getState(SPLIT) == true)
                                {
                                    // Count up in produced or waste
                                    if (machine.getState(LATCH) == true)
                                    {
                                        process.quantity++;
                                    }
                                    else
                                    {
                                        process.waste++;
                                    }

                                    process.change = DateTime.Now;

                                    // Restart timer
                                    splitTimer.Interval = new TimeSpan(0, 0, 5);
                                    splitTimer.Start();
                                }

                                

                                break;
                        }

                        updateGUI();
                
                    });

                }

        private void Usb_ConnectionLost(string message)
        {
            var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
            {
                textBlockStatus.Text = "USB Connection Lost";
            }));
        }

        private void Usb_ConnectionFailed(string message)
        {
            var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
            {
                textBlockStatus.Text = "USB Connection Failed";
            }));
        }

        private void OnConnectionEstablished()
        {
            var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
            {
                textBlockStatus.Text = "Microcontroller Connection Established";
            }));
        }

        private void Arduino_DeviceConnectionLost(string message)
        {
            var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
            {
                textBlockStatus.Text = "Microcontroller Connection Lost";
            }));
        }

        private void Arduino_DeviceConnectionFailed(string message)
        {
            var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
            {
                textBlockStatus.Text = "Microcontroller Connection Failed";
            }));
        }

        private void Arduino_DeviceReady()
        {
            arduino.pinMode(STARTUP, PinMode.INPUT);
            arduino.pinMode(NORMAL, PinMode.INPUT);
            arduino.pinMode(MANUAL, PinMode.INPUT);
            arduino.pinMode(LATCH, PinMode.INPUT);
            arduino.pinMode(SPLIT, PinMode.INPUT);

            arduino.DigitalPinUpdated += Arduino_DigitalPinUpdated;

            var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
            {
                textBlockStatus.Text = "Microcontroller Connection Ready";
            }));
        }

        // SplitTimer method
        private void splitTimer_Tick(object sender, object e)
        {
            splitTimer.Stop();
            eventPanel.Visibility = Visibility.Visible;
        }

        private void button_error_Click(object sender, RoutedEventArgs e)
        {
            eventPanel.Visibility = Visibility.Collapsed;
        }

        private void button_done_Click(object sender, RoutedEventArgs e)
        {
            order.complete = true;
            eventPanel.Visibility = Visibility.Collapsed;
            updateGUI();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            loadAllFromOrderCode("TestOrder");

        }

        private void loadAllFromOrderCode(string orderCode)
        {
            // Try to load order from DB
            sql.loadOrder(orderCode, ref order, textBlockStatus);

            // If order is still null, create new order
            if (order == null)
            {
                textBlockStatus.Text += "Order er null, opretter ny";
                order = new Order(-1, orderCode, "", 0);
                textBlockStatus.Text += order.start.ToString();
                order.id = sql.createOrder(order, textBlockStatus);
            }
            // Else try to load process
            else
            {
                textBlockStatus.Text += "Loader process fra DB";
                sql.loadProcessFromOrder(order.id, machine.processCode, ref process, textBlockStatus);
                //textBlockStatus.Text += "\nprocess.n_events: " + process.n_events;
            }

            // If process is still null, create new process
            if (process == null) {
                textBlockStatus.Text += "Process er null, opretter ny";
                int processId = sql.createProcess(machine.processCode, order, textBlockStatus);
                process = new Process(processId, machine.processCode);
            }
            // Else try to load events
            else
            {
                textBlockStatus.Text += "Loader Events fra DBz";
                sql.loadEventsFromProcess(process.id, process.n_events, ref events, textBlockStatus);
            }

        }

        private void saveAll()
        {
            // Save stuff to DB
            sql.updateOrder(order, textBlockStatus);
            sql.updateProcess(process, textBlockStatus);
            sql.updateEvents(events, textBlockStatus);
        }

        private void clearAll()
        {
            // Reset Stuff
            order = null;
            process = null;
            events.Clear();
        }
    }


}

