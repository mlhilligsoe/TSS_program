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
    public sealed partial class NoDBConnectionContentDialog : ContentDialog
    {
        MainPage mainPage;

        public NoDBConnectionContentDialog(MainPage mainPage)
        {
            this.mainPage = mainPage;

            this.Opened += NoDBConnectionContentDialog_Opened;
            this.Closed += NoDBConnectionContentDialog_Closed;

            this.KeyUp += NoDBConnectionContentDialog_KeyUp;

            this.InitializeComponent();
        }

        private void NoDBConnectionContentDialog_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            mainPage.MainPage_KeyUp(sender, e);
        }

        private void NoDBConnectionContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            //mainPage.contentDialogIsActive = false;
        }

        private void NoDBConnectionContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            //mainPage.contentDialogIsActive = true;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Hide();
            mainPage.hideNoDBConnectionContentDialog();
        }
    }
}
