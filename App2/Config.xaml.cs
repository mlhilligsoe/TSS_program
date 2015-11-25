using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace App2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class Config : Page
    {
        Windows.Storage.ApplicationDataContainer settings = Windows.Storage.ApplicationData.Current.LocalSettings;

        public Config()
        {
            this.InitializeComponent();

            getSetting<string>("MachineName", textBox_MachineName);
            getSetting<string>("MachineProcessCode", textBox_MachineProcessCode);

            getSetting<int>("StartupPin", textBox_StartupPin);
            getSetting<int>("NormalPin", textBox_NormalPin);
            getSetting<int>("ManualPin", textBox_ManualPin);
            getSetting<int>("LatchPin", textBox_LatchPin);
            getSetting<int>("SplitPin", textBox_SplitPin);

            getSetting<string>("SQLServer", textBox_SQLServer);
            getSetting<string>("SQLDB", textBox_SQLDB);
            getSetting<string>("SQLUser", textBox_SQLUser);
            getSetting<string>("SQLPass", textBox_SQLPass);

            getSetting<string>("VID", textBox_VID);
            getSetting<string>("PID", textBox_PID);
            
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            updateSetting("MachineName", textBox_MachineName.Text);
            updateSetting("MachineProcessCode", textBox_MachineProcessCode.Text);

            updateSetting("SQLServer", textBox_SQLServer.Text);
            updateSetting("SQLDB", textBox_SQLDB.Text);
            updateSetting("SQLUser", textBox_SQLUser.Text);
            updateSetting("SQLPass", textBox_SQLPass.Text);

            updateSetting("VID", textBox_VID.Text);
            updateSetting("PID", textBox_PID.Text);

            Frame.Navigate(typeof(MainPage));
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        void updateSetting(string key, string value)
        {
            if (value.Length > 0)
                Storage.SetSetting(key, value);
            else
                Storage.DeleteSetting(key);
        }

        void getSetting<T>(string setting, TextBox textBox)
        {
            if (Storage.SettingExists(setting))
            {
               textBox.Text = Storage.GetSetting<T>(setting).ToString();
            }
            else
            {
                textBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
            }
                
        }
    }
}
