/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using ESRI.ArcGIS.Mapping.Builder.Service;
using System.Collections.Generic;
using ESRI.ArcGIS.Mapping.Builder.Common;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.IO;

namespace ESRI.ArcGIS.Mapping.Builder.FileExplorer
{
    public class FileExplorerClient : ServiceBase
    {
        public FileExplorerClient(string UserId) : base(UserId) { }

        #region GetFiles
        public void GetFilesAsync(string siteId, bool isTemplate, string relativePath, ObservableCollection<string> fileExts, object userState = null)
        {
            Uri uri = CreateRestRequest("Files/Get", string.Format("siteId={0}&isTemplate={1}&relativePath={2}&fileExts={3}", siteId, isTemplate, relativePath, fileExts != null ? string.Join(",", fileExts) : null));
            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += new DownloadStringCompletedEventHandler(onGetFilesCompleted);
            webClient.DownloadStringAsync(uri, userState);
        }

        void onGetFilesCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Cancelled || GetFilesCompleted == null)
                return;

            Exception error = e.Error;
            if (error == null)
                error = GetErrorIfAny(e.Result);
            
            ObservableCollection<FileDescriptor> files = new ObservableCollection<FileDescriptor>();
            if (error == null)
            {
                try
                {
                    XDocument xDoc = XDocument.Parse(e.Result);
                    XElement rootElem = xDoc.Root;
                    if (rootElem != null)
                    {
                        IEnumerable<XElement> fileElems = rootElem.Elements("FileDescriptor");
                        if (fileElems != null)
                        {
                            foreach (XElement fileElem in fileElems)
                            {
                                FileDescriptor file = new FileDescriptor()
                                {
                                    DisplayName = fileElem.Element("DisplayName") != null ? fileElem.Element("DisplayName").Value : null,
                                    FileName = fileElem.Element("FileName") != null ? fileElem.Element("FileName").Value : null,
                                    IsFolder = fileElem.Element("IsFolder") != null ? bool.Parse(fileElem.Element("IsFolder").Value) : false,
                                    RelativePath = fileElem.Element("RelativePath") != null ? fileElem.Element("RelativePath").Value : null,
                                };
                                files.Add(file);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }
            GetFilesCompleted(this, new GetFilesCompletedEventArgs() { 
                 Files = files,
                 UserState = e.UserState,
                 Error = error,
            });
        }
        public EventHandler<GetFilesCompletedEventArgs> GetFilesCompleted { get; set; }
        #endregion

        #region UploadFile
        public void UploadFileToSiteAsync(string siteId, bool isTemplate, string relativePath, string fileName, byte[] fileContents, string fileContentType, object userState = null)
        {            
            Uri uri = CreateRestRequest("Files/Upload", string.Format("siteId={0}&isTemplate={1}&relativePath={2}&filename={3}", siteId, isTemplate, relativePath, fileName));
            HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(uri);
            req.Method = "POST";
            req.BeginGetRequestStream(new AsyncCallback(delegate(IAsyncResult asynchronousResult)
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
                using (Stream postStream = request.EndGetRequestStream(asynchronousResult))
                {                    
                    try
                    {
                        postStream.Write(fileContents, 0, fileContents.Length);
                        postStream.Flush();
                        postStream.Close();
                    }
                    catch (Exception err)
                    {
                        postStream.Close();
                        if (UploadFileToSiteCompleted != null)
                            UploadFileToSiteCompleted(this, new UploadFileToSiteCompletedEventArgs() { UserState = userState, Error = err });
                        return;
                    }
                }
                request.BeginGetResponse(new AsyncCallback(delegate(IAsyncResult asynchronousResult2)
                {
                    HttpWebRequest req2 = (HttpWebRequest)asynchronousResult2.AsyncState;
                    HttpWebResponse resp = null;
                    try
                    {
                        resp = (HttpWebResponse)req2.EndGetResponse(asynchronousResult2);
                    }
                    catch (System.Exception err)
                    {
                        if (UploadFileToSiteCompleted != null)
                            UploadFileToSiteCompleted(this, new UploadFileToSiteCompletedEventArgs() { UserState = userState, Error = err });
                        return;
                    }

                    string response = null;
                    Stream streamResponse = resp.GetResponseStream();
                    using (StreamReader streamRead = new StreamReader(streamResponse))
                    {
                        response = streamRead.ReadToEnd();
                    }                    

                    if (UploadFileToSiteCompleted != null)
                    {
                        FileDescriptor file = null;
                        Exception error = GetErrorIfAny(response);
                        if (error == null)
                        {
                            XDocument xDoc = XDocument.Parse(response);
                            XElement fileElem = xDoc.Root;
                            file = new FileDescriptor()
                            {
                                DisplayName = fileElem.Element("DisplayName") != null ? fileElem.Element("DisplayName").Value : null,
                                FileName = fileElem.Element("FileName") != null ? fileElem.Element("FileName").Value : null,
                                IsFolder = fileElem.Element("IsFolder") != null ? bool.Parse(fileElem.Element("IsFolder").Value) : false,
                                RelativePath = fileElem.Element("RelativePath") != null ? fileElem.Element("RelativePath").Value : null,
                            };
                        }

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            UploadFileToSiteCompleted(this, new UploadFileToSiteCompletedEventArgs()
                            {
                                File = file,
                                UserState = userState,
                                Error = error,
                            });
                        });
                    }
                }), request);
            }), req); 
        }        
        public EventHandler<UploadFileToSiteCompletedEventArgs> UploadFileToSiteCompleted { get; set; }
        #endregion
    }

    public class GetFilesCompletedEventArgs : AsyncCompletedEventArgs
    {
        public ObservableCollection<FileDescriptor> Files { get; set; }
    }

    public class UploadFileToSiteCompletedEventArgs : AsyncCompletedEventArgs
    {
        public FileDescriptor File { get; set; }
    }
}
