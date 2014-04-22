﻿using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
	/// <summary>
	/// 
	/// </summary>
	/// <category>Query Tasks</category>

	public sealed partial class AttributeQuery : Page
    {
        public AttributeQuery()
        {
            this.InitializeComponent();
            mapView1.Map.InitialExtent = new Envelope(-15000000, 2000000, -7000000, 8000000);
            InitializeComboBox().ContinueWith((_) => { }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task InitializeComboBox()
        {
            QueryTask queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5"));


            Query query = new Query("1=1")
            {
                ReturnGeometry = false,
            };
            query.OutFields.Add("STATE_NAME");

            try
            {
                var result = await queryTask.ExecuteAsync(query);
                QueryComboBox.ItemsSource = result.FeatureSet.Features.OrderBy(x => x.Attributes["STATE_NAME"]);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private async void QueryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await GetAttributes();
        }

        private async Task GetAttributes()
        {
            QueryTask queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Demographics/ESRI_Census_USA/MapServer/5"));

            var qryText = (string)(QueryComboBox.SelectedItem as Graphic).Attributes["STATE_NAME"];
            Query query = new Query(qryText)
            {
                OutFields = OutFields.All,
                ReturnGeometry = true,
                OutSpatialReference = mapView1.SpatialReference
            };
            try
            {
                ResultsGrid.ItemsSource = null;
                progress.IsActive = true;
                var result = await queryTask.ExecuteAsync(query);
                var featureSet = result.FeatureSet;
                // If an item has been selected            
                GraphicsLayer graphicsLayer = mapView1.Map.Layers["MyGraphicsLayer"] as GraphicsLayer;
                graphicsLayer.Graphics.Clear();

                if (featureSet != null && featureSet.Features.Count > 0)
                {
                    var symbol = LayoutRoot.Resources["DefaultFillSymbol"] as Esri.ArcGISRuntime.Symbology.Symbol;
                    var g = featureSet.Features[0];
                    graphicsLayer.Graphics.Add(g);
                    var selectedFeatureExtent = g.Geometry.Extent;
                    Envelope displayExtent = selectedFeatureExtent.Expand(1.3);
                    mapView1.SetView(displayExtent);
                    ResultsGrid.ItemsSource = g.Attributes;
                }
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                progress.IsActive = false;
            }
        }

        private void KeyLoaded(object sender, object e)
        {
            TextBlock textBlock = (TextBlock)sender;
            dynamic dyn = textBlock.DataContext;
            textBlock.Text = dyn.Key;
        }

        private void ValueLoaded(object sender, object e)
        {
            TextBlock textBlock = (TextBlock)sender;
            dynamic dyn = textBlock.DataContext;
            textBlock.Text = Convert.ToString(dyn.Value, CultureInfo.InvariantCulture);
        }
    }
}