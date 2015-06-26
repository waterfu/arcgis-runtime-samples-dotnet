using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using TestApp.Desktop;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates how to accumulated edits can be saved or canceled altogether.
    /// </summary>
    /// <title>Explicit Save</title>
    /// <category>Editing</category>
    public partial class ExplicitSave : UserControl
    {
        public ExplicitSave()
        {
            InitializeComponent();
			MyMapView.SetView(new Viewpoint(new Envelope(-13045660.491307795, 4036200.4792818795, -13044437.4988552, 4037423.471734474, SpatialReferences.WebMercator)));
        }

        /// <summary>
        /// Adds new feature on tap.
        /// </summary>
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = MyMapView.Scene.Layers["Notes"] as FeatureLayer;
            var table = layer.FeatureTable;
            string message = null;
            try
            {
				var mapPoint = await SceneDrawHelper.DrawPointAsync(MyMapView, CancellationToken.None);
                var feature = new GeodatabaseFeature(table.Schema)
                {
                    Geometry = mapPoint
                };
                await table.AddAsync(feature);
            }
            catch (TaskCanceledException te)
            {
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }

        /// <summary>
        /// Saves accumulated edits.
        /// </summary>
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = MyMapView.Scene.Layers["Notes"] as FeatureLayer;
            var table = (ArcGISFeatureTable)layer.FeatureTable;            
            string message = null;
            try
            {
                if (!table.HasEdits)
                    return;
                if (table is ServiceFeatureTable)
                {
                    var serviceTable = (ServiceFeatureTable)table;
                    // Pushes accumulated edits back to the server.
                    var saveResult = await serviceTable.ApplyEditsAsync();
                    if (saveResult != null && saveResult.AddResults != null
                        && saveResult.AddResults.All(r => r.Error == null && r.Success))
                        message = string.Format("Saved {0} features", saveResult.AddResults.Count);
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }

        /// <summary>
        /// Cancels accumulated edits.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = MyMapView.Scene.Layers["Notes"] as FeatureLayer;
            var table = (ArcGISFeatureTable)layer.FeatureTable;
            if (table.HasEdits)
                table.ClearEdits();
        }
    }
}