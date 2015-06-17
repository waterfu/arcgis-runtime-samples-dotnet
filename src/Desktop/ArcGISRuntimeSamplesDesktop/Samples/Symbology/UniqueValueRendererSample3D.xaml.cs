using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Sample shows how to create a UniqueValueRenderer for a graphics layer. US state polygons are pulled from an online source and rendered using the GraphicsLayer UniqueValueRenderer.
    /// </summary>
    /// <title>Unique Value Renderer 3D</title>
	/// <category>Symbology</category>
	public partial class UniqueValueRendererSample3D : UserControl
    {
        private Random _random = new Random();
		private GraphicsOverlay _states;

        /// <summary>Construct Unique Value Renderer sample control</summary>
		public UniqueValueRendererSample3D()
        {
            InitializeComponent();

			_states = MySceneView.GraphicsOverlays["states"];

			MySceneView.SpatialReferenceChanged += MySceneView_SpatialReferenceChanged;
		}

		private async void MySceneView_SpatialReferenceChanged(object sender, System.EventArgs e)
		{
			MySceneView.SpatialReferenceChanged -= MySceneView_SpatialReferenceChanged;

			try
			{
				// Wait until all layers are initialized
				await MySceneView.LayersLoadedAsync();

				// Set viewpoint and navigate to it
				var viewpoint = new Camera(
					new MapPoint(
						-122.406025330049,
						37.7890934457207,
						209.54040953517,
						SpatialReferences.Wgs84),
					338.125939203603,
					72.7452621261101);

				await LoadStatesAsync();

				await MySceneView.SetViewAsync(viewpoint, new TimeSpan(0, 0, 1), false);
				
                ChangeRenderer();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading states data: " + ex.Message, "Unique Value Renderer Sample");
            }
		}
	
	    // Change the graphics layer renderer to a new UniqueValueRenderer
        private void ChangeRendererButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeRenderer();
        }

        private void ChangeRenderer()
        {
            var renderer = new UniqueValueRenderer() 
			{ 
				Fields = new ObservableCollection<string>(new List<string> { "sub_region" }) 
			};

			renderer.Infos = new UniqueValueInfoCollection(_states.Graphics
                .Select(g => g.Attributes["sub_region"])
                .Distinct()
                .Select(obj => new UniqueValueInfo { 
					Values = new ObservableCollection<object>(new object[] { obj }), 
					Symbol = GetRandomSymbol() 
				}));

			_states.Renderer = renderer;
        }

        // Load US state data from map service
        private async Task LoadStatesAsync()
        {
            var queryTask = new QueryTask(
                new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2"));
			var query = new Query("1=1")
            {
                ReturnGeometry = true,
				OutSpatialReference = MySceneView.SpatialReference,
                OutFields = new OutFields(new List<string> { "sub_region" })
            };
            var result = await queryTask.ExecuteAsync(query);

			_states.Graphics.Clear();
			_states.Graphics.AddRange(result.FeatureSet.Features.OfType<Graphic>());
        }

        // Utility: Generate a random simple fill symbol
        private SimpleFillSymbol GetRandomSymbol()
        {
            var color = GetRandomColor();

            return new SimpleFillSymbol()
            {
                Color = Color.FromArgb(0x77, color.R, color.G, color.B),
                Outline = new SimpleLineSymbol() { Width = 2, Style = SimpleLineStyle.Solid, Color = color },
                Style = SimpleFillStyle.Solid
            };
        }

        // Utility function: Generate a random System.Windows.Media.Color
        private Color GetRandomColor()
        {
            var colorBytes = new byte[3];
            _random.NextBytes(colorBytes);
            return Color.FromRgb(colorBytes[0], colorBytes[1], colorBytes[2]);
        }
    }
}
