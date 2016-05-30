/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Animation;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Bing;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Toolkit.DataSources;
using System.Windows.Media;

namespace SearchTool
{
    /// <summary>
    /// Provides extension methods for use by the Search add-in
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Inserts spaces where the string contains camel-casing or underscores
        /// </summary>
        /// <param name="s">The string to process</param>
        /// <returns>The string with spaces inserted</returns>
        internal static string InsertSpaces(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            StringBuilder sb = new StringBuilder();
            for (int i = s.Length - 1; i >= 0; i--)
            {
                char c = s[i];
                if (char.IsUpper(c))
                {
                    // For a capital letter, we add a space before
                    sb.Insert(0, new char[] { c });
                    sb.Insert(0, new char[] { ' ' });
                }
                else if (c == '_')
                {
                    // replace underscores with a space
                    sb.Insert(0, new char[] { ' ' });
                }
                else
                {
                    sb.Insert(0, new char[] { c });
                }
            }
            return sb.ToString().Trim();
        }

        /// <summary>
        /// Checks whether a property exists on a given object
        /// </summary>
        internal static bool PropertyExists(this object o, string propertyName)
        {
            return o.GetType().GetProperty(propertyName) != null;
        }

        /// <summary>
        /// Checks whether a method exists on a given object
        /// </summary>
        internal static bool MethodExists(this object o, string methodName)
        {
            return o.GetType().GetMethod(methodName) != null;
        }

        /// <summary>
        /// Gets whether a spatial reference instance is in a Web Mercator projection
        /// </summary>
        internal static bool IsWebMercator(this SpatialReference sref)
        {
            return sref != null && sref.WKID == 3857 || sref.WKID == 102113 || sref.WKID == 102100;
        }

        /// <summary>
        /// Gets whether a spatial reference instance is in the Geographic WGS 1984 projection
        /// </summary>
        internal static bool IsGeographic(this SpatialReference sref)
        {
            return sref != null && sref.WKID == 4326;
        }

        /// <summary>
        /// Clones a given <see cref="ESRI.ArcGIS.Client.Geometry"/>
        /// </summary>
        internal static ESRI.ArcGIS.Client.Geometry.Geometry Clone(this ESRI.ArcGIS.Client.Geometry.Geometry geometry)
        {
            return ((dynamic)geometry).Clone();
        }

        /// <summary>
        /// Retrieves a storyboard for the visual state of a given <see cref="FrameowrkElement"/>
        /// </summary>
        /// <param name="element"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        internal static Storyboard FindStoryboard(this FrameworkElement element, string state)
        {
            if (element == null)
                return null;

            // Search for the storyboard the visual states defined on the element itself
            var vsgs = VisualStateManager.GetVisualStateGroups(element);
            if (vsgs != null && vsgs.Count > 0)
            {
                foreach (VisualStateGroup vsg in vsgs)
                {
                    if (vsg.States != null)
                    {
                        foreach (VisualState vs in vsg.States)
                        {
                            if (vs.Name == state)
                                return vs.Storyboard;
                        }
                    }
                }
            }

            // Look for the visual state on the first child object.  This is generally where states are declared
            // when they are part of a ControlTemplate.
            FrameworkElement child = null;
            if (System.Windows.Media.VisualTreeHelper.GetChildrenCount(element) > 0)
                child = System.Windows.Media.VisualTreeHelper.GetChild(element, 0) as FrameworkElement;

            vsgs = VisualStateManager.GetVisualStateGroups(child);
            if (vsgs != null && vsgs.Count > 0)
            {
                foreach (VisualStateGroup vsg in vsgs)
                {
                    if (vsg.States != null)
                    {
                        foreach (VisualState vs in vsg.States)
                        {
                            if (vs.Name == state)
                                return vs.Storyboard;
                        }
                    }
                }
            }
            return null;
        }

        internal static bool IsRunning(this Storyboard storyboard)
        {
            return (storyboard.GetCurrentState() == ClockState.Active || storyboard.GetCurrentState() == ClockState.Filling);
        }

        /// <summary>
        /// Returns the first ancestor in the visual tree of the specified type
        /// </summary>
        internal static T FindAncestorOfType<T>(this DependencyObject obj) where T : DependencyObject
        {
            while (obj != null)
            {
                obj = VisualTreeHelper.GetParent(obj);
                var objAsT = obj as T;
                if (objAsT != null)
                    return objAsT;
            }
            return null;
        }

        /// <summary>
        /// Gets a display name for the specified object
        /// </summary>
        public static string GetDisplayNameFromAttribute(this object o)
        {
            Type t = o.GetType();
            DisplayNameAttribute attribute = Attribute.GetCustomAttribute(
                t, typeof(DisplayNameAttribute)) as DisplayNameAttribute;

            string displayName = attribute != null ? attribute.Name : null;
            if (displayName == null)
                displayName = t.Name.InsertSpaces();

            return displayName;
        }

        /// <summary>
        /// Gets a description for the specified object
        /// </summary>
        public static string GetDescription(this object o)
        {
            Type t = o.GetType();
            DescriptionAttribute attribute = Attribute.GetCustomAttribute(
                t, typeof(DescriptionAttribute)) as DescriptionAttribute;

            string Description = attribute != null ? attribute.Description : null;
            if (Description == null)
                Description = string.Empty;

            return Description;
        }

        /// <summary>
        /// Serializes the object using DataContract serialization
        /// </summary>
        public static string DataContractSerialize<T>(this T o, IEnumerable<Type> knownTypes = null)
        {
            string serializedObject = "";
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Serialize the bookmarks collection
                DataContractSerializer serializer = new DataContractSerializer(typeof(T), knownTypes);
                serializer.WriteObject(memoryStream, o);
                memoryStream.Position = 0;
                using (StreamReader reader = new StreamReader(memoryStream, Encoding.UTF8))
                {
                    serializedObject = reader.ReadToEnd();
                }
            }

            return serializedObject;
        }

        /// <summary>
        /// Deserializes the string into an object using DataContract serialization
        /// </summary>
        public static T DataContractDeserialize<T>(this string s, IEnumerable<Type> knownTypes = null)
        {
            T obj;
            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(s)))
            {
                DataContractSerializer xmlSerializer = new DataContractSerializer(typeof(T), knownTypes);
                memoryStream.Position = 0;
                obj = (T)xmlSerializer.ReadObject(memoryStream);
                memoryStream.Close();
            }

            return obj;
        }

        public static Envelope ToEnvelope(this MapPoint point, double width, double height)
        {
            double widthExpansion = width / 2;
            double heightExpansion = height / 2;

            double xMin = point.X - widthExpansion;
            double xMax = point.X + widthExpansion;

            // Make extent height small relative to width to ensure that the specified width is honored
            double yMin = point.Y - heightExpansion;
            double yMax = point.Y + heightExpansion;

            return new Envelope(xMin, yMin, xMax, yMax) { SpatialReference = point.SpatialReference.Clone() };
        }

        /// <summary>
        /// Determines the map units for the map
        /// </summary>
        /// <param name="map">The map to find map units for</param>
        /// <param name="onComplete">The method to invoke once map units have been retrieved</param>
        /// <param name="onFailed">The method to invoke when map unit retrieval fails</param>
        public static void GetMapUnitsAsync(this Map Map, Action<LengthUnits> onComplete,
            Action onFailed = null)
        {
            if (Map != null)
            {
                Layer layer = Map.Layers.FirstOrDefault();
                if (layer != null)
                {
                    TileLayer tiledLayer = layer as TileLayer;
                    if (tiledLayer != null)
                    {
                        // Bing is web mercator
                        onComplete(LengthUnits.UnitsMeters);
                    }
                    else
                    {
                        OpenStreetMapLayer osmLayer = layer as OpenStreetMapLayer;
                        if (osmLayer != null)
                        {
                            // Open Streem map is web mercator
                            onComplete(LengthUnits.UnitsMeters);
                        }
                        else
                        {
                            // ArcGIS Server Base Map    
                            string units = null;
                            string layerUrl = null;
                            ArcGISTiledMapServiceLayer agsTiledlayer = layer as ArcGISTiledMapServiceLayer;
                            if (agsTiledlayer != null)
                            {
                                units = agsTiledlayer.Units;
                                layerUrl = agsTiledlayer.Url;
                            }
                            else
                            {
                                ArcGISDynamicMapServiceLayer agsDynamicLayer = layer as ArcGISDynamicMapServiceLayer;
                                if (agsDynamicLayer != null)
                                {
                                    units = agsDynamicLayer.Units;
                                    layerUrl = agsDynamicLayer.Url;
                                }
                                else
                                {
                                    ArcGISImageServiceLayer agsImageLayer = layer as ArcGISImageServiceLayer;
                                    if (agsImageLayer != null)
                                        layerUrl = agsImageLayer.Url;
                                }
                            }

                            if (!string.IsNullOrEmpty(units))
                            {
                                units = units.Replace("esri", "Units"); // replace esri prefix
                                if (Enum.IsDefined(typeof(LengthUnits), units))
                                {
                                    onComplete((LengthUnits)Enum.Parse(typeof(LengthUnits), units, true));
                                }
                                else if (onFailed != null)
                                {
                                    onFailed();
                                }
                            }
                            else if (!string.IsNullOrEmpty(layerUrl))
                            {
                                ArcGISService.GetServiceInfoAsync(layerUrl, null, (o, e) =>
                                    {
                                        if (e.Service != null && !string.IsNullOrEmpty(e.Service.Units))
                                        {
                                            string serviceUnits = e.Service.Units;
                                            serviceUnits = serviceUnits.Replace("esri", "Units"); // replace esri prefix
                                            if (Enum.IsDefined(typeof(LengthUnits), serviceUnits))
                                            {
                                                onComplete((LengthUnits)Enum.Parse(typeof(LengthUnits), serviceUnits, true));
                                            }
                                            else if (onFailed != null)
                                            {
                                                onFailed();
                                            }
                                        }
                                    });
                                return;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the specified DependencyProperty by name
        /// </summary>
        public static DependencyProperty GetDependencyProperty(this DependencyObject d, string name)
        {
            Type type = d.GetType();
            FieldInfo fieldInfo = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
            return (fieldInfo != null) ? fieldInfo.GetValue(null) as DependencyProperty : null;
        }
    }

    /// <summary>
    /// Provides generic attached properties for <see cref="DependencyObject"/>s
    /// </summary>
    public class Properties : DependencyObject
    {
        /// <summary>
        /// Backing DependencyProperty for the <see cref="IsSelected"/> attached property
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
          "IsSelected", typeof(bool), typeof(Properties), null);

        /// <summary>
        /// Sets whether an object is selected
        /// </summary>
        public static void SetIsSelected(DependencyObject d, bool value)
        {
            d.SetValue(IsSelectedProperty, value);
        }

        /// <summary>
        /// Gets whether an object is selected
        /// </summary>
        public static bool GetIsSelected(DependencyObject d)
        {
            return (bool)d.GetValue(IsSelectedProperty);
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="DisplayName"/> property
        /// </summary>
        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.RegisterAttached(
          "DisplayName", typeof(string), typeof(Properties), null);

        /// <summary>
        /// Sets an object's display name
        /// </summary>
        public static void SetDisplayName(DependencyObject d, string value)
        {
            d.SetValue(DisplayNameProperty, value);
        }

        /// <summary>
        /// Gets an object's display name
        /// </summary>
        public static string GetDisplayName(DependencyObject d)
        {
            return d.GetValue(DisplayNameProperty) as string;
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Description"/> property
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.RegisterAttached(
          "Description", typeof(string), typeof(Properties), null);

        /// <summary>
        /// Sets an object's description
        /// </summary>
        public static void SetDescription(DependencyObject d, string value)
        {
            d.SetValue(DescriptionProperty, value);
        }

        /// <summary>
        /// Gets an object's display name
        /// </summary>
        public static string GetDescription(DependencyObject d)
        {
            return d.GetValue(DescriptionProperty) as string;
        }

        /// <summary>
        /// Backing property for the <see cref="AttachedObject"/> property 
        /// </summary>
        public static readonly DependencyProperty AttachedObjectProperty = DependencyProperty.RegisterAttached(
          "AttachedObject", typeof(object), typeof(Properties), null);

        /// <summary>
        /// Creates a generic association between two objects
        /// </summary>
        public static void SetAttachedObject(DependencyObject d, object value)
        {
            d.SetValue(AttachedObjectProperty, value);
        }

        /// <summary>
        /// Gets the association between two objects
        /// </summary>
        public static object GetAttachedObject(DependencyObject d)
        {
            return d.GetValue(AttachedObjectProperty) as object;
        }

        /// <summary>
        /// Attaches a callback used for notification when the specified DependencyProperty changes on the
        /// specified object.
        /// </summary>
        public static DependencyProperty NotifyOnDependencyPropertyChanged(
            string propertyName, DependencyObject source, PropertyChangedCallback callback)
        {
            if (source == null)
                return null;

            var b = new Binding(propertyName) { Source = source };
            DependencyProperty prop = DependencyProperty.RegisterAttached(
                "ListenerProperty" + DateTime.Now.Ticks.ToString(),
                typeof(object),
                typeof(DependencyObject),
                new PropertyMetadata(callback));

            BindingOperations.SetBinding(source, prop, b);

            return prop;
        }

        /// <summary>
        /// Removes bindings for a given property from the specified object
        /// </summary>
        public static void ClearAttachedPropertyListener(DependencyProperty property, DependencyObject source)
        {
            if (source == null || property == null)
                return;

            source.ClearValue(property);
        }
    }
}
