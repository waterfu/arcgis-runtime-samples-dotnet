using System.Windows.Controls;
using Esri.ArcGISRuntime.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates adding a local tiled layer from a tile package (.tpk) to a Map in XAML.
    /// </summary>
    /// <title>ArcGIS Local Tiled Layer</title>
	/// <category>Layers</category>
	/// <subcategory>Tiled Layers</subcategory>
	public partial class ArcGISLocalTiledLayerSample : UserControl
    {
        public ArcGISLocalTiledLayerSample()
        {
            InitializeComponent();
        }

		private void MapView_ExtentChanged(object sender, System.EventArgs e)
		{
			MySceneView.SetView(new Viewpoint(MyMapView.Extent));
		}
    }
}
