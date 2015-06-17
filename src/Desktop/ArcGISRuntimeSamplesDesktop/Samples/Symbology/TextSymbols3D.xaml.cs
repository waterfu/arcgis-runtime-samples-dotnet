using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Sample shows how to create Text Symbols (TextSymbol) and add graphics using the symbols.
    /// </summary>
    /// <title>Text Symbols 3D</title>
	/// <category>Symbology</category>
	public partial class TextSymbols3D : UserControl
    {
        private Random _random = new Random();
        private List<TextSymbol> _symbols;

        /// <summary>Construct Text Symbols sample control</summary>
		public TextSymbols3D()
        {
            InitializeComponent();

			MySceneView.SpatialReferenceChanged += MySceneView_SpatialReferenceChanged;
		}

		private async void MySceneView_SpatialReferenceChanged(object sender, System.EventArgs e)
		{
			MySceneView.SpatialReferenceChanged -= MySceneView_SpatialReferenceChanged;

			try
			{
				// Wait until all layers are initialized
				await MySceneView.LayersLoadedAsync();
				
				await SetupSymbolsAsync();
				DataContext = this;

				await AcceptPointsAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error loading states data: " + ex.Message, "Unique Value Renderer Sample");
			}
		}

        // Cancel current shape request when the symbol selection changes 
        private async void symbolCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MySceneView.Editor.IsActive)
				MySceneView.Editor.Cancel.Execute(null);

            await AcceptPointsAsync();
        }

        // Accept user map clicks and add points to the graphics layer with the selected symbol
        private async Task AcceptPointsAsync()
        {
            try
            {
				while (MySceneView.GetCurrentViewpoint(Esri.ArcGISRuntime.Controls.ViewpointType.BoundingGeometry).TargetGeometry.Extent
				 != null)
                {
					var point = await MySceneView.Editor.RequestPointAsync();

                    var symbol = _symbols[symbolCombo.SelectedIndex];
					graphicsOverlay.Graphics.Add(new Graphic(point, symbol));
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Text Symbols");
            }
        }

        // Create text symbols - text for the symbol is the name of the font but could be anything
        private async Task SetupSymbolsAsync()
        {
            try
            {
				var fontFamilies = new List<string>()
                {
                    "Bogus", "Algerian", "Chiller", "Comic Sans MS",
                    "Cooper", "Elephant", "Forte", "Jokerman",
                    "Lindsey", "Mistral", "Motorwerk", "Old English Text MT",
                    "Parchment", "Ravie", "Script MT", "Segoe Print",
                    "Showcard Gothic", "Snap ITC", "Vivaldi", "Wingdings"
                };

				// Create symbols from font list
				_symbols = fontFamilies
					.Select(f => new TextSymbol()
					{
						Text = f,
						Color = GetRandomColor(),
						HorizontalTextAlignment = HorizontalTextAlignment.Center,
						VerticalTextAlignment = VerticalTextAlignment.Middle,
						Font = new SymbolFont(f, 20)
					})
					.ToList();

                // Create image swatches for the UI
                Task<ImageSource>[] swatchTasks = _symbols
                    .Select(sym => sym.CreateSwatchAsync())
                    .ToArray();

                symbolCombo.ItemsSource = new List<ImageSource>(await Task.WhenAll(swatchTasks));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
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
