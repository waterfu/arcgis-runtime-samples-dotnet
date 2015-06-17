﻿using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates how to download a local tile cache from an online service with the ExportTiles operation enabled.
    /// </summary>
    /// <title>Export Tile Cache 3D</title>
    /// <category>Offline</category>
    public partial class ExportTileCache3D : UserControl
    {
        //private const string ONLINE_BASEMAP_URL = "https://tiledbasemaps.arcgis.com/arcgis/rest/services/World_Street_Map/MapServer";
        private const string ONLINE_BASEMAP_TOKEN_URL = ""; //"https://www.arcgis.com/sharing/rest/generatetoken";
        private const string USERNAME = "<Organizational Account UserName>";
        private const string PASSWORD = "<Organizational Account Password>";

        private const string ONLINE_BASEMAP_URL = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer";
        private const string ONLINE_LAYER_ID = "OnlineBasemap";
        private const string LOCAL_LAYER_ID = "LocalTiles";
        private const string TILE_CACHE_FOLDER = "ExportTileCacheSample";

        private ArcGISTiledMapServiceLayer _onlineTiledLayer;
        private GraphicsOverlay _aoiOverlay;
        private ExportTileCacheTask _exportTilesTask;
        private GenerateTileCacheParameters _genOptions;

		public ExportTileCache3D()
        {
            InitializeComponent();

			MySceneView.SpatialReferenceChanged += MySceneView_SpatialReferenceChanged;
        }

		private async void MySceneView_SpatialReferenceChanged(object sender, EventArgs e)
		{
			var extentWGS84 = new Envelope(-123.77, 36.80, -119.77, 38.42, SpatialReferences.Wgs84);
			await MySceneView.SetViewAsync(new Viewpoint(extentWGS84));

			await InitializeOnlineBasemap();

			_aoiOverlay = new GraphicsOverlay()
			{
				Renderer = layoutGrid.Resources["AOIRenderer"] as Renderer,
				ID = "AOIOverlay"
			};
			MySceneView.GraphicsOverlays.Add(_aoiOverlay);

			if (_onlineTiledLayer.ServiceInfo != null)
			{
				sliderLOD.Minimum = 0;
				sliderLOD.Maximum = _onlineTiledLayer.ServiceInfo.TileInfo.Lods.Count - 1;
				sliderLOD.Value = sliderLOD.Maximum;
			}

			_exportTilesTask = new ExportTileCacheTask(new Uri(_onlineTiledLayer.ServiceUri));
		}

        // Create the online basemap layer (with token credentials) and add it to the map
        private async Task InitializeOnlineBasemap()
        {
            try
            {
                _onlineTiledLayer = new ArcGISTiledMapServiceLayer(new Uri(ONLINE_BASEMAP_URL));
                _onlineTiledLayer.ID = _onlineTiledLayer.DisplayName = ONLINE_LAYER_ID;

                // Generate token credentials if using tiledbasemaps.arcgis.com
                if (!string.IsNullOrEmpty(ONLINE_BASEMAP_TOKEN_URL))
                {
                    // Set credentials and token for online basemap
                    var options = new GenerateTokenOptions()
                    {
                        Referer = new Uri(_onlineTiledLayer.ServiceUri)
                    };

                    var cred = await IdentityManager.Current.GenerateCredentialAsync(ONLINE_BASEMAP_TOKEN_URL, USERNAME, PASSWORD);

                    if (cred != null && !string.IsNullOrEmpty(cred.Token))
                    {
                        _onlineTiledLayer.Token = cred.Token;
                        IdentityManager.Current.AddCredential(cred);
                    }
                }

                await _onlineTiledLayer.InitializeAsync();
				MySceneView.Scene.Layers.Add(_onlineTiledLayer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
        }

        // Estimate local tile cache size / space
        private async void EstimateCacheSizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                panelUI.IsEnabled = false;
                panelExport.Visibility = Visibility.Collapsed;
                panelTOC.Visibility = Visibility.Collapsed;
                progress.Visibility = Visibility.Visible;

				var viewpoint = MySceneView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;
                _aoiOverlay.Graphics.Clear();

				//Create polygon from envolope and put it to the overlay
				var polygon = new Polygon(new List<MapPoint>
				{
					new MapPoint(viewpoint.XMax, viewpoint.YMin, SpatialReferences.Wgs84),
					new MapPoint(viewpoint.XMax, viewpoint.YMax, SpatialReferences.Wgs84),
					new MapPoint(viewpoint.XMin, viewpoint.YMax, SpatialReferences.Wgs84),
					new MapPoint(viewpoint.XMin, viewpoint.YMin, SpatialReferences.Wgs84)
				});
				_aoiOverlay.Graphics.Add(new Graphic(polygon));
                
				_aoiOverlay.IsVisible = true;

                _genOptions = new GenerateTileCacheParameters()
                {
                    Format = ExportTileCacheFormat.TilePackage,
                    MinScale = _onlineTiledLayer.ServiceInfo.TileInfo.Lods[(int)sliderLOD.Value].Scale,
                    MaxScale = _onlineTiledLayer.ServiceInfo.TileInfo.Lods[0].Scale,
					GeometryFilter = viewpoint
                };

                var job = await _exportTilesTask.EstimateTileCacheSizeAsync(_genOptions, 
                    (result, ex) =>  // Callback for when estimate operation has completed
                        {
                            if (ex == null) // Check whether operation completed with errors
                            {
                                txtExportSize.Text = string.Format("Tiles: {0} - Size (kb): {1:0}", result.TileCount, result.Size / 1024);
                                panelExport.Visibility = Visibility.Visible;
                                panelTOC.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                MessageBox.Show(ex.Message, "Sample Error");
                            }
                            panelUI.IsEnabled = true;
                            progress.Visibility = Visibility.Collapsed;
                        },
                        TimeSpan.FromSeconds(1), // Check the operation every five seconds
                        CancellationToken.None,
                        new Progress<ExportTileCacheJob>((j) =>  // Callback for status updates
                        {
                            Debug.WriteLine(getTileCacheGenerationStatusMessage(j));
                        }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
                panelExport.Visibility = Visibility.Visible;
                panelTOC.Visibility = Visibility.Collapsed;
            }
        }

        // Download the tile cache
        private async void ExportTilesButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                panelUI.IsEnabled = false;
                panelTOC.Visibility = Visibility.Collapsed;
                progress.Visibility = Visibility.Visible;

                string tilePath = Path.Combine(Path.GetTempPath(), TILE_CACHE_FOLDER);
                var downloadOptions = new DownloadTileCacheParameters(tilePath)
                {
                    OverwriteExistingFiles = true
                };

                var localTiledLayer = MySceneView.Scene.Layers.FirstOrDefault(lyr => lyr.ID == LOCAL_LAYER_ID);
                if (localTiledLayer != null)
					MySceneView.Scene.Layers.Remove(localTiledLayer);


                var result = await _exportTilesTask.GenerateTileCacheAndDownloadAsync(
                    _genOptions, downloadOptions, TimeSpan.FromSeconds(5), CancellationToken.None, 
                    new Progress<ExportTileCacheJob>((job) => // Callback for reporting status during tile cache generation
                    {
                        Debug.WriteLine(getTileCacheGenerationStatusMessage(job));
                    }),
                    new Progress<ExportTileCacheDownloadProgress>((downloadProgress) => // Callback for reporting status during tile cache download
                    {
                        Debug.WriteLine(getDownloadStatusMessage(downloadProgress));
                    }));


                localTiledLayer = new ArcGISLocalTiledLayer(result.OutputPath) { ID = LOCAL_LAYER_ID };
				MySceneView.Scene.Layers.Insert(1, localTiledLayer);

                _onlineTiledLayer.IsVisible = false;
                _aoiOverlay.IsVisible = true;

                panelTOC.Visibility = Visibility.Visible;
                panelExport.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
            finally
            {
                panelUI.IsEnabled = true;
                progress.Visibility = Visibility.Collapsed;
            }
        }

        private static string getTileCacheGenerationStatusMessage(ExportTileCacheJob job)
        {
            if (job.Messages == null)
                return "";

            var text = string.Format("Job Status: {0}\n\nMessages:\n=====================\n", job.Status);
            foreach (GPMessage message in job.Messages)
            {
                text += string.Format("Message type: {0}\nMessage: {1}\n--------------------\n",
                    message.MessageType, message.Description);
            }
            return text;
        }

        private static string getDownloadStatusMessage(ExportTileCacheDownloadProgress downloadProgress)
        {
            return string.Format("Downloading file {0} of {1}...\n{2:P0} complete\n" +
                "Bytes read: {3}", downloadProgress.FilesDownloaded, downloadProgress.TotalFiles, downloadProgress.ProgressPercentage,
                downloadProgress.CurrentFileBytesReceived);
        }

        private void btnDeleteLocalLayer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				var localTiledLayer = MySceneView.Scene.Layers.FirstOrDefault(lyr => lyr.ID == LOCAL_LAYER_ID);
				if (localTiledLayer != null)
					MySceneView.Scene.Layers.Remove(localTiledLayer);

                string tilePath = Path.Combine(Path.GetTempPath(), TILE_CACHE_FOLDER);
                if (Directory.Exists(tilePath))
                    Directory.Delete(tilePath, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
        }

        private void btnResetMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
				var localTiledLayer = MySceneView.Scene.Layers.FirstOrDefault(lyr => lyr.ID == LOCAL_LAYER_ID);
				if (localTiledLayer != null)
					MySceneView.Scene.Layers.Remove(localTiledLayer);

                var extentWGS84 = new Envelope(-123.77, 36.80, -119.77, 38.42, SpatialReferences.Wgs84);
                MySceneView.SetView(new Viewpoint(extentWGS84));

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
            finally
            {
                _aoiOverlay.IsVisible = false;
                _onlineTiledLayer.IsVisible = true;
            }
        }
    }
}
