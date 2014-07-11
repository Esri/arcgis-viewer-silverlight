/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class AppCoreHelper : IApplicationServices
    {
        #region Constructor
        private AppCoreHelper()
        {
        }
        #endregion

        #region Static Properties
        private static object lockObject = new object();
        private static AppCoreHelper _helperContext;
        public static AppCoreHelper Current
        {
            get
            {
                return _helperContext;
            }
        }

        private static IApplicationServices _appServices;
        #endregion

        public static void SetService(IApplicationServices applicationServices)
        {
            lock (lockObject)
            {
                if (_helperContext == null)
                {
                    _helperContext = new AppCoreHelper();
                }

                _appServices = applicationServices;
            }
        }

        public void BrowseForFile(EventHandler<BrowseCompleteEventArgs> onComplete, string[] fileExts = null, string startupFolderRelativePath = null, object userState = null)
        {
            if (_appServices == null)
                throw new Exception("Application Services are unavailable.");

            _appServices.BrowseForFile(onComplete, fileExts, startupFolderRelativePath, userState);
        }
    }
}
