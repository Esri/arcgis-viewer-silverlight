/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Mapping.Core.Symbols;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Core
{
    public abstract class CustomGraphicsLayer : GraphicsLayer, ICustomGraphicsLayer
    {
        public CustomGraphicsLayer()
        {
            Fields = new Collection<FieldInfo>();
        }

        public Collection<FieldInfo> Fields
        {
            get
            {
                return (Collection<FieldInfo>)GetValue(LayerExtensions.FieldsProperty);
            }
            set
            {
                SetValue(LayerExtensions.FieldsProperty, value);
            }
        }

        public virtual bool SupportsItemOnClickBehavior
        {
            get 
            {
                return false;
            }
        }

        #region IsItemOnClickBehaviorEnabled
        /// <summary>
        /// 
        /// </summary>
        public bool IsItemOnClickBehaviorEnabled
        {
            get { return (bool) GetValue(IsItemOnClickBehaviorEnabledProperty); }
            set { SetValue(IsItemOnClickBehaviorEnabledProperty, value); }
        }

        /// <summary>
        /// Identifies the IsItemOnClickBehaviorEnabled dependency property.
        /// </summary>
        public static readonly DependencyProperty IsItemOnClickBehaviorEnabledProperty =
            DependencyProperty.Register(
                "IsItemOnClickBehaviorEnabled",
                typeof (bool),
                typeof (CustomGraphicsLayer),
                new PropertyMetadata(true, OnIsItemOnClickBehaviorEnabledPropertyChanged));

        /// <summary>
        /// IsItemOnClickBehaviorEnabledProperty property changed handler.
        /// </summary>
        /// <param name="d">CustomGraphicsLayer that changed its IsItemOnClickBehaviorEnabled.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnIsItemOnClickBehaviorEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomGraphicsLayer source = d as CustomGraphicsLayer;
            source.OnIsItemOnClickBehaviorEnabledChanged();
        }
        #endregion

        private void OnIsItemOnClickBehaviorEnabledChanged()
        {
            UpdateCursorForRenderer();
        }

        protected virtual void UpdateCursorForRenderer()
        {
            changeCursorForRenderer(IsItemOnClickBehaviorEnabled && !LayerExtensions.GetPopUpsOnClick(this));
        }
        public virtual void OnItemClicked(Graphic item)
        {
        }

        [TypeConverter(typeof (SpatialReferenceTypeConverter))]
        public SpatialReference LayerSpatialReference { get; set; }

        [TypeConverter(typeof (SpatialReferenceTypeConverter))]
        public SpatialReference MapSpatialReference { get; set; }

        #region UpdateOnExtentChanged

        [Obsolete("Use LayerExtensions.Get/SetAutoUpdateOnExtentChanged()")]
        public bool UpdateOnExtentChanged
        {
            get { return LayerExtensions.GetAutoUpdateOnExtentChanged(this); }
            set
            {
                //SetValue(UpdateOnExtentChangedProperty, value);
                LayerExtensions.SetAutoUpdateOnExtentChanged(this, value);
            }
        }

        /// <summary>
        /// Identifies the UpdateOnExtentChanged dependency property.
        /// </summary>
        public static readonly DependencyProperty UpdateOnExtentChangedProperty =
            DependencyProperty.Register(
                "UpdateOnExtentChanged",
                typeof(bool),
                typeof(CustomGraphicsLayer),
                new PropertyMetadata(false, OnUpdateOnExtentChangedPropertyChanged));

        private static void OnUpdateOnExtentChangedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomGraphicsLayer source = d as CustomGraphicsLayer;
            source.OnUpdateOnExtentValueChanged(EventArgs.Empty);
        }
        #endregion

        #region PollForUpdates

        [Obsolete("Use LayerExtensions.Get/SetAutoUpdateInterval()")]
        public bool PollForUpdates
        {
            get { return (LayerExtensions.GetAutoUpdateInterval(this) > 0.0d); }
            set
            {
                //SetValue(PollForUpdatesProperty, value);
                if (value)
                {
                    // if there is an existing value already, then leave it.  Otherwise set to default (30000)
                    if (LayerExtensions.GetAutoUpdateInterval(this) <= 0.0d)
                    {
                        LayerExtensions.SetAutoUpdateInterval(this, LayerExtensions.DefaultAutoUpdateInterval);
                        PollIntervalType = PollIntervalType.Seconds;
                    }
                }
                else
                {
                    LayerExtensions.SetAutoUpdateInterval(this, 0.0);
                }
            }
        }

        /// <summary>
        /// Identifies the PollForUpdates dependency property.
        /// </summary>
        public static readonly DependencyProperty PollForUpdatesProperty =
            DependencyProperty.Register(
                "PollForUpdates",
                typeof(bool),
                typeof(CustomGraphicsLayer),
                new PropertyMetadata(false, OnPollForUpdatesPropertyChanged));

        /// <summary>
        /// PollForUpdatesProperty property changed handler.
        /// </summary>
        /// <param name="d">CustomGraphicsLayer that changed its PollForUpdates.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnPollForUpdatesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //CustomGraphicsLayer source = d as CustomGraphicsLayer;
            //source.OnPollForUpdatesChanged();
        }
        #endregion

        #region PollInterval (In milliseconds)
        [Obsolete("Use LayerExtensions.Get/SetAutoUpdateInterval()")]
        public double PollInterval
        {
            get { return LayerExtensions.GetAutoUpdateInterval(this); }
            set
            {
                //SetValue(PollIntervalProperty, value);
                if (value >= 0.0d)
                {
                    LayerExtensions.SetAutoUpdateInterval(this, value);
                }
            }
        }

        /// <summary>
        /// Identifies the PollInterval (In milliseconds) dependency property.
        /// </summary>
        public static readonly DependencyProperty PollIntervalProperty =
            DependencyProperty.Register(
                "PollInterval",
                typeof(double),
                typeof(CustomGraphicsLayer),
                new PropertyMetadata(LayerExtensions.DefaultAutoUpdateInterval, OnPollIntervalPropertyChanged));

        /// <summary>
        /// PollInterval Property property changed handler.
        /// </summary>
        /// <param name="d">CustomGraphicsLayer that changed its PollInterval.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnPollIntervalPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //CustomGraphicsLayer source = d as CustomGraphicsLayer;
            //source.OnPollIntervalChanged();
        }
        #endregion

        #region PollIntervalType
        /// <summary>
        /// 
        /// </summary>
        public PollIntervalType PollIntervalType
        {
            get { return (PollIntervalType)GetValue(PollIntervalTypeProperty); }
            set {SetValue(PollIntervalTypeProperty, value);}
        }

        /// <summary>
        /// Identifies the PollIntervalType dependency property.
        /// </summary>
        public static readonly DependencyProperty PollIntervalTypeProperty =
            DependencyProperty.Register(
                "PollIntervalType",
                typeof(PollIntervalType),
                typeof(CustomGraphicsLayer),
                new PropertyMetadata(PollIntervalType.Seconds));
        #endregion

        protected virtual void OnGraphicsCreated(GraphicCollection graphics, bool clearGraphics, EventHandler onGraphicsAddComplete)
        {
            if (graphics == null)
                return;

            if (MapSpatialReference == null || MapSpatialReference.Equals(LayerSpatialReference))
            {
                Graphics = graphics; // map spatial reference was unspecified => we assume points match the spatial ref of the match (responsbility is on the developer)
                OnUpdateCompleted(EventArgs.Empty);
                UpdateCursorForRenderer();
                if (onGraphicsAddComplete != null)
                    onGraphicsAddComplete(this, EventArgs.Empty);
            }
            else
            {
                GeometryServiceOperationHelper projectionHelper = GetProjectionHelper((s,e) =>
                {
                    if (clearGraphics)
                        Graphics = new GraphicCollection();
                    foreach (Graphic g2 in e.Graphics)
                    {
                        Graphics.Add(g2);
                    }
                    OnUpdateCompleted(EventArgs.Empty);
                    UpdateCursorForRenderer();
                    if (onGraphicsAddComplete != null)
                        onGraphicsAddComplete(this, EventArgs.Empty);
                });

                if (projectionHelper != null)
                {
                    projectionHelper.ProjectGraphics(graphics, MapSpatialReference);
                }
            }
        }

        public static GeometryServiceOperationHelper GetProjectionHelper(EventHandler<ProjectGraphicsCompletedEventArgs> projectGraphicsCompleted, EventHandler<ProjectPointsCompletedEventArgs> projectPointsCompleted = null)
        {
            string geometryServiceUrl = new ConfigurationStoreHelper().GetGeometryServiceUrl(ConfigurationStoreProvider.DefaultConfigurationStore);
            if (string.IsNullOrEmpty(geometryServiceUrl))
            {
                System.Diagnostics.Debug.WriteLine("Must specify GeometryServiceUrl if spatial reference of the Map and the layer do not match");
                return null;
            }
            GeometryServiceOperationHelper helper = new GeometryServiceOperationHelper(geometryServiceUrl);
            helper.ProjectGraphicsCompleted += projectGraphicsCompleted;
            helper.ProjectPointsCompleted += projectPointsCompleted;

            return helper;
        }

        private void changeCursorForRenderer(bool isClickableCursor)
        {
            SimpleRenderer simpleRenderer = Renderer as SimpleRenderer;
            if (simpleRenderer != null)
            {
                changeCursorForSymbol(simpleRenderer.Symbol, isClickableCursor);
            }
            else
            {
                ClassBreaksRenderer classBreaksRenderer = Renderer as ClassBreaksRenderer;
                if (classBreaksRenderer != null)
                {
                    changeCursorForSymbol(classBreaksRenderer.DefaultSymbol, isClickableCursor);
                    if (classBreaksRenderer.Classes != null)
                    {
                        foreach (ClassBreakInfo classBreak in classBreaksRenderer.Classes)
                            changeCursorForSymbol(classBreak.Symbol, isClickableCursor);
                    }
                }
                else
                {
                    UniqueValueRenderer uniqueValueRenderer = Renderer as UniqueValueRenderer;
                    if (uniqueValueRenderer != null)
                    {
                        changeCursorForSymbol(uniqueValueRenderer.DefaultSymbol, isClickableCursor);
                        if (uniqueValueRenderer.Infos != null)
                        {
                            foreach (UniqueValueInfo uniqueValue in uniqueValueRenderer.Infos)
                                changeCursorForSymbol(uniqueValue.Symbol, isClickableCursor);
                        }
                    }
                }
            }
        }

        private void changeCursorForSymbol(Symbol symbol, bool isClickableCursor)
        {
            if (symbol == null)
                return;

            ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol imageFillSymbol = symbol as ESRI.ArcGIS.Mapping.Core.Symbols.ImageFillSymbol;
            if (imageFillSymbol != null)
                imageFillSymbol.CursorName = isClickableCursor ? "Hand" : "Arrow";
        }

        public virtual void ForceRefresh(EventHandler refreshCompletedHander, EventHandler<TaskFailedEventArgs> refreshFailedHandler)
        {

        }

        public virtual void UpdateOnMapExtentChanged(ExtentEventArgs e)
        {
        }

        protected virtual void CheckForUpdates()
        {
        }

        public virtual bool SupportsNavigateToServiceDetailsUrl()
        {
            return false;
        }
        public virtual Uri GetServiceDetailsUrl()
        {
            return null;
        }

        /// <summary>
        /// Clears the graphics, and fetches a new update from the service.
        /// </summary>
        public void Update()
        {
            CheckForUpdates();
        }


        protected virtual void OnUpdateOnExtentValueChanged(EventArgs args)
        {
            if (UpdateOnExtentValueChanged != null)
                UpdateOnExtentValueChanged(this, args);
        }

        protected virtual void OnUpdateCompleted(EventArgs args)
        {
            if (UpdateCompleted != null)
                UpdateCompleted(this, args);
        }

        protected virtual void OnUpdateFailed(EventArgs args)
        {
            if (UpdateFailed != null)
                UpdateFailed(this, args);
        }
        public event EventHandler UpdateCompleted;
        public event EventHandler UpdateFailed;
        public event EventHandler UpdateOnExtentValueChanged;
    }

    public enum PollIntervalType
    {
        Seconds,
        Minutes
    }

    public class SpatialReferenceTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof (string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof (string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string val = (string)value;
            int wkid = default(int);
            if (int.TryParse(val, out wkid))
                return new SpatialReference(wkid);
            else if (!string.IsNullOrEmpty(val))
                return new SpatialReference(val);
            return null;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            SpatialReference sref = (SpatialReference)value;
            if (!string.IsNullOrEmpty(sref.WKT))
                return sref.WKT;
            else if (sref.WKID == -1)
                return sref.WKID.ToString();
            return null;
        }
    }
}
