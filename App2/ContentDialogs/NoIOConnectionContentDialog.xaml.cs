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

    public sealed partial class NoIOConnectionContentDialog : ContentDialog
    {
        MainPage mainPage;

        public NoIOConnectionContentDialog(MainPage mainPage)
        {
            this.mainPage = mainPage;

            this.Opened += NoIOConnectionContentDialog_Opened;
            this.Closed += NoIOConnectionContentDialog_Closed;

            this.KeyUp += NoIOConnectionContentDialog_KeyUp;
            this.KeyDown += NoIOConnectionContentDialog_KeyDown;

            this.InitializeComponent();

        }

        private void NoIOConnectionContentDialog_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            
        }

        private void NoIOConnectionContentDialog_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            mainPage.MainPage_KeyUp(sender, e);
        }

        private void NoIOConnectionContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            //mainPage.contentDialogIsActive = false;
        }

        private void NoIOConnectionContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            //mainPage.contentDialogIsActive = true;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Hide();
            this.Focus(FocusState.Programmatic);
            mainPage.hideNoIOConnectionContentDialog();
        }
    }
}
