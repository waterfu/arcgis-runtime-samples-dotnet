using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System.Windows.Controls;
using Esri.ArcGISRuntime.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates applying raster functions to an image service layer.
    /// </summary>
    /// <title>Image Service Raster Functions</title>
	/// <category>Layers</category>
	/// <subcategory>Dynamic Service Layers</subcategory>
	public partial class ImageServiceRasterFunctions : UserControl
    {
        public ImageServiceRasterFunctions()
        {
            InitializeComponent();

			MyMapView.SetView(new Viewpoint(new Envelope(1445440,540657,1452348,544407,new SpatialReference(2264))));
			MyMapView.LayerLoaded += MyMapView_LayerLoaded;
        }

        private void RasterFunctionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
			ArcGISImageServiceLayer imageLayer = MyMapView.Scene.Layers["ImageLayer"] as ArcGISImageServiceLayer;
            var rasterFunction = (sender as ComboBox).SelectedItem as RasterFunctionInfo;
            if (rasterFunction != null)
            {
                RenderingRule renderingRule = new RenderingRule() { RasterFunctionName = rasterFunction.FunctionName };
                imageLayer.RenderingRule = renderingRule;
            }
        }

        private void MyMapView_LayerLoaded(object sender, Esri.ArcGISRuntime.Controls.LayerLoadedEventArgs e)
        {
            if (e.Layer.ID == "ImageLayer")
            {
                ArcGISImageServiceLayer imageLayer = e.Layer as ArcGISImageServiceLayer;
                if (e.LoadError == null)
                {
                    RasterFunctionsComboBox.ItemsSource = imageLayer.ServiceInfo.RasterFunctionInfos;
                    RasterFunctionsComboBox.SelectedIndex = 0;
                }
            }
        }
    }
}
