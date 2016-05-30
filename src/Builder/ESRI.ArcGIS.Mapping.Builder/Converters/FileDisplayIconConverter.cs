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
using System.Windows.Data;
using System.Linq;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Builder.FileExplorer;
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using ESRI.ArcGIS.Mapping.Builder.Common;

namespace ESRI.ArcGIS.Mapping.Builder
{   
    public class FileDisplayIconConverter : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            FileDescriptor fileDescriptor = value as FileDescriptor;
            if (fileDescriptor != null)
            {
                UpNavigationItem upNavItem = value as UpNavigationItem;
                if (upNavItem != null)
                {
                    return new Uri("/ESRI.ArcGIS.Mapping.Builder;component/images/folderUp.png", UriKind.Relative);
                }
                else
                {
                    if (fileDescriptor.IsFolder)
                        return new Uri("/ESRI.ArcGIS.Mapping.Builder;component/images/folder.png", UriKind.Relative);
                    if (BuilderApplication.Instance.CurrentSite != null)
                    {
                        return new Uri(string.Format("{0}/{1}", BuilderApplication.Instance.CurrentSite.Url.TrimEnd('/'), fileDescriptor.RelativePath), UriKind.Absolute);
                    }
                    else 
                    {
                        Template currentTemplate = BuilderApplication.Instance.CurrentTemplate;
                        if (currentTemplate == null)
                            currentTemplate = BuilderApplication.Instance.Templates.FirstOrDefault();
                        if (currentTemplate != null)
                        {
                            string url = string.Format("{0}/{1}", currentTemplate.BaseUrl.TrimEnd('/'), fileDescriptor.RelativePath);
                            return new Uri(url, UriKind.Absolute);
                        }
                    }
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
