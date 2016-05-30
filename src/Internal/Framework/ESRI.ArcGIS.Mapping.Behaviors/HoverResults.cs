/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.Client;
using System.Windows.Data;
using System.Windows.Media;
using ESRI.ArcGIS.Client.Extensibility;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ESRI.ArcGIS.Mapping.Behaviors
{
    public class HoverResults : Control
    {
        public FrameworkElement AttributeContainer;
        public FrameworkElement PopUpContainer;

        public HoverResults()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            AttributeContainer = GetTemplateChild("AttributeContainer") as FrameworkElement;
            PopUpContainer = GetTemplateChild("PopUpContainer") as FrameworkElement;
            if (PopUpContainer != null)
                PopUpContainer.DataContext = PopupInfo;

            BindingOperations.SetBinding(this, AttributesProperty, new Binding());
        }

        public GraphicsLayer Layer
        {
            get { return (GraphicsLayer)GetValue(LayerProperty); }
            set { SetValue(LayerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Layer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayerProperty =
            DependencyProperty.Register("Layer", typeof(GraphicsLayer), typeof(HoverResults), new PropertyMetadata(null));

        public Graphic Graphic
        {
            get { return (Graphic)GetValue(GraphicProperty); }
            set { SetValue(GraphicProperty, value); }
        }

        public Map Map
        {
            get { return (Map)GetValue(MapProperty); }
            set { SetValue(MapProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Map.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MapProperty =
            DependencyProperty.Register("Map", typeof(Map), typeof(HoverResults), null);

        // Using a DependencyProperty as the backing store for Graphic.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GraphicProperty =
            DependencyProperty.Register("Graphic", typeof(Graphic), typeof(HoverResults), null);

        public IDictionary<string, object> Attributes
        {
            get { return (IDictionary<string, object>)GetValue(AttributesProperty); }
            set { SetValue(AttributesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Attributes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AttributesProperty =
            DependencyProperty.Register("Attributes", typeof(IDictionary<string, object>), typeof(HoverResults), new PropertyMetadata(null, OnAttributesChange));

        static void OnAttributesChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            HoverResults hr = o as HoverResults;
            if (hr == null) return;
            hr.Graphic = hr.Layer.Graphics.FirstOrDefault(item => item.Attributes == hr.Attributes); //null or matching graphic
#if DEBUG
            if (hr.Attributes == null)
                Debug.WriteLine(string.Format("@@Change, NULL, {0}", MapApplication.GetLayerName(hr.Layer)));
            else if (hr.Attributes.ContainsKey("NAME"))
                Debug.WriteLine(string.Format("@@Change, {0}, {1}", hr.Attributes["NAME"], MapApplication.GetLayerName(hr.Layer)));
            else
                Debug.WriteLine(string.Format("@@Change (no NAME field), {0}", MapApplication.GetLayerName(hr.Layer)));
#endif
            PositionMapTip.rebuildMapTipContentsBasedOnFieldVisibility(hr);
        }

        public ESRI.ArcGIS.Client.Extensibility.PopupInfo PopupInfo
        {
            get { return (ESRI.ArcGIS.Client.Extensibility.PopupInfo)GetValue(PopupInfoProperty); }
            set { SetValue(PopupInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PopupInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupInfoProperty =
            DependencyProperty.Register("PopupInfo", typeof(ESRI.ArcGIS.Client.Extensibility.PopupInfo), typeof(HoverResults), new PropertyMetadata(null, OnPopupInfoChange));

        static void OnPopupInfoChange(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            HoverResults hr = o as HoverResults;
            PopupInfo oldPopupInfo = args.OldValue as PopupInfo;
            if (oldPopupInfo != null)
                hr.attributeChangeEventHookups(oldPopupInfo, false);
            if (hr.PopupInfo != null)
                hr.attributeChangeEventHookups(hr.PopupInfo, true);
            if (hr.PopUpContainer != null)
                hr.PopUpContainer.DataContext = hr.PopupInfo;
        }

        void attributeChangeEventHookups(PopupInfo popupInfo, bool hookup)
        {
            if (PopupInfo != null)
            {
                if (hookup)
                    PopupInfo.PopupItem.PropertyChanged += PopupItem_PropertyChanged;
                else
                    PopupInfo.PopupItem.PropertyChanged -= PopupItem_PropertyChanged;
            }
        }

        void PopupItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Graphic")
            {
                PopupInfo.PopupItem.Title = ESRI.ArcGIS.Mapping.Core.MapTipsHelper.ConvertExpressionWithFieldNames
                    (PopupInfo.PopupItem, PopupInfo.PopupItem.TitleExpression);
            }
        }

    }
}
