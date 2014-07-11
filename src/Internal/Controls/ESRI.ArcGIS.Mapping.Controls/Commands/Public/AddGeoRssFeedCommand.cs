/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel.Composition;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
    [DisplayName("AddGeoRssFeedDisplayName")]
    [Category("CategoryMap")]
    [Description("AddGeoRssFeedDescription")]
    public class AddGeoRssFeedCommand : CommandBase
    {
        #region ICommand Members

        EnterUrlDialog enterUrlDialog;
        public override void Execute(object parameter)
        {            
            if (enterUrlDialog == null)
            {
                enterUrlDialog = new EnterUrlDialog(null);
                enterUrlDialog.UrlChosen += (o, e) =>
                {
                    addGeoRssLayer(e);
                    return;
                };
                enterUrlDialog.CancelButtonClicked += (o, e) => {
                    MapApplication.Current.HideWindow(enterUrlDialog);
                };                
            }

            WindowType windowType = MapApplication.Current.IsEditMode ? WindowType.DesignTimeFloating : WindowType.Floating;
            MapApplication.Current.ShowWindow(LocalizableStrings.AddGeoRSSTitle, enterUrlDialog, false, null, null, windowType);            
        }

        private void addGeoRssLayer(UrlChosenEventArgs e)
        {
            if (e.Url == null)
                return;
            Uri targetUri = null;
            if (!Uri.TryCreate(e.Url, UriKind.Absolute, out targetUri))
            {
                return;
            }
            if (View.Instance != null)
            {

                CustomGeoRssLayer geoRssLayer = new CustomGeoRssLayer()
                {
                    Source = targetUri,
                    ID = Guid.NewGuid().ToString("N"),
                };
                Core.LayerExtensions.SetDisplayUrl(geoRssLayer, e.Url);                
                geoRssLayer.SetValue(ESRI.ArcGIS.Client.Extensibility.MapApplication.LayerNameProperty, targetUri.Host);
                geoRssLayer.MapSpatialReference = View.Instance.Map.SpatialReference;
                geoRssLayer.UpdateCompleted +=new EventHandler(applyAutomaticClustering);
                View.Instance.Map.Layers.Add(geoRssLayer); 
            }
            MapApplication.Current.HideWindow(enterUrlDialog);
            OnUrlChosen(e);
        }

        void applyAutomaticClustering(object sender, EventArgs e)
        {
            CustomGeoRssLayer gLayer = sender as CustomGeoRssLayer;
            if (gLayer != null && gLayer.Graphics.Count >= Constants.AutoClusterFeaturesThresholdLimit)
            {
                if (ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetGeometryType(gLayer) == GeometryType.Point && gLayer.Clusterer == null)
                    gLayer.Clusterer = new FlareClusterer();
            }
            if(gLayer != null)
                gLayer.UpdateCompleted -= applyAutomaticClustering;
        }
        #endregion

        protected void OnUrlChosen(UrlChosenEventArgs args)
        {
            if (UrlChosen != null)
                UrlChosen(this, args);
        }

        internal event EventHandler<UrlChosenEventArgs> UrlChosen;
    }
}
