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
    public sealed partial class LoadOrderContentDialog : ContentDialog
    {
        MainPage mainPage;

        public LoadOrderContentDialog(MainPage mainPage)
        {
            this.mainPage = mainPage;

            this.Opened += LoadOrderContentDialog_Opened;
            this.Closed += LoadOrderContentDialog_Closed;

            this.KeyUp += LoadOrderContentDialog_KeyUp;
            this.KeyDown += LoadOrderContentDialog_KeyDown;

            this.InitializeComponent();
        }

        private void LoadOrderContentDialog_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            
        }

        private void LoadOrderContentDialog_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            mainPage.MainPage_KeyUp(sender, e);
        }

        private void LoadOrderContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            //mainPage.contentDialogIsActive = false;
        }

        private void LoadOrderContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            //mainPage.contentDialogIsActive = true;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Hide();
            mainPage.hideLoadOrderContentDialog();
        }
    }
}
