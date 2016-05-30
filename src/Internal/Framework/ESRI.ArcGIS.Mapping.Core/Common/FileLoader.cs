/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace ESRI.ArcGIS.Mapping.Core
{
    internal class FileLoader
    {
        public FileLoader() 
        { 

        }

        public FileLoader(DataFile dataFile)
        {            
            this.File = dataFile;
        }

        public DataFile File { get; set; }

        private WebClient wc;
        public void GetFileAsTextAsync(object userState)
        {
            if (File == null)
                throw new InvalidOperationException(Resources.Strings.ExceptionMustSpecifyFile);

            if (File.IsUrl)
            {
                wc = WebClientFactory.CreateWebClient();
                wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(wc_DownloadStringCompleted);
                wc.DownloadStringAsync(new Uri(File.Path, UriKind.RelativeOrAbsolute), userState);
            }
            else
            {
                OnGetFileAsTextFailed(new ExceptionEventArgs(new Exception(Resources.Strings.ExceptionInvalidFileMustBeUrlForSilverlightApplication), userState));                
            }
        }

        public string GetFileAsText()
        {
            throw new InvalidOperationException(Resources.Strings.ExceptionCannotcallSyncronousMethodGetFileAsText);            
        }

        void  wc_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;

            if (e.Error != null)
            {
                bool redownloadAttempted = WebClientFactory.RedownloadAttempted.Contains(wc);
                if (Utility.IsMessageLimitExceededException(e.Error) && !redownloadAttempted)
                {
                    // Re-issue the request which should serve it out of cache      
                    // and helps us avoid the error which is caused by setting AllowReadStreamBuffering=false
                    // which was used to workaround the problem of SL4 and gzipped content
                    WebClientFactory.RedownloadStringAsync(wc, new Uri(File.Path, UriKind.RelativeOrAbsolute), e.UserState);
                }
                else
                {
                    if (redownloadAttempted) WebClientFactory.RedownloadAttempted.Remove(wc);
                    OnGetFileAsTextFailed(new ExceptionEventArgs(e.Error, e.UserState));
                }
                return;
            }

            OnGetFileAsTextCompleted(new GetFileAsTextCompletedEventArgs() { FileContents = e.Result, UserState = e.UserState });
        }

        public void Cancel()
        {
            if (wc != null && wc.IsBusy)
                wc.CancelAsync();
        }

        protected virtual void OnGetFileAsTextCompleted(GetFileAsTextCompletedEventArgs args)
        {
            if (GetFileAsTextCompleted != null)
                GetFileAsTextCompleted(this, args);
        }

        protected virtual void OnGetFileAsTextFailed(ExceptionEventArgs args)
        {
            if (GetFileAsTextFailed != null)
                GetFileAsTextFailed(this, args);
        }

        public event EventHandler<GetFileAsTextCompletedEventArgs> GetFileAsTextCompleted;
        public event EventHandler<ExceptionEventArgs> GetFileAsTextFailed;
    }

    public class GetFileAsTextCompletedEventArgs : EventArgs
    {
        public string FileContents { get; set; }
        public object UserState { get; set; }
    }
}
