using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TestApp.Desktop;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Sample shows how to test the spatial relationship of two geometries.
    /// </summary>
    /// <title>Relationship</title>
	/// <category>Geometry</category>
	public partial class Relationship : UserControl
    {
        private List<Symbol> _symbols;
		private GraphicsOverlay _graphicsOverlay;

        /// <summary>Construct Relationship sample control</summary>
        public Relationship()
        {
            InitializeComponent();

            _symbols = new List<Symbol>();
            _symbols.Add(layoutGrid.Resources["PointSymbol"] as Symbol);
            _symbols.Add(layoutGrid.Resources["LineSymbol"] as Symbol);
            _symbols.Add(layoutGrid.Resources["FillSymbol"] as Symbol);

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];

            MyMapView.CameraChanged += MyMapView_ExtentChanged;
        }

        // Start map interaction
        private void MyMapView_ExtentChanged(object sender, EventArgs e)
        {
            try
            {
				MyMapView.CameraChanged -= MyMapView_ExtentChanged;
                btnDraw.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Relationship Sample");
            }
        }

        // Accepts two user shapes and adds them to the graphics layer
        private async Task AcceptShapeAsync()
        {
			// Shape One
			DrawShape drawShape1 = (DrawShape)comboShapeOne.SelectedItem;
			Esri.ArcGISRuntime.Geometry.Geometry shapeOne = null;
			if (drawShape1 == DrawShape.Point)
				shapeOne = await SceneDrawHelper.DrawPointAsync(MyMapView, CancellationToken.None);
			else if (drawShape1 == DrawShape.Polyline)
				shapeOne = await SceneDrawHelper.DrawPolylineAsync(MyMapView, CancellationToken.None);
			else if (drawShape1 == DrawShape.Polygon)
				shapeOne = await SceneDrawHelper.DrawPolygonAsync(MyMapView, CancellationToken.None);
			shapeOne = GeometryEngine.Project(shapeOne, SpatialReferences.WebMercator);
			_graphicsOverlay.Graphics.Add(new Graphic(shapeOne, _symbols[comboShapeOne.SelectedIndex]));
			var drawShape2 = (DrawShape)comboShapeTwo.SelectedItem;
			Esri.ArcGISRuntime.Geometry.Geometry shapeTwo = null;
			// Shape Two
			if (drawShape2 == DrawShape.Point)
				shapeTwo = await SceneDrawHelper.DrawPointAsync(MyMapView, CancellationToken.None);
			else if (drawShape2 == DrawShape.Polyline)
				shapeTwo = await SceneDrawHelper.DrawPolylineAsync(MyMapView, CancellationToken.None);
			else if (drawShape2 == DrawShape.Polygon)
				shapeTwo = await SceneDrawHelper.DrawPolygonAsync(MyMapView, CancellationToken.None);
			shapeTwo = GeometryEngine.Project(shapeTwo, SpatialReferences.WebMercator);

			_graphicsOverlay.Graphics.Add(new Graphic(shapeTwo, _symbols[comboShapeTwo.SelectedIndex]));

            Dictionary<string, bool> relations = new Dictionary<string, bool>();
            relations["Contains"] = GeometryEngine.Contains(shapeOne, shapeTwo);
            relations["Crosses"] = GeometryEngine.Crosses(shapeOne, shapeTwo);
            relations["Disjoint"] = GeometryEngine.Disjoint(shapeOne, shapeTwo);
            relations["Equals"] = GeometryEngine.Equals(shapeOne, shapeTwo);
            relations["Intersects"] = GeometryEngine.Intersects(shapeOne, shapeTwo);
            relations["Overlaps"] = GeometryEngine.Overlaps(shapeOne, shapeTwo);
            relations["Touches"] = GeometryEngine.Touches(shapeOne, shapeTwo);
            relations["Within"] = GeometryEngine.Within(shapeOne, shapeTwo);

            resultsPanel.Visibility = Visibility.Visible;
            resultsListView.ItemsSource = relations;
        }

        private async void StartDrawingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnDraw.IsEnabled = false;
                resultsPanel.Visibility = Visibility.Collapsed;

				_graphicsOverlay.Graphics.Clear();
                await AcceptShapeAsync();
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Relationship Sample");
            }
            finally
            {
                btnDraw.IsEnabled = true;
            }
        }
    }
}
