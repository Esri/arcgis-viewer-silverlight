/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class WebClientHelper
    {
        public WebClient BackgroundOpenReadAsync(Uri address, object userToken)
        {
            WebClient webClient = WebClientFactory.CreateWebClient();

            // Create a background worker thread to execute the async read without buffering which can only be done on a
            // background thread.
            BackgroundWorker worker = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false,
            };
            worker.DoWork += (sender, args) =>
            {
                webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);
                webClient.OpenReadAsync(address, userToken);
            };
            worker.RunWorkerAsync();

            return webClient;
        }

        private void webClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e == null || e.Cancelled)
                return;

            if (e.Error != null)
            {
                OnBackgroundOpenReadAsyncFailed(new ExceptionEventArgs(e.Error, e.UserState));
                return;
            }

            // Convert stream contents to a string. This processing must be done here, on the background thread, in order to succeed.
            // Once this is done, however, all remaining logic should be executed on the main thread as the original invocation of this
            // function has logic that requires that it run on the main thread (updating UI elements, etc.).
            string temp = "";
            using (var reader = new StreamReader(e.Result))
            {
                temp = reader.ReadToEnd();
            }

            if (string.IsNullOrEmpty(temp))
            {
                OnBackgroundOpenReadAsyncFailed(new ExceptionEventArgs(new Exception("Empty response!"), e.UserState));
                return;
            }

            OnBackgroundOpenReadAsyncCompleted(new BackgroundOpenReadAsyncEventArgs() 
                {
                    OrcEventArgs = e,
                    Json = temp,
                    UserToken = e.UserState 
                });
        }

        protected virtual void OnBackgroundOpenReadAsyncFailed(ExceptionEventArgs args)
        {
            if (BackgroundOpenReadAsyncFailed != null)
            {
                if (ESRI.ArcGIS.Client.Extensibility.MapApplication.Current != null)
                {
                    ESRI.ArcGIS.Client.Extensibility.MapApplication.Current.Dispatcher.BeginInvoke((Action)delegate
                    {
                        BackgroundOpenReadAsyncFailed(this, args);
                    });
                }
                else
                    BackgroundOpenReadAsyncFailed(this, args);
            }
        }

        protected virtual void OnBackgroundOpenReadAsyncCompleted(BackgroundOpenReadAsyncEventArgs args)
        {
            if (BackgroundOpenReadAsyncCompleted != null)
            {
                if (ESRI.ArcGIS.Client.Extensibility.MapApplication.Current != null)
                {
                    ESRI.ArcGIS.Client.Extensibility.MapApplication.Current.Dispatcher.BeginInvoke((Action)delegate
                        {
                            BackgroundOpenReadAsyncCompleted(this, args);
                        });
                }
                else
                    BackgroundOpenReadAsyncCompleted(this, args);
            }
        }

        public event EventHandler<BackgroundOpenReadAsyncEventArgs> BackgroundOpenReadAsyncCompleted;
        public event EventHandler<ExceptionEventArgs> BackgroundOpenReadAsyncFailed;
    }

    public class BackgroundOpenReadAsyncEventArgs : EventArgs
    {
        public OpenReadCompletedEventArgs OrcEventArgs { get; set; }
        public string Json { get; set; }
        public object UserToken { get; set; }
    }
}
