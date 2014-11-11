/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Windows.Browser;
using System.Text;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls.ArcGISOnline
{
  /// <summary>
  /// Provides static utility methods for interacting with the web.
  /// </summary>
  public class WebUtil
  {
    /// <summary>
    /// Helper method to read from a web service asynchronously.
    /// </summary>
    public static void OpenReadAsync(string url, object userState, EventHandler<OpenReadEventArgs> callback, 
        bool forceBrowserAuth = false)
    {
        try
        {
            OpenReadAsync(new Uri(url), userState, callback, forceBrowserAuth);
        }
        catch (Exception ex)
        {
            callback(null, new OpenReadEventArgs(null) { Error = ex });
        }
    }

    static bool m_browserAuthInProgress;
    static Queue<RequestInfo> m_openReadRequests = new Queue<RequestInfo>();
    /// <summary>
    /// Helper method to read from a web service asynchronously.
    /// </summary>
    public static void OpenReadAsync(Uri uri, object userState, EventHandler<OpenReadEventArgs> callback,
        bool forceBrowserAuth = false, string proxyUrl = null)
    {
        System.Diagnostics.Debug.WriteLine(uri.ToString());
        if (m_browserAuthInProgress)
        {
            m_openReadRequests.Enqueue(new RequestInfo(uri, userState, callback, forceBrowserAuth));
        }
        else
        {
            string id = "hiddenSLViewerAuthFrame";
            bool iframeExists = HtmlPage.Document.GetElementById(id) != null;

            if (!iframeExists || forceBrowserAuth)
            {
                m_browserAuthInProgress = true;

                // The portal may be PKI-secured.  Try injecting an iframe into the HTML DOM to hit the portal endpoint.
                // In IE, this will force the browser to perform client certificate authentication.  If the initial request
                // to the portal endpoint fails, the client cert auth will not occur, so we must try this first.

                // Insert the iframe
                var iframe = WebUtil.insertHiddenIFrame("hiddenSLViewerAuthFrame");
                var loadErrorTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };

                // Create a handler for the iframe's onload event
                EventHandler onloadHandler = null;
                onloadHandler = (o, e) =>
                {
                    loadErrorTimer.Stop();
                    loadErrorTimer.Tick -= onloadHandler;
                    iframe.DetachEvent("onload", onloadHandler);
                    doOpenRead(uri, userState, callback);
                };

                // Register the handler with the iframe's onload event and the load error timer's tick event
                iframe.AttachEvent("onload", onloadHandler);
                loadErrorTimer.Tick += onloadHandler;

                // Set the source of the iframe to make it hit the portal.  Append ticks to circumvent caching.
                var tempUrl = uri.ToString() + "&r=" + DateTime.Now.Ticks;
                iframe.SetAttribute("src", tempUrl);

                // Start the load error timer.  If the timer ticks, the onload handler will be called.  This is needed
                // in scenarios where the iframe's onload event never fires.
                loadErrorTimer.Tick += onloadHandler;
                loadErrorTimer.Start();
            }
            else
            {
                doOpenRead(uri, userState, callback);
            }
        }
    }

    private static void doOpenRead(Uri uri, object userState, EventHandler<OpenReadEventArgs> callback, string proxyUrl = null)
    {
        var wc = new ArcGISWebClient() { ProxyUrl = proxyUrl };
        wc.OpenReadCompleted += (sender, e) =>
        {
            // if the request failed because of a security exception - missing clientaccesspolicy file -
            // then try to go thru the proxy server
            //
            if (e.Error is System.Security.SecurityException)
            {
                if (string.IsNullOrEmpty(ArcGISOnlineEnvironment.ConfigurationUrls.ProxyServer))
                {
                    handleQueuedRequests();
                    callback(sender, new OpenReadEventArgs(e));
                    return;
                }

                wc = new ArcGISWebClient() { ProxyUrl = ArcGISOnlineEnvironment.ConfigurationUrls.ProxyServer };
                wc.OpenReadCompleted += (sender2, e2) =>
                {
                    handleQueuedRequests();
                    callback(sender, new OpenReadEventArgs(e2) { UsedProxy = true });
                };

                System.Diagnostics.Debug.WriteLine(uri.ToString());
                wc.OpenReadAsync(uri, null, ArcGISWebClient.HttpMethods.Auto, userState);
            }
            else
            {
                handleQueuedRequests();
                callback(sender, new OpenReadEventArgs(e));
            }
        };
        wc.OpenReadAsync(uri, null, ArcGISWebClient.HttpMethods.Auto, userState);
    }

    private static void handleQueuedRequests()
    {
        m_browserAuthInProgress = false;
        if (m_openReadRequests.Count > 0)
        {
            RequestInfo nextRequest = m_openReadRequests.Dequeue();
            OpenReadAsync(nextRequest.Uri, nextRequest.UserState, nextRequest.Callback, nextRequest.ForceBrowserAuth);
        }
    }

    /// <summary>
    /// Helper method to download a string from a web service asynchronously.
    /// </summary>
    public static void DownloadStringAsync(string url, object userState, DownloadStringCompletedEventHandler callback)
    {
      DownloadStringAsync(new Uri(url), userState, callback);
    }

    /// <summary>
    /// Helper method to download a string from a web service asynchronously.
    /// </summary>
    public static void DownloadStringAsync(Uri uri, object userState, DownloadStringCompletedEventHandler callback)
    {
      System.Diagnostics.Debug.WriteLine(uri.ToString());
      WebClient wc = new WebClient();

      wc.DownloadStringCompleted += callback;
      wc.DownloadStringAsync(uri, userState);
    }

    /// <summary>
    /// Helper method to upload a string from a web service asynchronously (POST).
    /// </summary>
    public static void UploadStringAsync(string url, object userState, string data, UploadStringCompletedEventHandler callback)
    {
      UploadStringAsync(new Uri(url), userState, data, callback);
    }

    /// <summary>
    /// Helper method to upload a string from a web service asynchronously (POST).
    /// </summary>
    public static void UploadStringAsync(Uri uri, object userState, string data, UploadStringCompletedEventHandler callback)
    {
      System.Diagnostics.Debug.WriteLine(uri.ToString());
      WebClient wc = new WebClient();
      wc.UploadStringCompleted += callback;
      wc.UploadStringAsync(uri, null, data, userState);
    }

    /// <summary>
    /// Helper method to perform a multi-part post. If dispatcher is null, the callback occurs on a different thread.
    /// </summary>
    public static void MultiPartPostAsync(string url, IEnumerable<HttpContentItem> contentItems, Dispatcher dispatcher, EventHandler<MultiPartPostEventArgs> callback)
    {
      MultiPartPostAsync(new Uri(url), contentItems, dispatcher, callback);
    }

    /// <summary>
    /// Helper method to perform a multi-part post. If dispatcher is null, the callback occurs on a different thread.
    /// </summary>
    public static void MultiPartPostAsync(Uri uri, IEnumerable<HttpContentItem> contentItems, Dispatcher dispatcher, EventHandler<MultiPartPostEventArgs> callback)
    {
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
      request.Method = "POST";
      string boundary = "---------------" + DateTime.Now.Ticks.ToString();
      request.ContentType = "multipart/form-data; boundary=" + boundary;
      request.BeginGetRequestStream(new AsyncCallback(asyncResult =>
      {
        Stream stream = request.EndGetRequestStream(asyncResult);

        DataContractMultiPartSerializer ser = new DataContractMultiPartSerializer(boundary);
        ser.WriteContent(stream, contentItems);
        stream.Close();
        request.BeginGetResponse(new AsyncCallback(asyncResult2 =>
          {
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asyncResult2);

            if (dispatcher == null)
            {
              callback(null, new MultiPartPostEventArgs() { Result = response.GetResponseStream() });
              response.Close();
            }
            else  // switch to the dispatcher thread
              dispatcher.BeginInvoke(delegate
              {
                callback(null, new MultiPartPostEventArgs() { Result = response.GetResponseStream() });
                response.Close();
              });

          }), request);
      }), request);
    }

    /// <summary>
    /// Internal implementation of the multi-part POST request.
    /// </summary>
    static void PostMultiPartAsync(HttpWebRequest request, IEnumerable<HttpContentItem> contentItems, AsyncCallback callback)
    {
      request.Method = "POST";
      string boundary = "---------------" + DateTime.Now.Ticks.ToString();
      request.ContentType = "multipart/form-data; boundary=" + boundary;
      request.BeginGetRequestStream(new AsyncCallback(asyncResult =>
      {
        Stream stream = request.EndGetRequestStream(asyncResult);

        DataContractMultiPartSerializer ser = new DataContractMultiPartSerializer(boundary);
        ser.WriteContent(stream, contentItems);
        stream.Close();
        request.BeginGetResponse(callback, request);
      }), request);
    }

    /// <summary>
    /// Reads an object from the specified stream. Returns null if the object could
    /// not be read. The format of the object is json.
    /// </summary>
    public static T ReadObject<T>(System.IO.Stream stream) where T : class
    {
      try
      {
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
        return serializer.ReadObject(stream) as T;
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// Saves the specified object to json and returns the text.
    /// </summary>
    public static string SaveObject<T>(T obj) where T : class
    {
      MemoryStream ms = new MemoryStream();
      DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
      serializer.WriteObject(ms, obj);
      byte[] json = ms.ToArray();
      ms.Close();
      return Encoding.UTF8.GetString(json, 0, json.Length);
    }

    /// <summary>
    /// Sets a browser cookie for the specified duration.
    /// </summary>
    public static void SetCookie(string key, string value, string domain, string path, int durationDays)
    {
      if (!HtmlPage.IsEnabled)
        return;

      DateTime expireDate = DateTime.Now + TimeSpan.FromDays(durationDays);
      string newCookie = key + "=" + value + ";expires=" + expireDate.ToString("R");

      if (!string.IsNullOrEmpty(path))
        newCookie += ";path=" + path;

      if (!string.IsNullOrEmpty(domain))
        newCookie += ";domain=" + domain;

      HtmlPage.Document.SetProperty("cookie", newCookie);
    }

    /// <summary>
    /// Deletes a specified cookie by setting its value to empty and expiration to -1 days
    /// </summary>
    public static void DeleteCookie(string key, string domain)
    {
      SetCookie(key, "", domain, "/", -1);
    }

    /// <summary>
    /// Gets the specified browser cookie.
    /// </summary>
    public static string GetCookie(string key)
    {
      if (!HtmlPage.IsEnabled)
        return null;

      string[] cookies = HtmlPage.Document.Cookies.Split(';');
      foreach (string cookie in cookies)
      {
        string[] tokens = cookie.Trim().Split('=');
        if (tokens.Length == 2 && tokens[0] == key)
          return tokens[1];
      }

      return null;
    }

    // Utility method to insert an iframe into the HTML DOM
    private static HtmlElement insertHiddenIFrame(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        // Check whether the element already exists
        var htmlObj = HtmlPage.Document.GetElementById(id);
        if (htmlObj != null)
            return htmlObj;

        // Construct JavaScript to insert the element
        var insertIFrameJS = @"
                iframe = document.createElement('iframe');
                iframe.id = '{0}';
                iframe.style.display = 'none';
                document.body.appendChild(iframe);";
        insertIFrameJS = string.Format(insertIFrameJS, id);

        // Use the eval method to dynamically invoke the insertion logic
        HtmlPage.Window.Invoke("eval", insertIFrameJS);

        // Retrieve and return the newly inserted iframe
        return HtmlPage.Document.GetElementById(id);
    }
  }

  public class MultiPartPostEventArgs : EventArgs
  {
    public Stream Result { get; set; }
  }

  public class OpenReadEventArgs : EventArgs
  {
    public bool Cancelled { get; set; }
    public Exception Error { get; set; }
    public object UserState { get; set; }
    public Stream Result { get; set; }
    public bool UsedProxy { get; set; }

    public OpenReadEventArgs(ArcGISWebClient.OpenReadCompletedEventArgs e)
    {
        if (e != null)
        {
            Cancelled = e.Cancelled;
            Error = e.Error;
            UserState = e.UserState;
            if (e.Error == null)
                Result = e.Result;
        }
    }
  }

  /// <summary>
  /// Describes a single part in a multi-part Http POST request.
  /// </summary>
  public class HttpContentItem
  {
    /// <summary>
    /// The name of the part.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The name of the file. Should be set in conjunction with a content-type and a 
    /// Stream for the Value.
    /// </summary>
    public string Filename { get; set; }

    /// <summary>
    /// Valid when the Filename is set.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// The value of the part. Simple text string, or Stream for files.
    /// </summary>
    public object Value { get; set; }
  }


  /// <summary>
  /// Helper class to serialize a collection of HttpContentItems for a multi-part Http POST.
  /// </summary>
  internal class DataContractMultiPartSerializer
  {
    private string boundary;
    public DataContractMultiPartSerializer(string boundary)
    {
      this.boundary = boundary;
    }

    /// <summary>
    /// Writes an individual part to the stream.
    /// </summary>
    private void WriteContent(StreamWriter writer, HttpContentItem content)
    {
      writer.Write("--");
      writer.WriteLine(boundary);
      if (content.Filename != null)
      {
        writer.WriteLine(@"Content-Disposition: form-data; name=""{0}""; filename=""{1}""", content.Name, content.Filename);
        writer.WriteLine("Content-Type: " + content.Type); // eg. "image/x-png"

        Stream input = content.Value as Stream;
        input.Seek(0, SeekOrigin.Begin);
        writer.WriteLine("Content-Length: " + input.Length);
        writer.WriteLine();
        writer.Flush();
        Stream output = writer.BaseStream;
        byte[] buffer = new byte[4096];
        for (int size = input.Read(buffer, 0, buffer.Length); size > 0; size = input.Read(buffer, 0, buffer.Length))
          output.Write(buffer, 0, size);

        output.Flush();
        writer.WriteLine();
      }
      else
      {
        writer.WriteLine(@"Content-Disposition: form-data; name=""{0}""", content.Name);
        writer.WriteLine();
        writer.WriteLine(content.Value == null ? "" : content.Value.ToString());
      }
    }

    /// <summary>
    /// Writes a collection of parts to the stream.
    /// </summary>
    public void WriteContent(Stream stream, IEnumerable<HttpContentItem> contentItems)
    {
      StreamWriter writer = new StreamWriter(stream);
      writer.NewLine = "\r\n"; // set this explicitly so it's the same on the Mac

      foreach (HttpContentItem item in contentItems)
        WriteContent(writer, item);

      writer.Write("--");
      writer.Write(boundary);
      writer.WriteLine("--");
      writer.Flush();
    }
  }

  internal class RequestInfo
  {
      internal RequestInfo(Uri uri, object userState, EventHandler<OpenReadEventArgs> callback, bool forceBrowserAuth)
      {
          Uri = uri;
          UserState = userState;
          Callback = callback;
          ForceBrowserAuth = forceBrowserAuth;
      }

      internal Uri Uri { get; private set; }

      internal object UserState { get; private set; }

      internal EventHandler<OpenReadEventArgs> Callback { get; private set; }

      internal bool ForceBrowserAuth { get; private set; }
  }
}
