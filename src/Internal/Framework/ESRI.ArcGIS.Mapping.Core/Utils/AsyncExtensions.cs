/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Portal;
using ESRI.ArcGIS.Mapping.Core.Resources;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Core
{
    /// <summary>
    /// Provides extension methods to allow leveraging async syntax new at Silverlight 5 and Visual Studio 2012
    /// </summary>
    public static class AsyncExtensions
    {
        /// <summary>
        /// Retrieves a credential from the specified service using the specified user information
        /// </summary>
        public static Task<IdentityManager.Credential> GenerateCredentialTaskAsyncEx(
            this IdentityManager manager, string url, string userName, string password,
            IdentityManager.GenerateTokenOptions generateTokenOptions = null)
        {
            var tcs = new TaskCompletionSource<IdentityManager.Credential>();

            // Authenticate using the passed-in info
            manager.GenerateCredentialAsync(url, userName, password, (cred, ex) =>
            {
                if (ex != null)
                    tcs.TrySetException(ex);
                else
                    tcs.TrySetResult(cred);
            }, generateTokenOptions);

            return tcs.Task;
        }

        public static Task<IdentityManager.Credential> GenerateCredentialTaskAsyncEx(
            this IdentityManager manager, string url, 
            IdentityManager.GenerateTokenOptions generateTokenOptions = null)
        {
            var tcs = new TaskCompletionSource<IdentityManager.Credential>();

            // Authenticate using the passed-in info
            manager.GenerateCredentialAsync(url, (cred, ex) =>
            {
                if (ex != null)
                    tcs.TrySetException(ex);
                else
                    tcs.TrySetResult(cred);
            }, generateTokenOptions);

            return tcs.Task;
        }

        /// <summary>
        /// Initializes the <see cref="ArcGISPortal"/> instance
        /// </summary>
        /// <param name="portal"></param>
        /// <returns></returns>
        public static Task InitializeTaskAsync(this ArcGISPortal portal)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            portal.InitializeAsync(portal.Url, (agsPortal, ex) =>
                {
                    if (ex != null)
                        tcs.TrySetException(ex);
                    else
                        tcs.TrySetResult(true);
                });

            return tcs.Task;
        }

        /// <summary>
        /// Gets the position of the element relative to the application root.  Performed
        /// asynchronously in case a rendering pass is required to get the position.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Task<Point?> GetPosition(this UIElement element)
        {
            TaskCompletionSource<Point?> tcs = new TaskCompletionSource<Point?>();

            try
            {
                Point? p = getPosition(element);
                if (p == null || (((Point)p).X == 0 && ((Point)p).Y == 0))
                {
                    element.Dispatcher.BeginInvoke(() =>
                    {
                        tcs.TrySetResult(getPosition(element));
                    });
                }
                else
                {
                    tcs.TrySetResult(p);
                }
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }

            return tcs.Task;
        }

        /// <summary>
        /// Gets the actual width of the element.  Performed asynchronously in case a rendering pass is
        /// required for calculating the width.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Task<double> GetActualWidth(this FrameworkElement element)
        {
            TaskCompletionSource<double> tcs = new TaskCompletionSource<double>();

            if (element.ActualWidth > 0)
                tcs.TrySetResult(element.ActualWidth);
            else
                element.Dispatcher.BeginInvoke(() => { tcs.TrySetResult(element.ActualWidth); });

            return tcs.Task;
        }

        /// <summary>
        /// Issues a GET request to the specified URI and returns the result
        /// </summary>
        public static async Task<ArcGISWebClient.DownloadStringCompletedEventArgs> DownloadStringTaskAsync(
            this ArcGISWebClient client, Uri uri, object userState = null)
        {
            return await client.DownloadStringTaskAsync(uri, null, ArcGISWebClient.HttpMethods.Auto, userState);
        }

        public static bool DownloadStringTaskInProgress;

        /// <summary>
        /// Issues a GET request to the specified URI and returns the result
        /// </summary>
        public static Task<ArcGISWebClient.DownloadStringCompletedEventArgs> DownloadStringTaskAsync(
            this ArcGISWebClient client, Uri uri, IDictionary<string, string> parameters, 
            ArcGISWebClient.HttpMethods httpMethod, object userState = null)
        {
            TaskCompletionSource<ArcGISWebClient.DownloadStringCompletedEventArgs> tcs =
                new TaskCompletionSource<ArcGISWebClient.DownloadStringCompletedEventArgs>();

            EventHandler<ApplicationUnhandledExceptionEventArgs> unhandledExceptionHandler = null;
            EventHandler<ArcGISWebClient.DownloadStringCompletedEventArgs> downloadHandler = null;

            // Handle application unhandled exception due to issue with API where uncatchable deserialization 
            // exception is sometimes thrown for requests that return an error
            unhandledExceptionHandler = (o, e) =>
                {
                    DownloadStringTaskInProgress = false;

                    Application.Current.UnhandledException -= unhandledExceptionHandler;
                    client.DownloadStringCompleted -= downloadHandler;

                    e.Handled = true;

                    tcs.TrySetException(new Exception(Strings.ExceptionNoResult));
                };

            downloadHandler = (o, e) =>
                {
                    DownloadStringTaskInProgress = false;

                    if (Application.Current != null)
                        Application.Current.UnhandledException -= unhandledExceptionHandler;

                    client.DownloadStringCompleted -= downloadHandler;
                    if (e != null)
                        tcs.TrySetResult(e);
                    else
                        tcs.TrySetException(new Exception(Strings.ExceptionNoResult));
                };
            client.DownloadStringCompleted += downloadHandler;

            if (Application.Current != null)
                Application.Current.UnhandledException += unhandledExceptionHandler;

            DownloadStringTaskInProgress = true;
            client.DownloadStringAsync(uri, parameters, httpMethod, userState);

            return tcs.Task;
        }

        /// <summary>
        /// Opens a readable stream to the specified resource
        /// </summary>
        public static async Task<ArcGISWebClient.OpenReadCompletedEventArgs> OpenReadTaskAsync(
            this ArcGISWebClient client, Uri uri, object userState = null)
        {
            return await client.OpenReadTaskAsync(uri, null, ArcGISWebClient.HttpMethods.Auto, userState);
        }

        /// <summary>
        /// Opens a readable stream to the specified resource
        /// </summary>
        public static Task<ArcGISWebClient.OpenReadCompletedEventArgs> OpenReadTaskAsync(
            this ArcGISWebClient client, Uri uri, IDictionary<string, string> parameters = null, 
            ArcGISWebClient.HttpMethods httpMethod = ArcGISWebClient.HttpMethods.Auto, object userState = null)
        {
            TaskCompletionSource<ArcGISWebClient.OpenReadCompletedEventArgs> tcs =
                new TaskCompletionSource<ArcGISWebClient.OpenReadCompletedEventArgs>();

            EventHandler<ArcGISWebClient.OpenReadCompletedEventArgs> openReadHandler = null;
            openReadHandler = (o, e) =>
            {
                client.OpenReadCompleted -= openReadHandler;
                if (e != null)
                    tcs.TrySetResult(e);
                else
                    tcs.TrySetException(new Exception(Strings.ExceptionNoResult));
            };
            client.OpenReadCompleted += openReadHandler;
            client.OpenReadAsync(uri, parameters, httpMethod, userState);

            return tcs.Task;
        }

        private static Point? getPosition(UIElement element)
        {
            Point? p = null;

            if (Application.Current == null || Application.Current.RootVisual == null)
                throw new Exception(Strings.RootVisualError);

            GeneralTransform gt = element.TransformToVisual(Application.Current.RootVisual);
            if (gt != null)
                p = gt.Transform(new Point());

            return p;
        }
    }
}
