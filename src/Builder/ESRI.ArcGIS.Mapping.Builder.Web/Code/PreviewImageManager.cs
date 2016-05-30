/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public static class PreviewImageManager
    {
        private const string APP_THUMBNAILSFOLDERPATH = "~/Images/app_thumbnails";
        public static void SavePreviewImageForSite(string siteId, byte[] imageBytes)
        {
            if (imageBytes == null || string.IsNullOrEmpty(siteId))
                return;
            try
            {
                using (System.IO.Stream stream = new System.IO.FileStream(ServerUtility.MapPath(APP_THUMBNAILSFOLDERPATH + "/" + siteId + ".png"), System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    stream.Write(imageBytes, 0, imageBytes.Length);
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.ToString());
            }
        }
    }
}
