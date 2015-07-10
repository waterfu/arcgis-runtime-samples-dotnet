using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates Querying the elevation of a local file elevation source
	/// </summary>
	/// <title>3D Query Elevation</title>
	/// <category>Scene</category>
	/// <subcategory>Elevation</subcategory>
	public partial class QueryElevation3d : UserControl
	{
		private bool _isSceneReady;

		public QueryElevation3d()
		{
			InitializeComponent();
			MySceneView.SpatialReferenceChanged += MySceneView_SpatialReferenceChanged;
		}

		private async void MySceneView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			MySceneView.SpatialReferenceChanged -= MySceneView_SpatialReferenceChanged;

			MySceneView.SetViewAsync(new Camera(new MapPoint(-156.277, 18.356, 58877.626), 20.091, 70.160), new TimeSpan(0, 0, 5));

			await MySceneView.LayersLoadedAsync();
			_isSceneReady = true;

			MySceneView.MouseMove += MySceneView_MouseMove;
		}

		void MySceneView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			System.Windows.Point screenPoint = e.GetPosition(MySceneView);
			MapPoint mapPoint = MySceneView.ScreenToLocation(screenPoint);

			if (mapPoint == null)
				return;

			QueryElevation(mapPoint);
		}

		private async Task QueryElevation(MapPoint location)
		{
			if (MySceneView.GetCurrentViewpoint(ViewpointType.BoundingGeometry) == null)
				return;

			if (!_isSceneReady)
				return;

			try
			{
				_isSceneReady = false;
				var fileElevSource = MySceneView.Scene.Surface["elevationLayer"] as FileElevationSource;
				double elevation = await fileElevSource.GetElevationAsync(location);

				if (elevation.ToString() == "NaN")
				{
					mapTip.Visibility = System.Windows.Visibility.Hidden;
					return;
				}

				MapView.SetViewOverlayAnchor(mapTip, location);
				mapTip.Visibility = System.Windows.Visibility.Visible;
				txtElevation.Text = String.Format("Elevation: {0} meters", elevation.ToString());

			}
			catch (Exception ex)
			{
				MessageBox.Show("Error retrieving elevation values: " + ex.Message, "Sample Error");
			}
			finally
			{
				_isSceneReady = true;
			}
		}
	}
}
