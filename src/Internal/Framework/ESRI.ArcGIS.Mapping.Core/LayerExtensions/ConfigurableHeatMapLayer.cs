/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class ConfigurableHeatMapLayer : DynamicLayer
    {
        BackgroundWorker renderThread; //background thread used for generating the heat map
		ESRI.ArcGIS.Client.Geometry.PointCollection heatMapPoints;
		private Envelope fullExtent; //cached value of the calculated full extent

		private struct HeatPoint
		{
			public int X;
			public int Y;
		}

		private struct ThreadSafeGradientStop
		{
			public double Offset;
			public Color Color;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HeatMapLayer"/> class.
		/// </summary>
        public ConfigurableHeatMapLayer()
		{
			GradientStopCollection stops = new GradientStopCollection();
			stops.Add(new GradientStop() { Color = Colors.Transparent, Offset = 0 });
			stops.Add(new GradientStop() { Color = Colors.Blue, Offset = .5 });
			stops.Add(new GradientStop() { Color = Colors.Red, Offset = .75 });
			stops.Add(new GradientStop() { Color = Colors.Yellow, Offset = .8 });
			stops.Add(new GradientStop() { Color = Colors.White, Offset = 1 });
			Gradient = stops;
			HeatMapPoints = new ESRI.ArcGIS.Client.Geometry.PointCollection();
			//Create a separate thread for rendering the heatmap layer.
			renderThread = new BackgroundWorker() { WorkerReportsProgress = false, WorkerSupportsCancellation = true };
			renderThread.ProgressChanged += new ProgressChangedEventHandler(renderThread_ProgressChanged);
			renderThread.RunWorkerCompleted += new RunWorkerCompletedEventHandler(renderThread_RunWorkerCompleted);
			renderThread.DoWork += new DoWorkEventHandler(renderThread_DoWork);
		}

		/// <summary>
		/// The full extent of the layer.
		/// </summary>
		public override Envelope FullExtent
		{
			get
			{
				if (fullExtent == null && heatMapPoints != null && heatMapPoints.Count > 0)
				{
					fullExtent = new Envelope();
					foreach (MapPoint p in heatMapPoints)
					{
						fullExtent = fullExtent.Union(p.Extent);
					}
				}
				return fullExtent;
			}
			protected set { throw new NotSupportedException(); }
		}

		/// <summary>
		/// Identifies the <see cref="Interval"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty IntervalProperty =
						DependencyProperty.Register("Interval", typeof(int), typeof(ConfigurableHeatMapLayer),
						new PropertyMetadata(10, OnIntervalPropertyChanged));

		/// <summary>
		/// Gets or sets the interval.
		/// </summary>
		public int Intensity
		{
			get { return (int)GetValue(IntervalProperty); }
			set { SetValue(IntervalProperty, value); }
		}

		/// <summary>
		/// IntervalProperty property changed handler. 
		/// </summary>
		/// <param name="d">HeatMapLayer that changed its Interval.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnIntervalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if ((int)e.NewValue < 1)
				throw new ArgumentOutOfRangeException("Intensity");
            ConfigurableHeatMapLayer layer = d as ConfigurableHeatMapLayer;
			layer.OnLayerChanged();
		}


		/// <summary>
		/// Identifies the <see cref="Resolution"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ResolutionProperty =
						DependencyProperty.Register("Resolution", typeof(double), typeof(ConfigurableHeatMapLayer),
						new PropertyMetadata(1.0, OnResolutionPropertyChanged));
		/// <summary>
		/// Gets or sets Resolution factor. Set this &lt; 1 to increase performance.
		/// </summary>
		public double Resolution
		{
			get { return (double)GetValue(ResolutionProperty); }
			set { SetValue(ResolutionProperty, value); }
		}

		/// <summary>
		/// ResolutionProperty property changed handler. 
		/// </summary>
		/// <param name="d">HeatMapLayer that changed its Resolution.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnResolutionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            ConfigurableHeatMapLayer layer = d as ConfigurableHeatMapLayer;
			double newValue = (double)e.NewValue;
			if (newValue <= 0 || newValue > 1)
				throw new ArgumentOutOfRangeException("Resolution must be between 0 and 1.");
			layer.OnLayerChanged();
		}


		/// <summary>
		/// Gets or sets the heat map points.
		/// </summary>
		/// <value>The heat map points.</value>
		public ESRI.ArcGIS.Client.Geometry.PointCollection HeatMapPoints
		{
			get { return heatMapPoints; }
			set
			{
				if (heatMapPoints != null)
					heatMapPoints.CollectionChanged -= heatMapPoints_CollectionChanged;
				heatMapPoints = value;
				if (heatMapPoints != null)
					heatMapPoints.CollectionChanged += heatMapPoints_CollectionChanged;
				fullExtent = null;
			}
		}

		/// <summary>
		/// Handles the CollectionChanged event of the heatMapPoints control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		private void heatMapPoints_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			fullExtent = null;
			OnLayerChanged();
		}

		/// <summary>
		/// Identifies the <see cref="Gradient"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty GradientProperty =
						DependencyProperty.Register("Gradient", typeof(GradientStopCollection), typeof(ConfigurableHeatMapLayer),
						new PropertyMetadata(null, OnGradientPropertyChanged));
		/// <summary>
		/// Gets or sets the heat map gradient.
		/// </summary>
		public GradientStopCollection Gradient
		{
			get { return (GradientStopCollection)GetValue(GradientProperty); }
			set { SetValue(GradientProperty, value); }
		}
		/// <summary>
		/// GradientProperty property changed handler. 
		/// </summary>
		/// <param name="d">HeatMapLayer that changed its Gradient.</param>
		/// <param name="e">DependencyPropertyChangedEventArgs.</param> 
		private static void OnGradientPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            ConfigurableHeatMapLayer dp = d as ConfigurableHeatMapLayer;
			dp.OnLayerChanged();
		}


		/// <summary>
		/// Gets the source image to display in the dynamic layer. Override this to generate
		/// or modify images.
		/// </summary>
		/// <param name="extent">The extent of the image being request.</param>
		/// <param name="width">The width of the image being request.</param>
		/// <param name="height">The height of the image being request.</param>
		/// <param name="onComplete">The method to call when the image is ready.</param>
		/// <seealso cref="OnProgress"/>
		protected override void GetSource(Envelope extent, int width, int height, DynamicLayer.OnImageComplete onComplete)
		{
			if (!IsInitialized)
			{
				onComplete(null, -1, -1, null);
				return;
			}

			if (renderThread != null && renderThread.IsBusy)
			{
				renderThread.CancelAsync(); //render already running. Cancel current process.
				while (renderThread.IsBusy) //wait for thread to cancel
				{
#if SILVERLIGHT
					Thread.Sleep(10);
#else
					System.Windows.Forms.Application.DoEvents();
#endif
				}
			}

			//Accessing a GradientStop collection from a non-UI thread is not allowed,
			//so we used a private class gradient collection
			List<ThreadSafeGradientStop> stops = new List<ThreadSafeGradientStop>(Gradient.Count);
			foreach (GradientStop stop in Gradient)
			{
				stops.Add(new ThreadSafeGradientStop() { Color = stop.Color, Offset = stop.Offset });
			}
			//Gradients must be sorted by offset
			stops.Sort((ThreadSafeGradientStop g1, ThreadSafeGradientStop g2) => { return g1.Offset.CompareTo(g2.Offset); });

			List<HeatPoint> points = new List<HeatPoint>();
			double res = (extent.Width / width) / Resolution;
			//adjust extent to include points slightly outside the view so pan won't affect the outcome
			Envelope extent2 = new Envelope(extent.XMin - Intensity * res, extent.YMin - Intensity * res,
				extent.XMax + Intensity * res, extent.YMax + Intensity * res);
			//get points within the extent and transform them to pixel space
			foreach (MapPoint p in HeatMapPoints) 
			{
				if (p.X >= extent2.XMin && p.Y >= extent2.YMin &&
					p.X <= extent2.XMax && p.Y <= extent2.YMax)
				{
					points.Add(new HeatPoint() { 
						X = (int)Math.Round((p.X - extent.XMin) / res), 
						Y = (int)Math.Round((extent.YMax - p.Y) / res) 
					});
				}
			}
			//Start the render thread
			renderThread.RunWorkerAsync(
				new object[] { extent, width, height, this.Intensity, this.Resolution, stops, points, onComplete });
		}

		/// <summary>
		/// Stops loading of any pending images
		/// </summary>
		protected override void Cancel()
		{
			if (renderThread != null && renderThread.IsBusy)
			{
				renderThread.CancelAsync();
			}
			base.Cancel();
		}

		/// <summary>
		/// Handles the DoWork event of the renderThread control. This is where we
		/// render the heatmap outside the UI thread.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.DoWorkEventArgs"/> instance 
		/// containing the event data.</param>
		private void renderThread_DoWork(object sender, DoWorkEventArgs e)
		{            
			BackgroundWorker worker = (BackgroundWorker)sender;
			object[] args = (object[])e.Argument;
			Envelope extent = (Envelope)args[0];
			double res = (double)args[4];
			int width = (int)Math.Ceiling((int)args[1] * res);
			int height = (int)Math.Ceiling((int)args[2] * res);
			int size = (int)Math.Ceiling((int)args[3] * res);
			List<ThreadSafeGradientStop> stops = (List<ThreadSafeGradientStop>)args[5];
			List<HeatPoint> points = (List<HeatPoint>)args[6];
			OnImageComplete onComplete = (OnImageComplete)args[7];

			size = size * 2 + 1;
			ushort[] matrix = CreateDistanceMatrix(size);
			int[] output = new int[width * height];
			foreach (HeatPoint p in points)
			{
				AddPoint(matrix, size, p.X, p.Y, output, width);
				if (worker.CancellationPending)
				{
					e.Cancel = true;
					e.Result = null;
					return;
				}
			}
			matrix = null;
			int max = 0;
			foreach (int val in output) //find max - used for scaling the intensity
				if (max < val) max = val;

			//If we only have single points in the view, don't show them with too much intensity.
			if (max < 2) max = 2; 
			PngEncoder ei = new PngEncoder(width, height);
			for (int idx = 0; idx < height; idx++)      // Height (y)
			{
				int rowstart = ei.GetRowStart(idx);
				for (int jdx = 0; jdx < width; jdx++)     // Width (x)
				{
					Color c = InterpolateColor(output[idx * width + jdx] / (float)max, stops);
					ei.SetPixelAtRowStart(jdx, rowstart, c.R, c.G, c.B, c.A);
				}
				if (worker.CancellationPending)
				{
					e.Cancel = true;
					e.Result = null;
					output = null;
					ei = null;
					return;
				}
				//Raise the progress event for each line rendered
				//worker.ReportProgress((idx + 1) * 100 / height);
			}
			stops.Clear();
			output = null;

			// Get stream and set image source
			e.Result = new object[] { ei, width, height, extent, onComplete };
		}

		/// <summary>
		/// Handles the RunWorkerCompleted event of the renderThread control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
		private void renderThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Cancelled || e.Result == null) return;
			object[] result = (object[])e.Result;
			PngEncoder ei = (PngEncoder)result[0];
			int width = (int)result[1];
			int height = (int)result[2];
			Envelope extent = (Envelope)result[3];
			OnImageComplete onComplete = (OnImageComplete)result[4];
            try
            {
                BitmapImage image = new BitmapImage();
#if SILVERLIGHT            
                image.SetSource(ei.GetImageStream());                
#else       
			    image.BeginInit();
			    image.StreamSource = ei.GetImageStream();
			    image.EndInit();
#endif

                onComplete(image, width, height, extent);
            }
            catch { }
        }

		/// <summary>
		/// Handles the ProgressChanged event of the renderThread control and fires the layer progress event.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.ComponentModel.ProgressChangedEventArgs"/> instance containing the event data.</param>
		private void renderThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			//Raise the layer progress event
			OnProgress(e.ProgressPercentage);
		}

		/// <summary>
		/// Lienarly interpolates a color from a list of colors.
		/// </summary>
		/// <param name="value">The value relative to the gradient stop offsets.</param>
		/// <param name="stops">The color stops sorted by the offset.</param>
		/// <returns></returns>
		private static Color InterpolateColor(float value, List<ThreadSafeGradientStop> stops)
		{
			if (value < 1 / 255f)
				return Colors.Transparent;
			if (stops == null || stops.Count == 0)
				return Colors.Black;
			if (stops.Count == 1)
				return stops[0].Color;

			if (stops[0].Offset >= value) //clip to bottom
				return stops[0].Color;
			else if (stops[stops.Count - 1].Offset <= value) //clip to top
				return stops[stops.Count - 1].Color;
			int i = 0;
			for (i = 1; i < stops.Count; i++)
			{
				if (stops[i].Offset > value)
				{
					Color start = stops[i - 1].Color;
					Color end = stops[i].Color;

					double frac = (value - stops[i - 1].Offset) / (stops[i].Offset - stops[i - 1].Offset);
					byte R = (byte)Math.Round((start.R * (1 - frac) + end.R * frac));
					byte G = (byte)Math.Round((start.G * (1 - frac) + end.G * frac));
					byte B = (byte)Math.Round((start.B * (1 - frac) + end.B * frac));
					byte A = (byte)Math.Round((start.A * (1 - frac) + end.A * frac));
					return Color.FromArgb(A, R, G, B);
				}
			}
			return stops[stops.Count - 1].Color; //should never happen
		}

		/// <summary>
		/// Adds a heat map point to the intensity matrix.
		/// </summary>
		/// <param name="distanceMatrix">The distance matrix.</param>
		/// <param name="size">The size of the distance matrix.</param>
		/// <param name="x">x.</param>
		/// <param name="y">y</param>
		/// <param name="intensityMap">The intensity map.</param>
		/// <param name="width">The width of the intensity map..</param>
		private static void AddPoint(ushort[] distanceMatrix, int size, int x, int y, int[] intensityMap, int width)
		{
			for (int i = 0; i < size * 2 - 1; i++)
			{
				int start = (y - size + 1 + i) * width + x - size;
				for (int j = 0; j < size * 2 - 1; j++)
				{
					if (j + x - size < 0 || j + x - size >= width) continue;
					int idx = start + j;
					if (idx < 0 || idx >= intensityMap.Length)
						continue;
					intensityMap[idx] += distanceMatrix[i * (size * 2 - 1) + j];
				}
			}
		}

		/// <summary>
		/// Creates the distance matrix.
		/// </summary>
		/// <param name="size">The size of the matrix (must be and odd number).</param>
		/// <returns></returns>
		private static ushort[] CreateDistanceMatrix(int size)
		{
			int width = size * 2 - 1;
			ushort[] matrix = new ushort[(int)Math.Pow(width, 2)];
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < width; j++)
				{
					matrix[i * width + j] = (ushort)Math.Max((size - (Math.Sqrt(Math.Pow(i - size + 1, 2) + Math.Pow(j - size + 1, 2)))), 0);
				}
			}
			return matrix;
		}
    }
}
