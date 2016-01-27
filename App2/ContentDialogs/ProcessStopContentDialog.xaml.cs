using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TSSDataLogger.ContentDialogs
{
    public enum ProcessStopResult
    {
        Complete,
        Pause,
        Error1,
        Error2,
        NewBar
    }
    
    public sealed partial class ProcessStopContentDialog : ContentDialog
    {
        public ProcessStopResult Result { get; private set; }

        public ProcessStopContentDialog()
        {
            this.InitializeComponent();

            // Set default response, in case no button is pressed
            this.Result = ProcessStopResult.NewBar;
        }

        private void completeButton_Click(object sender, RoutedEventArgs e)
        {
            this.Result = ProcessStopResult.Complete;
            this.Hide();
        }

        private void error1Button_Click(object sender, RoutedEventArgs e)
        {
            this.Result = ProcessStopResult.Error1;
            this.Hide();
        }

        private void error2Button_Click(object sender, RoutedEventArgs e)
        {
            this.Result = ProcessStopResult.Error2;
            this.Hide();
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Result = ProcessStopResult.Pause;
            this.Hide();
        }
    }
}
