/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace ESRI.ArcGIS.Mapping.Builder.Server
{
    public abstract class ServiceRequestHandlerBase : IHttpHandler, IDisposable
    {
        protected HttpRequest Request { get; private set; }
        protected HttpResponse Response { get; private set; }
        protected HttpContext Context { get; private set; }        

        public bool IsReusable
        {
            get { return true; }
        }
        
        public void ProcessRequest(HttpContext context)
        {
            Request = context.Request;
            Response = context.Response;
            Context = context;

            HandleRequest();
        }

        protected abstract void HandleRequest();

        protected void WriteError(string errorMessage)
        {            
            Response.Write(string.Format("error:{0}",errorMessage));
            Response.Flush();
        }

        public void Dispose()
        {
            Context = null;
            Response = null;
            Request = null;
        }
    }
}
