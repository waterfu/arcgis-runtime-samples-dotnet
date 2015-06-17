﻿using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample demonstrates use of the GeometryEngine to calculate a buffer. To use the sample, click a point on the map. The click point and a buffer of 5 miles around the point will be shown.
	/// </summary>
	/// <title>Buffer 3D</title>
	/// <category>Geometry</category>
	public partial class BufferSample3D : UserControl
	{
		private const double MILES_TO_METERS = 1609.34;

		private PictureMarkerSymbol _pinSymbol;
		private SimpleFillSymbol _bufferSymbol;

		private GraphicsOverlay _graphicOverlay; 

		/// <summary>Construct Buffer sample control</summary>
		public BufferSample3D()
		{
			InitializeComponent();

			_graphicOverlay = MySceneView.GraphicsOverlays["graphicOverlay"];

			SetupSymbols();
		}

		private void MySceneView_SceneViewTapped(object sender, MapViewInputEventArgs e)
		{
			try
			{
				_graphicOverlay.Graphics.Clear();

				// Convert screen point to map point
				var point = e.Location;
				var buffer = GeometryEngine.Buffer(point, 5);

				var bufferGraphic = new Graphic { Geometry = buffer, Symbol = _bufferSymbol };
				_graphicOverlay.Graphics.Add(bufferGraphic);

				var pointGraphic = new Graphic { Geometry = point, Symbol = _pinSymbol };
				_graphicOverlay.Graphics.Add(pointGraphic);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Geometry Engine Failed!");
			}
		}

		private async void SetupSymbols()
		{
			try
			{
				var viewpointGeometry = new Envelope(10863036, 3838021, -10744801, 3887145, SpatialReferences.WebMercator);
				viewpointGeometry = GeometryEngine.Project(viewpointGeometry, SpatialReferences.Wgs84) as Envelope;

				var viewpoint = new ViewpointExtent(viewpointGeometry);
				await MySceneView.SetViewAsync(viewpoint);


				_pinSymbol = new PictureMarkerSymbol() { Width = 48, Height = 48, YOffset = 24 };
				await _pinSymbol.SetSourceAsync(
					new Uri("pack://application:,,,/ArcGISRuntimeSamplesDesktop;component/Assets/RedStickpin.png"));

				_bufferSymbol = layoutGrid.Resources["BufferSymbol"] as SimpleFillSymbol;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error occured : " + ex.Message, "Buffer Sample");
			}
		}
	}
}
