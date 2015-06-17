using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample shows how to add the ArcGIS Traffic service to a map.
    /// </summary>
    /// <title>Traffic 3D</title>
    /// <category>ArcGIS Online Services</category>
	public partial class Traffic3D : UserControl
    {
        private ArcGISDynamicMapServiceLayer _trafficLayer;

        public Traffic3D()
        {
            InitializeComponent();
            IdentityManager.Current.OAuthAuthorizeHandler = new OAuthAuthorizeHandler();
            IdentityManager.Current.ChallengeHandler = new ChallengeHandler(PortalSecurity.Challenge);

            _trafficLayer = MySceneView.Scene.Layers["Traffic"] as ArcGISDynamicMapServiceLayer;

			MySceneView.LayerLoaded += MySceneView_LayerLoaded;
        }

        // Populate layer legend with north america traffic sublayer names
		private async void MySceneView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.Layer == _trafficLayer)
            {
                var legendLayer = _trafficLayer as ILegendSupport;
                var layerLegendInfo = await legendLayer.GetLegendInfosAsync();
                legendTree.ItemsSource = layerLegendInfo.LayerLegendInfos.First().LayerLegendInfos;
            }
        }

        private async void MySceneView_SceneViewTapped(object sender, MapViewInputEventArgs e)
        {
            try
            {
                incidentOverlay.Visibility = Visibility.Collapsed;
                incidentOverlay.DataContext = null;

                var identifyTask = new IdentifyTask(new Uri(_trafficLayer.ServiceUri));

                IdentifyParameters identifyParams = new IdentifyParameters(
					e.Location, 
					(MySceneView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope),
					 5,(int)MySceneView.ActualHeight, (int)MySceneView.ActualWidth)
                {
                    LayerIDs = new int[] { 2, 3, 4 },
                    LayerOption = LayerOption.Top,
                    SpatialReference = MySceneView.SpatialReference,
                };

                var result = await identifyTask.ExecuteAsync(identifyParams);

                if (result != null && result.Results != null && result.Results.Count > 0)
                {
                    incidentOverlay.DataContext = result.Results.First();
                    incidentOverlay.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Identify Error");
            }
        }
    }
}
