using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("station"))
            {
                textBox_station.Text = (String)(ApplicationData.Current.LocalSettings.Values["station"]);
            }


        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {

            ApplicationData.Current.LocalSettings.Values["station"] = textBox_station.Text;


            HttpBaseProtocolFilter filter;
            HttpClient httpClient;
            String resourceUri;
            Uri uri;

            resourceUri = "http://192.168.1.100/getStations.php?id=12QW34as";

            filter = new HttpBaseProtocolFilter();
            httpClient = new HttpClient(filter);
            Uri.TryCreate("http://192.168.1.100/getStations.php?id=12QW34as", UriKind.Absolute, out uri);

            textBlock_response.Text = "Connecting to server";

            HttpResponseMessage response;
            try {
                response = await httpClient.GetAsync(uri);
                String responseBodyAsText = await response.Content.ReadAsStringAsync();

                textBlock_response.Text = responseBodyAsText;

                XElement element = XElement.Parse(await response.Content.ReadAsStringAsync());
                OutputList.ItemsSource = (from c in element.Elements("item") select c.Attribute("station_name").Value);


            }
            catch (Exception ex)
            {
                textBlock_response.Text = "Error: " + ex.Message + "\n" + ex.GetType().ToString();
            }
            
            
        }
    }
}
