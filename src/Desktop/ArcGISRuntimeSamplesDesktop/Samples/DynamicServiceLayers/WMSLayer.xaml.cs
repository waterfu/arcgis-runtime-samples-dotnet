using Esri.ArcGISRuntime.Controls;
using System.Diagnostics;
using System.Windows.Controls;
using Esri.ArcGISRuntime.Geometry;

namespace ArcGISRuntime.Samples.Desktop.DynamicLayers
{
    /// <summary>
    /// This sample demonstrates displaying an WMSLayer draped over an ArcGISTiledMapServiceLayer. The WMSLayer allows users to display Open GIS Consortium (OGC) WMS layers.
    /// </summary>
    /// <title>WMS Layer</title>
	/// <category>Layers</category>
	/// <subcategory>Dynamic Service Layers</subcategory>
	public partial class WMSLayer : UserControl
    {
        public WMSLayer()
        {
            InitializeComponent();
	        MyMapView.SetView(
		        new Viewpoint(new Envelope(-15000000, 2000000, -7000000, 8000000, SpatialReferences.WebMercator)));
        }

		private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
		{
			if (e.LoadError == null)
				return;

			Debug.WriteLine(string.Format("Error while loading layer : {0} - {1}", e.Layer.ID, e.LoadError.Message));
		}
    }
}
