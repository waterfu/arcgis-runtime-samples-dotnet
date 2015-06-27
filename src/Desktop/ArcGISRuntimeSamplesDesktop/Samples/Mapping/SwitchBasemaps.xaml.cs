using Esri.ArcGISRuntime.Layers;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates changing the basemap layer in a map by switching  between ArcGIS tiled map services layers hosted by ArcGIS Online.
    /// </summary>
    /// <title>Switch Basemaps</title>
	/// <category>Mapping</category>
	public partial class SwitchBasemaps : UserControl
    {
        public SwitchBasemaps()
        {
            InitializeComponent();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
			var serviceUri = ((RadioButton)sender).Tag as string;
			UpdateFirstLayer(MyMapView.Map.Layers, serviceUri);
			UpdateFirstLayer(MySceneView.Scene.Layers, serviceUri);
        }

	    private void UpdateFirstLayer(LayerCollection layers, string serviceUri)
	    {
			layers.RemoveAt(0);

		    layers.Add(new ArcGISTiledMapServiceLayer()
			{
				ServiceUri = serviceUri
			});
	    }
    }
}
