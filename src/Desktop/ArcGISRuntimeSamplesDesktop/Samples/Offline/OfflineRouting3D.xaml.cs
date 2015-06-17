using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalyst;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates offline routing with a network analyst LocalRouteTask.
    /// </summary>
    /// <title>Routing 3D</title>
    /// <category>Offline</category>
	public partial class OfflineRouting3D : UserControl
    {
        private const string _localRoutingDatabase = @"..\..\..\..\samples-data\networks\san-diego\san-diego-network.geodatabase";
        private const string _networkName = "Streets_ND";

        private LocalRouteTask _routeTask;
        private Symbol _directionPointSymbol;
		private GraphicsOverlay _stopsOverlay;
		private GraphicsOverlay _routesOverlay;
		private GraphicsOverlay _directionsOverlay;
        private bool _isMapReady;

        public OfflineRouting3D()
        {
            InitializeComponent();

            _isMapReady = false;
            _directionPointSymbol = layoutGrid.Resources["directionPointSymbol"] as Symbol;
			_stopsOverlay = MySceneView.GraphicsOverlays["stopsOverlay"];
			_routesOverlay = MySceneView.GraphicsOverlays["routesOverlay"];
			_directionsOverlay = MySceneView.GraphicsOverlays["directionsOverlay"];

			MySceneView.SpatialReferenceChanged += MySceneView_SpatialReferenceChanged;
		}

		private async void MySceneView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			try
			{
				MySceneView.SpatialReferenceChanged -= MySceneView_SpatialReferenceChanged;
				await MySceneView.LayersLoadedAsync();

				await MySceneView.SetViewAsync(new ViewpointCenter(
					new MapPoint(-117.176302598632, 32.7406774245258, SpatialReferences.Wgs84), 26560));

				_isMapReady = true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.ToString());
			}
		}

        // Get user Stop points
		private void MySceneView_SceneViewTapped(object sender, MapViewInputEventArgs e)
        {
            if (!_isMapReady)
                return;

            try
            {
                e.Handled = true;

                if (_directionsOverlay.Graphics.Count() > 0)
                {
                    panelResults.Visibility = Visibility.Collapsed;

                    _stopsOverlay.Graphics.Clear();
					_routesOverlay.Graphics.Clear();
					_directionsOverlay.GraphicsSource = null;
                }

				_stopsOverlay.Graphics.Add(new Graphic(e.Location));
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
        }

        // Calculate the route
		private async void MySceneView_MapSceneDoubleTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
			if (!_isMapReady || _stopsOverlay.Graphics.Count() < 2)
                return;

            try
            {
                e.Handled = true;

                panelResults.Visibility = Visibility.Collapsed;
                progress.Visibility = Visibility.Visible;

                if (_routeTask == null)
                    _routeTask = await Task.Run<LocalRouteTask>(() => new LocalRouteTask(_localRoutingDatabase, _networkName));

                RouteParameters routeParams = await _routeTask.GetDefaultParametersAsync();
                routeParams.OutSpatialReference = MySceneView.SpatialReference;
                routeParams.ReturnDirections = true;
                routeParams.DirectionsLengthUnit = LinearUnits.Miles;
                routeParams.DirectionsLanguage = new CultureInfo("en-US");
				routeParams.SetStops(_stopsOverlay.Graphics);

                var routeResult = await _routeTask.SolveAsync(routeParams);
                if (routeResult == null || routeResult.Routes == null || routeResult.Routes.Count() == 0)
                    throw new ApplicationException("No route could be calculated");

                var route = routeResult.Routes.First();
				_routesOverlay.Graphics.Add(new Graphic(route.RouteFeature.Geometry));

                _directionsOverlay.GraphicsSource = route.RouteDirections.Select(rd => GraphicFromRouteDirection(rd));

                var totalTime = route.RouteDirections.Select(rd => rd.Time).Aggregate(TimeSpan.Zero, (p, v) => p.Add(v));
                var totalLength = route.RouteDirections.Select(rd => rd.GetLength(LinearUnits.Miles)).Sum();
                txtRouteTotals.Text = string.Format("Time: {0:h':'mm':'ss} / Length: {1:0.00} mi", totalTime, totalLength);

                await MySceneView.SetViewAsync(new Viewpoint(route.RouteFeature.Geometry.Extent.Expand(1.2)));
            }
            catch (AggregateException ex)
            {
                var innermostExceptions = ex.Flatten().InnerExceptions;
                if (innermostExceptions != null && innermostExceptions.Count > 0)
                    MessageBox.Show(innermostExceptions[0].Message, "Sample Error");
                else
                    MessageBox.Show(ex.Message, "Sample Error");
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
				if (_directionsOverlay.Graphics.Count() > 0)
                    panelResults.Visibility = Visibility.Visible;
            }
        }

        private Graphic GraphicFromRouteDirection(RouteDirection rd)
        {
            var graphic = new Graphic(rd.Geometry);
            graphic.Attributes.Add("Direction", rd);
            graphic.Attributes["Length"] = rd.GetLength(LinearUnits.Miles);
            if (rd.Geometry is MapPoint)
                graphic.Symbol = _directionPointSymbol;

            return graphic;
        }
    }
}
