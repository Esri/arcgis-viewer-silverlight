/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Windows;
using ESRI.ArcGIS.Mapping.Core;
using System.Xml.Linq;
using System.Linq;
using ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class StartupHelper
    {
        public static void SetupESRI.ArcGIS.Mapping.Controls.ArcGISOnline(string configFilePath, UriKind configFilePathUriKind)
        {
            Uri uri = new Uri(configFilePath, configFilePathUriKind);
            ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.WebUtil.OpenReadAsync(uri, null, (sender2, e2) =>
            {
                XDocument xml = XDocument.Load(e2.Result);

                var v = (from next in xml.Descendants("ArcGISOnline")
                         select new
                         {
                             Url = next.Element("Url").Value,
                             UrlSecure = next.Element("UrlSecure").Value
                         }).SingleOrDefault();
                ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnlineEnvironment.ArcGISOnline.Initialize(v.Url, v.UrlSecure);
                ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.ArcGISOnlineEnvironment.ArcGISOnline.User.SignedInOut += User_SignedInOut;
                ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.BasemapSupport.GetBasemaps(null);
            });
        }

        static void User_SignedInOut(object sender, EventArgs e)
        {
          ArcGISOnline agol = ArcGISOnlineEnvironment.ArcGISOnline;
          if (agol.User.IsSignedIn)
          {
            SignInSettings.Current.SignInStatus = "Sign Out";
            SignInSettings.Current.SignedInUser = agol.User.Current.FullName;
            SignInSettings.Current.SignInButtonSource = new BitmapImage()
            {
              UriSource = new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/toolbar/SignIn16.png", UriKind.Relative)
            };
            SignInSettings.Current.UserNameVisibility = Visibility.Visible;
            SignInSettings.Current.ButtonWidth = double.NaN;
          }
          else
          {
            SignInSettings.Current.SignInStatus = "Sign In";
            SignInSettings.Current.SignedInUser = "";
            SignInSettings.Current.SignInButtonSource = new BitmapImage()
            {
              UriSource = new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/toolbar/SignIn32.png", UriKind.Relative)
            };
            SignInSettings.Current.UserNameVisibility = Visibility.Collapsed;
            SignInSettings.Current.ButtonWidth = 60;
          }
        }
    }

    public class MapViewInitializer
    {
      public void InitializeMapView(View View)
      {
        //if (ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MapView.Current == null)
        //{
        ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MapView mapView = new ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MapView(null);
        ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.AppControls.PrintTarget = View.Map;
        mapView.MapReadyForInsertion += (o, args) =>
        {
          View.RemoveMap();
          View.AddMap(args.Map);
          bool same = (View.Map == ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MapView.Current.Map);
        };
        mapView.MapReadyForRemoval += (o, args) =>
        {
          View.RemoveMap();
        };
        View.MapRecreated += (o, args) =>
        {
          ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MapView.Current.Map = args.NewMap;
        };
        MapDocument oldDoc = (MapView.Current == null) ? null : MapView.Current.Document;
        ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MapView.Current = mapView;
        mapView.Initialized += mapView_Initialized;
        ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MapDocument doc = ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MapSerialization.MapDocumentHelper.UpdateMapDocument(View.Map, oldDoc);
        //MapViewInitialized += callback;
        ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MapView.Current.Initialize(doc);
        //}
        //else
        //{
        //  ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MapSerialization.MapDocumentHelper.UpdateMapDocument(ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MapView.Current.Map, ESRI.ArcGIS.Mapping.Controls.ArcGISOnline.MapView.Current.Document);
        //  if (MapViewReady)
        //    callback(null, null);
        //  else
        //    MapViewInitialized += callback;
        //}

      }

      void mapView_Initialized(object sender, EventArgs e)
      {
        if (MapViewInitialized != null)
          MapViewInitialized(null, e);
      }
      public event EventHandler MapViewInitialized;
    }
}
