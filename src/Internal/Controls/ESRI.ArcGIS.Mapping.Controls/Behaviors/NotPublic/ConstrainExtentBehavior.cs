/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Geometry;
using ESRI.ArcGIS.Client.Tasks;
using ESRI.ArcGIS.Mapping.Core;
using System.Windows;

namespace ESRI.ArcGIS.Mapping.Controls
{
    //[Export(typeof(Behavior<Map>))]
    //[DisplayName("Constrain Map Extent")]
    //[Category("Map")]
    //[Description("Set constrains to the extent of map")]
    public class ConstrainExtentBehavior : Behavior<Map>, ISupportsConfiguration
    {
        private Envelope _constrainedExtent;

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.ExtentChanging += AssociatedObject_ExtentChanging;
            this.AssociatedObject.ExtentChanged += AssociatedObject_ExtentChanging;

            if (View.Instance != null)
            {
                View.Instance.BaseMapChangeComplete -= Instance_BaseMapChangeComplete;
                View.Instance.BaseMapChangeComplete += Instance_BaseMapChangeComplete;
            }
            SetMapConstraint();
        }

        void Instance_BaseMapChangeComplete(object sender, EventArgs e)
        {
            SetMapConstraint();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.ExtentChanging -= AssociatedObject_ExtentChanging;
            this.AssociatedObject.ExtentChanged -= AssociatedObject_ExtentChanging;

            if (View.Instance != null)
            {
                View.Instance.BaseMapChangeComplete -= Instance_BaseMapChangeComplete;
            }
        }

        void SetMapConstraint()
        {
            if (AssociatedObject == null)
                return;

            if (_constrainedExtent == null || _constrainedExtent.SpatialReference == null)
                return;

            SpatialReference mapSpatialRef = AssociatedObject.SpatialReference;

            // If the layer's spatial reference matches the map's, set the constraint extent
            if ((_constrainedExtent.SpatialReference.WKID > 0 && _constrainedExtent.SpatialReference.WKID == mapSpatialRef.WKID) ||
                (!string.IsNullOrEmpty(_constrainedExtent.SpatialReference.WKT) && _constrainedExtent.SpatialReference.WKT == mapSpatialRef.WKT))
            {
                if(!_constrainedExtent.Equals(ConstrainedExtent))
                    ConstrainedExtent = _constrainedExtent;
            }
            else
            {
                // Otherwise, use a geometry service to project the extent
                if (MapApplication.Current == null || MapApplication.Current.Urls == null || String.IsNullOrEmpty(MapApplication.Current.Urls.GeometryServiceUrl))
                    return;

                GeometryService geometryService = new GeometryService(MapApplication.Current.Urls.GeometryServiceUrl);
                geometryService.ProjectCompleted += (o, e1) =>
                {
                    Envelope newExtent = null;
                    if (e1.Results != null && e1.Results.Count > 0 && e1.Results[0].Geometry != null)
                        newExtent = e1.Results[0].Geometry.Extent;

                    if (!newExtent.Equals(ConstrainedExtent))
                        ConstrainedExtent = newExtent;
                };

                geometryService.Failed += (o, e1) =>
                {
                    NotificationPanel.Instance.AddNotification("Reprojecting extent of ConstrainExtentBehavior failed",
                        "Failed to project the specified extent into the map's spatial reference.", string.Format("{0}\n\n{1}",
                        e1.Error.Message, e1.Error.StackTrace), MessageType.Warning);
                };

                geometryService.ProjectAsync(new Graphic[] { new Graphic() { Geometry = _constrainedExtent } }, mapSpatialRef);
            }
        }

        public void Configure()
        {
            ConstrainExtentBehaviorConfigControl config = new ConstrainExtentBehaviorConfigControl();
            config.ConstrainedExtent = _constrainedExtent;
            if (config != null)
            {
                MapApplication.Current.ShowWindow(LocalizableStrings.GetString("ConstrainExtentBehaviorTitle"), config, false, null, delegate(object obj, EventArgs args1)
                                {
                                    if (config != null && config.ConstrainedExtent != null)
                                    {
                                        ConstrainedExtent = _constrainedExtent = config.ConstrainedExtent;
                                    }
                                });
            }
        }

        public void LoadConfiguration(string configData)
        {
            ConstraintFromXml(configData);
        }

        public string SaveConfiguration()
        {
            return _constrainedExtent != null ? ConstraintToXml() : null;
        }

        private void ConstraintFromXml(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            _constrainedExtent = XmlSerializer.Deserialize<Envelope>(value);
        }

        private string ConstraintToXml()
        {
            return XmlSerializer.Serialize<Envelope>(_constrainedExtent);
        }


        /// <summary>
        /// Gets or sets the constrained extent.
        /// </summary>
        /// <value>The constrained extent.</value>
        public Envelope ConstrainedExtent
        {
            get { return (Envelope)GetValue(ConstrainedExtentProperty); }
            set { SetValue(ConstrainedExtentProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ConstrainedExtent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ConstrainedExtentProperty =
            DependencyProperty.Register("ConstrainedExtent", typeof(Envelope), typeof(ConstrainExtentBehavior), null);

        private Envelope lastExtentChecked;
        private void AssociatedObject_ExtentChanging(object sender, ExtentEventArgs e)
        {
            if (ConstrainedExtent == null) return;
            if (IsWithin(e.NewExtent)) return;
            if (e.NewExtent.Equals(lastExtentChecked)) return;
            lastExtentChecked = e.NewExtent;
            if (e.NewExtent.Width <= ConstrainedExtent.Width && e.NewExtent.Height <= ConstrainedExtent.Height)
            {
                MapPoint center = e.NewExtent.GetCenter();
                Envelope ext = e.NewExtent.Clone();
                //Pan is sufficient
                if (ext.XMin < ConstrainedExtent.XMin)
                {
                    center.X = ConstrainedExtent.XMin + ext.Width * .5;
                }
                else if (ext.XMax > ConstrainedExtent.XMax)
                {
                    center.X = ConstrainedExtent.XMax - ext.Width * .5;
                }
                if (ext.YMin < ConstrainedExtent.YMin)
                {
                    center.Y = ConstrainedExtent.YMin + ext.Height * .5;
                }
                else if (ext.YMax > ConstrainedExtent.YMax)
                {
                    center.Y = ConstrainedExtent.YMax - ext.Height * .5;
                }
                this.AssociatedObject.PanTo(center);
            }
            else
            {
                var newExt = e.NewExtent.Intersection(ConstrainedExtent) ?? ConstrainedExtent;
                MapPoint c = newExt.GetCenter();
                double ratio = e.NewExtent.Height / e.NewExtent.Width;
                double wb = newExt.Width;
                double hb = newExt.Height;
                if (hb / wb < ratio) { wb = hb / ratio; }
                else { hb = wb * ratio; }
                newExt = new Envelope() { XMin = c.X - wb / 2, YMin = c.Y - hb / 2, XMax = c.X + wb / 2, YMax = c.Y + hb / 2 };

                this.AssociatedObject.Extent = lastExtentChecked = newExt;
            }
        }

        private bool IsWithin(Envelope env)
        {
            return (env.XMin >= ConstrainedExtent.XMin &&
                env.XMax <= ConstrainedExtent.XMax &&
                env.YMin >= ConstrainedExtent.YMin &&
                env.YMax <= ConstrainedExtent.YMax);
        }
    }

}
