using System.Windows.Controls;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates adding an ArcGIS image service layer to a Map in XAML.
    /// </summary>
    /// <title>ArcGIS Image Service Layer</title>
	/// <category>Layers</category>
	/// <subcategory>Dynamic Service Layers</subcategory>
	public partial class ArcGISImageServiceLayerSample : UserControl
    {
        public ArcGISImageServiceLayerSample()
        {
            InitializeComponent();
			MyMapView.SetView(new Viewpoint(new Envelope(-13486609, 5713307, -13263258, 5823117, SpatialReferences.WebMercator)));
        }
    }
}
