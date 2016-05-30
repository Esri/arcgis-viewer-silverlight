/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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

namespace ESRI.ArcGIS.Mapping.Core
{
    public abstract class LayoutProvider
    {
        private const string MAP = "Map";
        private const string SCALEBARCONTAINER = "ScaleBarContainer";                
        private const string PROGRESSINDICATORCONTAINER = "ProgressIndicatorContainer";
        private const string FEATUREDATAGRIDCONTAINER = "FeatureDataGridContainer";
        private const string ATTRIBUTIONDISPLAYCONTAINER = "AttributionDisplayContainer";
        private const string EDITORCONFIGCONTAINER = "EditorConfigContainer";

        public abstract void GetLayout(object userState, EventHandler<LayoutEventArgs> onCompleted, EventHandler<ExceptionEventArgs> onFailed);

        public static LayoutEventArgs ParseXamlFileContents(string xaml)
        {
            object parsedObject = null;           
            parsedObject = System.Windows.Markup.XamlReader.Load(xaml);

            LayoutEventArgs args = null;
            FrameworkElement elem = parsedObject as FrameworkElement;
            if (elem != null)
            {
                args = new LayoutEventArgs()
                {
                    Content = elem,
                    Map = elem.FindName(MAP) as Map,                    
                    ProgressIndicatorContainer = elem.FindName(PROGRESSINDICATORCONTAINER) as ContentControl,
                    ScaleBarContainer = elem.FindName(SCALEBARCONTAINER) as ContentControl,                    
                    AttributeTableContainer = elem.FindName(FEATUREDATAGRIDCONTAINER) as ContentControl,
                    AttributionDisplayContainer = elem.FindName(ATTRIBUTIONDISPLAYCONTAINER) as ContentControl,
                    EditorConfigContainer = elem.FindName(EDITORCONFIGCONTAINER) as ContentControl,
                };
            }
            return args;
        }
    }

    public class LayoutEventArgs : EventArgs
    {
        public FrameworkElement Content { get; set; }
        public Map Map { get; set; }
        public ContentControl AttributeTableContainer { get; set; }
        public ContentControl ScaleBarContainer { get; set; }       
        public ContentControl ProgressIndicatorContainer { get; set; }
        public ContentControl AttributionDisplayContainer { get; set; }
        public ContentControl EditorConfigContainer { get; set; }
    }
}
