<%@ Page Language="C#" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" >

<head>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8">
    <title>_ArcGISApplicationBuilderForMicrosoftSilverlight_</title>
    <style type="text/css">
    html, body {
	    height: 100%;
	    overflow: auto;
    }
    body {
	    padding: 0;
	    margin: 0;
    }
    #silverlightControlHost {
	    height: 100%;
	    text-align:center;
    }
    </style>
    <script type="text/javascript" src="Silverlight.js"></script>
    <script type="text/javascript">
        function onSilverlightError(sender, args) {
            var appSource = "";
            if (sender != null && sender != 0) {
              appSource = sender.getHost().Source;
            }
            
            var errorType = args.ErrorType;
            var iErrorCode = args.ErrorCode;

            if (errorType == "ImageError" || errorType == "MediaError") {
              return;
            }

             var errMsg = "_ErrorUnHandledErrorInSilverlightApplcation_ " + appSource + "\n";

             errMsg += "_CodeColon_ " + iErrorCode + "    \n";
             errMsg += "_CategoryColon_ " + errorType + "       \n";
             errMsg += "_MessageColon_ " + args.ErrorMessage + "     \n";

            if (errorType == "ParserError") {
            	errMsg += "_FileColon_ " + args.xamlFile + "     \n";
            	errMsg += "_LineColon_ " + args.lineNumber + "     \n";
            	errMsg += "_PositionColon_ " + args.charPosition + "     \n";
            }
            else if (errorType == "RuntimeError") {           
                if (args.lineNumber != 0) {
                	errMsg += "_LineColon_ " + args.lineNumber + "     \n";
                	errMsg += "_PositionColon_ " + args.charPosition + "     \n";
                }
                errMsg += "_MethodNameColon_ " + args.methodName + "     \n";
            }

            throw new Error(errMsg);
        }
        function onSourceDownloadProgressChanged(sender, eventArgs) {
            sender.findName("uxStatusText").Text = "_LoadingColon_";
            sender.findName("uxStatusPercent").Text = Math.round((eventArgs.progress * 1000) / 10) + "%";
            sender.findName("uxProgressBar").ScaleX = eventArgs.progress;
        }

        function onload() {
            // Inject plug-in HTML dynamically to work around issue with Silverlight app occasionally not initializing

            var pluginHtml = "<object data=\"data:application/x-silverlight-2,\" type=\"application/x-silverlight-2\" width=\"100%\" height=\"100%\">" +

            // Dynamically construct the path to the XAP file, appending the time the XAP was created as a query string.
            // This will force the XAP to be downloaded if it's been updated.
            <%                
                string strSourceFile = @"ESRI.ArcGIS.Mapping.Builder.xap";
                string xappath = HttpContext.Current.Server.MapPath(@"") + @"\" + strSourceFile;
                DateTime xapCreationDate = System.IO.File.GetLastWriteTime(xappath);
                string param = "\"<param name=\\\"source\\\" value=\\\"" + strSourceFile + "?v="
                        + xapCreationDate.Ticks + "\\\" />\" +";
                Response.Write(param);
            %>
            "<param name=\"onError\" value=\"onSilverlightError\" />" +
            "<param name=\"background\" value=\"#ECEEF1\" />" +
            "<param name=\"minRuntimeVersion\" value=\"4.0.50826.0\" />" +
            "<param name=\"autoUpgrade\" value=\"true\" />" +
            "<param name=\"splashscreensource\" value=\"SplashScreen.xaml\" />" +
            "<param name=\"culture\" value=\"en-US\" />" +
            "<param name=\"uiculture\" value=\"en-US\" />" +
            "<param name=\"onSourceDownloadProgressChanged\" value=\"onSourceDownloadProgressChanged\" />" +
            "<a href=\"http://go.microsoft.com/fwlink/?LinkID=149156&v=4.0.50826.0\" style=\"text-decoration: none\">" +
                "<img src=\"http://go.microsoft.com/fwlink/?LinkId=161376\" alt=\"Get Microsoft Silverlight\" style=\"border-style: none\" />" +
            "</a>" +
            "</object>" +
            "<iframe id=\"_sl_historyFrame\" style=\"visibility: hidden; height: 0px; width: 0px;border: 0px\"></iframe>";

            var host = document.getElementById("silverlightControlHost");
            host.innerHTML = pluginHtml;
        }
    </script>
</head>
<body onload="onload()">
    <form id="form1" runat="server" style="height: 100%">
    <div id="silverlightControlHost">
    </div>
    </form>
</body>
</html>
