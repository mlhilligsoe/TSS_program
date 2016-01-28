using System;
using Windows.Storage;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Microsoft.Maker.Serial;
using Microsoft.Maker.RemoteWiring;
using Windows.UI.Xaml.Media;
using TSSDataLogger.Data;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Diagnostics;
using TSSDataLogger.ContentDialogs;
using TSSDataLogger.Connectors;

namespace TSSDataLogger
{
    /* EventListViewItem Template Selector */
    public class EventListTemplateSelector : DataTemplateSelector
    {
        //These are public properties that will be used in the Resources section of the XAML.
        public DataTemplate CompleteEventTemplate { get; set; }
        public DataTemplate OpenEventTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var currentFrame = Window.Current.Content as Frame;
            var currentPage = currentFrame.Content as Page;

            if (item != null && currentPage != null)
            {
                Event evt = item as Event;

                if (evt.complete)
                    return CompleteEventTemplate;
                else
                    return OpenEventTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }

    public sealed partial class MainPage : Page
    {
        // Access app storage
        ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;

        // Input/Output Connector for machine signals
        IOConnector io;

        // SQL Connector for DB read/write
        MySqlConnector sql;

        // Machine, order & process states
        Machine machine;
        Order order;
        Process process;
        ObservableCollection<Event> events;

        // Machine/Process logic
        public Logic logic;
        
        public MainPage()
        {
            Debug.WriteLine("MainPage");

            // Init App
            this.InitializeComponent();
            
            // Fullscreen
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            
            // Start app once MainPage is loaded (Otherwise, navigations will yield an error)
            this.Loaded += MainPage_Loaded;

        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MainPage.MainPage_Loaded");

            // Test if application has been configured
            testAppConfiguration();

            // Init Machine
            machine = new Machine(this);
            order = new Order(this);
            process = new Process(this);
            events = new ObservableCollection<Event>();

            // Configure and open USB/Arduino connection
            io = new IOConnector(this, machine);
            
            // Configure and open MySQL connection
            sql = new MySqlConnector(this, machine, ref order, ref process, events);
            
            logic = new Logic(this, sql, machine, order, process, events);

            updateGUI();

            // Uncomment if Debug is needed from startup
            if(order != null)
                Debug.WriteLine("MainPage.MainPage_Loaded->order.id: " + order.id);
            else
                Debug.WriteLine("MainPage.MainPage_Loaded->order: null");
            logic.parseCommand("DEBUG");
            if (order != null)
                Debug.WriteLine("MainPage.MainPage_Loaded->order.id: " + order.id);
            else
                Debug.WriteLine("MainPage.MainPage_Loaded->order: null");
        }
        
        // Tests if application has been configured
        private void testAppConfiguration()
        {
            Debug.WriteLine("MainPage.testAppConfiguration");

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

        // Called on keyup from command textbox
        private void textBoxCommand_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                logic.parseCommand(textBoxCommand.Text);
                textBoxCommand.Text = "";
            }
        }

        // Updates GUI
        public void updateGUI()
        {
            Debug.WriteLine("MainPage.updateGUI");

            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {

                // Update machine state
                if (io.isMicroControllerConnectionAlive())
                {
                    if (machine.getState(MachineStates.STARTUP))
                        textBlockProcStatus.Text = "Opstart";
                    else if (machine.getState(MachineStates.AUTO))
                        textBlockProcStatus.Text = "Auto";
                    else if (machine.getState(MachineStates.MANUAL))
                        textBlockProcStatus.Text = "Manual";
                    else
                        textBlockProcStatus.Text = "-";
                }
                else
                    textBlockProcStatus.Text = "-";

                // Update Process
                if (process.isLoaded())
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
                if (order.isLoaded())
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
                CircStartup.Fill = machine.getState(MachineStates.STARTUP) ? on : off;
                CircAuto.Fill = machine.getState(MachineStates.AUTO) ? on : off;
                CircManual.Fill = machine.getState(MachineStates.MANUAL) ? on : off;
                CircLatch.Fill = machine.getState(MachineStates.LATCH) ? on : off;
                CircSplit.Fill = machine.getState(MachineStates.SPLIT) ? on : off;

                // Set focus on input command bar
                textBoxCommand.Focus(FocusState.Keyboard);
            });
        }

        public void showToggleSwitches() { Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { toggleSwitches.Visibility = Visibility.Visible; }); }
        public void hideToggleSwitches() { Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { toggleSwitches.Visibility = Visibility.Collapsed; }); }

        // Called when toggle switches are toggled
        private void toggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;

            switch (toggleSwitch.Name.ToLower())
            {
                case "startup":
                    machine.updateState(MachineStates.STARTUP, toggleSwitch.IsOn);
                    break;
                case "auto":
                    machine.updateState(MachineStates.AUTO, toggleSwitch.IsOn);
                    break;
                case "manual":
                    machine.updateState(MachineStates.MANUAL, toggleSwitch.IsOn);
                    break;
                case "latch":
                    machine.updateState(MachineStates.LATCH, toggleSwitch.IsOn);
                    break;
                case "split":
                    machine.updateState(MachineStates.SPLIT, toggleSwitch.IsOn);
                    break;

            }

        }
        
        /* Write status to tectBlockStatus */
        public void setStatus(String msg) { Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { textBlockStatus.Text = msg; }); }
        public void appendStatus(String msg) { Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { textBlockStatus.Text += msg; }); }
        //public void setStatus(String msg) { textBlockStatus.Text = msg; }
        //public void appendStatus(String msg) { textBlockStatus.Text += msg; }

        public ProcessStopResult processStopContenDialog()
        {
            ProcessStopContentDialog processStopContentDialog;

            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                processStopContentDialog = new ProcessStopContentDialog();
                await processStopContentDialog.ShowAsync();
            });

            return processStopContentDialog.Result;
        }
    }
}

