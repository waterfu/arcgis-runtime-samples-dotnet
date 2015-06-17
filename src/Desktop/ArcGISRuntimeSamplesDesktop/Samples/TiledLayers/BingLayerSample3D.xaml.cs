using Esri.ArcGISRuntime.Layers;
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates adding a Bing Maps layer to a Scene in code.  A Bing Maps key must be provided to view Bing Maps tiled map services.
    /// </summary>
    /// <title>Bing Maps Layer 3D</title>
	/// <category>Layers</category>
	/// <subcategory>Tiled Layers</subcategory>
	public partial class BingLayerSample3D : UserControl
    {
		public BingLayerSample3D()
        {
            InitializeComponent();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            string layerNameTag = (string)((RadioButton)sender).Tag;

			foreach (Layer layer in myScene.Layers)
                if (layer is BingLayer)
                    layer.IsVisible = false;

			myScene.Layers[layerNameTag].IsVisible = true;
        }

        private void BingKeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text.Length >= 64)
                LoadMapButton.IsEnabled = true;
            else
                LoadMapButton.IsEnabled = false;
        }

        private void LoadMapButton_Click(object sender, RoutedEventArgs e)
        {
            WebClient webClient = new WebClient();
            string uri = string.Format("http://dev.virtualearth.net/REST/v1/Imagery/Metadata/Aerial?supressStatus=true&key={0}", BingKeyTextBox.Text);

            webClient.OpenReadCompleted += (s, a) =>
            {
                if (a.Error == null)
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(BingAuthentication));
                    BingAuthentication bingAuthentication = serializer.ReadObject(a.Result) as BingAuthentication;
                    a.Result.Close();
                    string authenticationResult = bingAuthentication.AuthenticationResultCode.ToString();

                    if (authenticationResult == "ValidCredentials")
                    {
                        foreach (BingLayer.LayerType layerType in (BingLayer.LayerType[])Enum.GetValues(typeof(BingLayer.LayerType)))
                        {
                            BingLayer bingLayer = new BingLayer()
                            {
                                ID = layerType.ToString(),
                                MapStyle = layerType,
                                Key = BingKeyTextBox.Text,
                                IsVisible = false
                            };

							myScene.Layers.Add(bingLayer);
                        }

						myScene.Layers[0].IsVisible = true;

                        BingKeyGrid.Visibility = System.Windows.Visibility.Collapsed;
                        LayerStyleGrid.Visibility = System.Windows.Visibility.Visible;

                        InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Collapsed;

                    }
                    else InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible;
                }
                else InvalidBingKeyTextBlock.Visibility = System.Windows.Visibility.Visible;
            };

            webClient.OpenReadAsync(new System.Uri(uri));
        }

        [DataContract]
        public class BingAuthentication
        {
            [DataMember(Name = "authenticationResultCode")]
            public string AuthenticationResultCode { get; set; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("IExplore.exe", "https://www.bingmapsportal.com");
        }
    }
}
