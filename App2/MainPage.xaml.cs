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

        // Content dialogs
        public ProcessStopContentDialog processStopContentDialog;
        public LoadOrderContentDialog loadOrderContentDialog;
        public NoDBConnectionContentDialog noDBConnectionContentDialog;
        public NoIOConnectionContentDialog noIOConnectionContentDialog;
        public bool contentDialogIsActive = false;

        string command = "";

        public MainPage()
        {
            Debug.WriteLine("MainPage");

            machine = new Machine(this);
            order = new Order();
            process = new Process();
            events = new ObservableCollection<Event>();

            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;

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
            
            // Configure and open USB/Arduino connection
            io = new IOConnector(this, machine);
            
            // Configure and open MySQL connection
            sql = new MySqlConnector(this, machine, ref order, ref process, events);
            
            logic = new Logic(this, sql, machine, order, process, events);

            processStopContentDialog = new ProcessStopContentDialog(this, logic);
            loadOrderContentDialog = new LoadOrderContentDialog(this);
            noDBConnectionContentDialog = new NoDBConnectionContentDialog(this);
            noIOConnectionContentDialog = new NoIOConnectionContentDialog(this);
            
            updateGUI();

            // Uncomment if Debug is needed from startup
            //logic.parseCommand("DEBUG");
        }

        private void CoreWindow_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs e)
        {
            Debug.WriteLine("mainPage.MainPage_KeyUp");
            setStatus("contentDialogIsActive: " + contentDialogIsActive);
            if (!contentDialogIsActive)
            {
                if (e.VirtualKey == VirtualKey.Enter)
                {
                    if (command.Length > 0)
                    {
                        logic.parseCommand(command);
                        command = "";
                    }
                }
                else if (e.VirtualKey == VirtualKey.Back)
                {
                    if (command.Length > 0)
                        command = command.Remove(command.Length - 1);
                }
                else
                    command += e.VirtualKey.ToString().Replace("Number", "");
            }

            setStatus(command);
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

        // Updates GUI
        public void updateGUI()
        {
            Debug.WriteLine("MainPage.updateGUI");

            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {

                // Update Process
                if (process.isLoaded())
                {
                    textBlockProcQuantity.Text = process.quantity.ToString();
                    textBlockProcWaste.Text = process.waste.ToString();
                    textBlockProcStart.Text = process.start.ToString("H:mm dd/MM");
                }
                else
                {
                    textBlockProcQuantity.Text = "-";
                    textBlockProcWaste.Text = "-";
                    textBlockProcStart.Text = "-";
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
                    textBlockOrderCode.Text = "SKAN ORDRE";
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
            });
        }

        public void showToggleSwitches() { Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { toggleSwitches.Visibility = Visibility.Visible; }); }
        public void hideToggleSwitches() { Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { toggleSwitches.Visibility = Visibility.Collapsed; }); }

        // Called when toggle switches are toggled
        private void toggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("mainPage.toggleSwitch_Toggled");

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
        
        public void showProcessStopContentDialog() { if (!contentDialogIsActive) { Debug.WriteLine("MainPage.showProcessStopContenDialog"); contentDialogIsActive = true; Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { processStopContentDialog.ShowAsync(); }); }; }
        public void hideProcessStopContentDialog() { Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { processStopContentDialog.Hide(); }); contentDialogIsActive = false; }

        public void showLoadOrderContentDialog() { if (!contentDialogIsActive) { Debug.WriteLine("MainPage.showLoadOrderContenDialog"); contentDialogIsActive = true; Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { loadOrderContentDialog.ShowAsync(); }); }; }
        public void hideLoadOrderContentDialog() { Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { loadOrderContentDialog.Hide(); }); contentDialogIsActive = false; }

        public void showNoIOConnectionContentDialog() { if (!contentDialogIsActive) { Debug.WriteLine("MainPage.showNoIOConnectionContenDialog"); contentDialogIsActive = true; Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { noIOConnectionContentDialog.ShowAsync(); }); }; }
        public void hideNoIOConnectionContentDialog() { Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { noIOConnectionContentDialog.Hide(); }); contentDialogIsActive = false; }

        public void showNoDBConnectionContentDialog() { if (!contentDialogIsActive) { Debug.WriteLine("MainPage.showNoDBConnectionContenDialog"); contentDialogIsActive = true; Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { noDBConnectionContentDialog.ShowAsync(); }); }; }
        public void hideNoDBConnectionContentDialog() { Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { noDBConnectionContentDialog.Hide(); }); contentDialogIsActive = false; }

    }
}

