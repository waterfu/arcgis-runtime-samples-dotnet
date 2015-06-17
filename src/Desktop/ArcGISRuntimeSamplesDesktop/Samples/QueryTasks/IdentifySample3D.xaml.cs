﻿using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates performing identify operations. To use the sample, click anywhere in the United States to identify features. The results will be shown in the combo box and list view on the right side of the application. View the data for different results by selecting them from the combo box. In the code-behind, an IdentifyTask is used to perform the identify operation. The tasks IdentifyParameters specify to query the geometry of the map click and to query all the layers in the target map service, which enables returning results from multiple layers.
    /// </summary>
    /// <title>Identify 3D</title>
	/// <category>Tasks</category>
	/// <subcategory>Query</subcategory>
	public partial class IdentifySample3D : UserControl
    {
        /// <summary>Construct Identify sample control</summary>
		public IdentifySample3D()
        {
            InitializeComponent();
        }

        // Identify features at the click point
		private async void MySceneView_SceneViewTapped(object sender, MapViewInputEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;
                resultsGrid.DataContext = null;

				GraphicsOverlay graphicsOverlay = MySceneView.GraphicsOverlays["graphicsOverlay"];
				graphicsOverlay.Graphics.Clear();
				graphicsOverlay.Graphics.Add(new Graphic(e.Location));

				IdentifyParameters identifyParams = new IdentifyParameters(
				e.Location, 
				MySceneView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry.Extent, 
				2, 
				(int)MySceneView.ActualHeight, 
				(int)MySceneView.ActualWidth)
                {
                    LayerOption = LayerOption.Visible,
                    SpatialReference = MySceneView.SpatialReference,
                };

                IdentifyTask identifyTask = new IdentifyTask(
                    new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer"));

                var result = await identifyTask.ExecuteAsync(identifyParams);

                resultsGrid.DataContext = result.Results;
                if (result != null && result.Results != null && result.Results.Count > 0)
                    titleComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Identify Sample");
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
