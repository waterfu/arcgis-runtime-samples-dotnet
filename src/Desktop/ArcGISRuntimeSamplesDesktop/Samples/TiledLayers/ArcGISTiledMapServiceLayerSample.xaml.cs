using System.Windows.Controls;
using Esri.ArcGISRuntime.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates adding an ArcGIS tiled map service to a Map in XAML.
    /// </summary>
    /// <title>ArcGIS Tiled Map Service Layer</title>
	/// <category>Layers</category>
	/// <subcategory>Tiled Layers</subcategory>
	public partial class ArcGISTiledMapServiceLayerSample : UserControl
    {
        public ArcGISTiledMapServiceLayerSample()
        {
            InitializeComponent();
        }

		private void MapView_ExtentChanged(object sender, System.EventArgs e)
		{
			var mapView = (MapView) sender;
			MySceneView.SetView(new Viewpoint(mapView.Extent));
		}
    }
}
