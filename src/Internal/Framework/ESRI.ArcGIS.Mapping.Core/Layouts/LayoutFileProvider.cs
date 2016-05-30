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

namespace ESRI.ArcGIS.Mapping.Core
{
    public class LayoutFileProvider : LayoutProvider
    {
        public string FileUrl { get; set; }
        public string LayoutFileContents { get; set; }

        public override void GetLayout(object userState, EventHandler<LayoutEventArgs> onCompleted, EventHandler<ExceptionEventArgs> onFailed)
        {
            if (!string.IsNullOrEmpty(LayoutFileContents))
            {
                try
                {
                    LayoutEventArgs args = ParseXamlFileContents(LayoutFileContents);
                    if (args != null)
                    {
                        if (onCompleted != null)
                            onCompleted(this, args);
                    }
                    else
                    {
                        if (onFailed != null)
							onFailed(this, new ExceptionEventArgs(Resources.Strings.ExceptionUnableToParseFile, userState));
                    }
                }
                catch (Exception ex)
                {
                    if (onFailed != null)
                        onFailed(this, new ExceptionEventArgs(ex, userState));
                }
                return;
            }

            if (string.IsNullOrEmpty(FileUrl))
                throw new InvalidOperationException(Resources.Strings.ExceptionMustSpecifyFileUrl);

            Uri uri = null;
            if(!Uri.TryCreate(FileUrl, UriKind.RelativeOrAbsolute, out uri))
                throw new InvalidOperationException(Resources.Strings.ExceptionMustSpecifyValidFileUrl);

            WebClient webClient = WebClientFactory.CreateWebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(layoutFileDownloadCompleted);
            webClient.DownloadStringAsync(uri, new object[]{ onCompleted, onFailed, userState });
        }

        void layoutFileDownloadCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;

            EventHandler<LayoutEventArgs> onCompleted = (e.UserState as object[])[0] as EventHandler<LayoutEventArgs>;
            EventHandler<ExceptionEventArgs> onFailed = (e.UserState as object[])[1] as EventHandler<ExceptionEventArgs>;
            object userState = (e.UserState as object[])[2];

            if (e.Error != null || string.IsNullOrEmpty(e.Result))
            {
                if (onFailed != null)
                    onFailed(this, new ExceptionEventArgs(e.Error ?? new Exception(Resources.Strings.ExceptionEmptyFile), userState));
                return;
            }

            string xaml = e.Result;
            LayoutFileContents = xaml;
            try
            {
                LayoutEventArgs args = ParseXamlFileContents(xaml);
                if (args != null)
                {
                    if (onCompleted != null)
                        onCompleted(this, args);
                }
                else
                {
                    if (onFailed != null)
                        onFailed(this, new ExceptionEventArgs(Resources.Strings.ExceptionUnableToParseFile, userState));
                }
            }
            catch (Exception ex)
            {                
                if (onFailed != null)
                    onFailed(this, new ExceptionEventArgs(ex, userState));
            }
            
        }
    }
}
