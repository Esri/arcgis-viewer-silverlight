/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client;
using System.Windows.Data;
using System.ComponentModel;
using ESRI.ArcGIS.Client.Geometry;
using System.Windows.Markup;
using ESRI.ArcGIS.Client.Portal;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Object surfacing the members of the current <see cref="MapApplication"/> for use in XAML.  This object
    /// can be declared as a XAML resource, enabling property binding and use in triggers and actions.
    /// </summary>
    public class MapApplicationBindingSource : DependencyObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapApplicationBindingSource"/> object
        /// </summary>
        public MapApplicationBindingSource()
        {
            if (!DesignerProperties.IsInDesignTool && MapApplication.Current != null)
            {
                // try-catch is required for when MapApplicationBindingSource is used in applications that don't
                // fully implement IMapApplication, such as Builder.
                try 
                {   
                    Binding b = new Binding("SelectedLayer") { Source = MapApplication.Current };
                    BindingOperations.SetBinding(this, SelectedLayerProperty, b);

                    b = new Binding("Map") { Source = MapApplication.Current };
                    BindingOperations.SetBinding(this, MapProperty, b);

                    b = new Binding("Urls") { Source = MapApplication.Current };
                    BindingOperations.SetBinding(this, UrlsProperty, b);

                    b = XamlReader.Load(
                        @"<Binding xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                               xmlns:esri=""http://schemas.esri.com/arcgis/client/extensibility/2010""        
                               Path=""SelectedLayer.(esri:MapApplication.LayerName)"" />") as Binding;
                    b.Source = MapApplication.Current;
                    BindingOperations.SetBinding(this, SelectedLayerNameProperty, b);

                    b = new Binding("Portal") { Source = MapApplication.Current };
                    BindingOperations.SetBinding(this, PortalProperty, b);

                    MapApplication.Current.SelectedLayerChanged += OnSelectedLayerChanged;
                    MapApplication.Current.Initialized += OnInitialized;
                    MapApplication.Current.InitializationFailed += OnInitializationFailed;
                }
                catch
                {
                }
            }
            else
            {
                SelectedLayer = createDesignTimeLayer();
                SelectedLayerName = MapApplication.GetLayerName(SelectedLayer);
                Map = new Map();
                Map.Layers.Add(SelectedLayer);
            }
        }

        /// <summary>
        /// Identifies the <see cref="SelectedLayer"/> DependencyProperty
        /// </summary>
        public static DependencyProperty SelectedLayerProperty = DependencyProperty.Register(
            "SelectedLayer", typeof(Layer), typeof(MapApplicationBindingSource), null);

        /// <summary>
        /// Gets or set the application's currently selected layer
        /// </summary>
        public Layer SelectedLayer
        {
            get { return GetValue(SelectedLayerProperty) as Layer; }
            set { SetValue(SelectedLayerProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="SelectedLayerName"/> DependencyProperty
        /// </summary>
        public static DependencyProperty SelectedLayerNameProperty = DependencyProperty.Register(
            "SelectedLayerName", typeof(string), typeof(MapApplicationBindingSource), null);

        /// <summary>
        /// Gets the display name of the <see cref="SelectedLayer"/>
        /// </summary>
        public string SelectedLayerName
        {
            get { return GetValue(SelectedLayerNameProperty) as string; }
            private set { SetValue(SelectedLayerNameProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Map"/> DependencyProperty
        /// </summary>
        public static DependencyProperty MapProperty = DependencyProperty.Register(
            "Map", typeof(Map), typeof(MapApplicationBindingSource), null);

        /// <summary>
        /// Gets the Map object for the current application instance
        /// </summary>
        public Map Map
        {
            get { return GetValue(MapProperty) as Map; }
            private set { SetValue(MapProperty, value); }
        }

        /// <summary>
        /// Gets the URL-accessible endpoints that are used by the application
        /// </summary>
        public ApplicationUrls Urls
        {
            get { return (ApplicationUrls)GetValue(UrlsProperty); }
            private set { SetValue(UrlsProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Urls"/> DependencyProperty
        /// </summary>
        public static readonly DependencyProperty UrlsProperty =
            DependencyProperty.Register("Urls", typeof(ApplicationUrls), typeof(MapApplicationBindingSource), null);

        /// <summary>
        /// Gets the ArcGIS Portal used by the application
        /// </summary>
        public ArcGISPortal Portal
        {
            get { return (ArcGISPortal)GetValue(PortalProperty); }
            private set { SetValue(PortalProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Portal"/> DependencyProperty
        /// </summary>
        public static readonly DependencyProperty PortalProperty =
            DependencyProperty.Register("Portal", typeof(ArcGISPortal), typeof(MapApplicationBindingSource), null);

        #region events
        /// <summary>
        /// Occurs when the application's selected layer changes
        /// </summary>
        public event EventHandler SelectedLayerChanged;

        private void OnSelectedLayerChanged(object sender, EventArgs e)
        {
            if (SelectedLayerChanged != null)
                SelectedLayerChanged(sender, e);
        }

        /// <summary>
        /// Occurs when the application has been initialized
        /// </summary>
        public event EventHandler Initialized;

        private void OnInitialized(object sender, EventArgs e)
        {
            if (Initialized != null)
                Initialized(sender, e);
        }

        /// <summary>
        /// Occurs when application initialization fails
        /// </summary>
        public event EventHandler InitializationFailed;

        private void OnInitializationFailed(object sender, EventArgs e)
        {
            if (InitializationFailed != null)
                InitializationFailed(sender, e);
        }
        #endregion

        #region Design-time (e.g. Blend & VS) data creation

        private GraphicsLayer createDesignTimeLayer()
        {
            GraphicsLayer designTimeLayer = new GraphicsLayer() { ID = "SampleDataLayer" };
            for (int i = 0; i < 20; i++)
                addSampleGraphic(designTimeLayer);
            MapApplication.SetLayerName(designTimeLayer, "Sample Layer");
            return designTimeLayer;
        }

        private void addSampleGraphic(GraphicsLayer gLyr)
        {
            Graphic g = new Graphic()
            {
                Geometry = new MapPoint(-110, 35),
            };
            g.Attributes.Add("Attribute1", "Value1");
            g.Attributes.Add("Attribute2", "Value2");
            g.Attributes.Add("Attribute3", "Value3");
            g.Attributes.Add("Attribute4", "Value4");
            g.Attributes.Add("Attribute5", "Value5");
            g.Attributes.Add("Attribute6", "Value6");
            g.Attributes.Add("Attribute7", "Value7");
            g.Attributes.Add("Attribute8", "Value8");
            g.Attributes.Add("Attribute9", "Value9");
            g.Attributes.Add("Attribute10", "Value10");

            gLyr.Graphics.Add(g);
        }

        #endregion
    }
}
