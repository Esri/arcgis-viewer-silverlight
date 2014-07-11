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
using System.Collections.Generic;
using System.IO;

namespace ESRI.ArcGIS.Mapping.Builder.Service
{
    public class HttpMultiPartFormPost
    {
        public void PostForm(Uri formUri, IDictionary<string, string> formParameters, IEnumerable<FileInfo> files, Action<OnPostComplete> onComplete = null, object userState = null)
        {
            string boundary = DateTime.Now.Ticks.ToString("x");
            HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(formUri);
            req.Method = "POST";
            //
            req.ContentType = "multipart/form-data; boundary=" + boundary;
            req.BeginGetRequestStream(new AsyncCallback(delegate(IAsyncResult asynchronousResult)
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
                // End the operation.
                using (Stream postStream = request.EndGetRequestStream(asynchronousResult))
                {
                    StreamWriter writer = new StreamWriter(postStream);
                    try
                    {                        
                        if (files != null)
                        {
                            string startBoundary = null;
                            string header = null;
                            foreach (FileInfo file in files)
                            {
                                startBoundary = "--" + boundary + "\r\n";
                                // Write start boundary
                                writer.Write(startBoundary);

                                // Write the header                                            
                                header = "Content-Disposition: form-data; name=\"" + file.FileParameterName + "\"; filename=\"" + file.FileName + "\"\r\nContent-Type: " + file.FileContentType + "\r\n\r\n";
                                writer.Write(header);

                                // Write the file contents                        
                                writer.Write(file.FileContents);                                
                            }
                        }                        

                        // Write name value pairs            
                        if (formParameters != null)
                        {
                            foreach (KeyValuePair<string, string> formParameter in formParameters)
                            {
                                writeKeyValuePair(boundary, writer, formParameter.Key, formParameter.Value);
                            }
                        }                        

                        // Write close boundary
                        string closeBoundary = "--" + boundary + "--\r\n";
                        writer.Write(closeBoundary);

                        writer.Flush();
                    }
                    catch (Exception)
                    {
                        postStream.Close();
                        if (onComplete != null)
                            onComplete(new OnPostComplete() { UserState = userState, ErrorOccured = true });
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
                    catch (System.Exception)
                    {
                        if (onComplete != null)
                            onComplete(new OnPostComplete() { UserState = userState, ErrorOccured = true });
                        return;
                    }

                    string response = null;
                    Stream streamResponse = resp.GetResponseStream();
                    using (StreamReader streamRead = new StreamReader(streamResponse))
                    {
                        response = streamRead.ReadToEnd();
                    }                    

                    if (onComplete != null)
                        onComplete(new OnPostComplete() { UserState = userState, ResponseText = response });
                }), request);
            }), req); 
        }

        private static void writeKeyValuePair(string boundary, StreamWriter writer, string key, string value)
        {
            string formdataTemplate = ("--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}\r\n");
            string formitem = string.Format(formdataTemplate, key, value);
            writer.Write(formitem);
        }
    }

    public class FileInfo
    {
        public string FileParameterName { get; set; }
        public string FileName { get; set; }
        public string FileContents { get; set; }
        public string FileContentType { get; set; }
    }

    public class OnPostComplete
    {
        public object UserState { get; set; }
        public bool ErrorOccured { get; set; }
        public string ResponseText { get; set; }
    }
}
