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
    public sealed partial class ProcessStopContentDialog : ContentDialog
    {
        MainPage mainPage;
        Logic logic;
        
        public ProcessStopContentDialog(MainPage mainPage, Logic logic)
        {
            this.mainPage = mainPage;
            this.logic = logic;

            this.Opened += ProcessStopContentDialog_Opened;
            this.Closed += ProcessStopContentDialog_Closed;

            this.KeyUp += ProcessStopContentDialog_KeyUp;

            this.InitializeComponent();
        }

        private void ProcessStopContentDialog_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            mainPage.MainPage_KeyUp(sender, e);
        }

        private void ProcessStopContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            //mainPage.contentDialogIsActive = false;
        }

        private void ProcessStopContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            //mainPage.contentDialogIsActive = true;
        }

        private void completeButton_Click(object sender, RoutedEventArgs e)
        {
            mainPage.hideProcessStopContentDialog();
            logic.completeOrder();
        }

        private void error1Button_Click(object sender, RoutedEventArgs e)
        {
            mainPage.hideProcessStopContentDialog();
            logic.error1Event();
        }

        private void error2Button_Click(object sender, RoutedEventArgs e)
        {
            mainPage.hideProcessStopContentDialog();
            logic.error2Event();
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            mainPage.hideProcessStopContentDialog();
            logic.pauseEvent();
        }
    }
}
