/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public static class IOHelper
    {
        public static void CopyDirectoryContents(string sourceDir, string destDir, bool recursiveCopy)
        {
            CopyDirectoryContents(sourceDir, destDir, recursiveCopy, false);
        }

        public static void CopyDirectoryContents(string sourceDir, string destDir, bool recursiveCopy, bool overwriteFiles)
        {
            string[] files = Directory.GetFiles(sourceDir);

            foreach (string file in files)
            {
                int indexOfLastSlash = file.LastIndexOf('\\');
                string fileNameToCopy = file.Substring(indexOfLastSlash + 1);

                if (overwriteFiles && File.Exists(Path.Combine(destDir, fileNameToCopy)))
                    File.Delete(Path.Combine(destDir, fileNameToCopy));

                File.Copy(file, Path.Combine(destDir, fileNameToCopy));
            }

            if (recursiveCopy)
            {
                string[] subDirectories = Directory.GetDirectories(sourceDir);
                if (subDirectories.Length > 0)
                {
                    foreach (string subDir in subDirectories)
                    {
                        int indexOfLastSlash = subDir.LastIndexOf('\\');
                        string subdirectoryName = subDir.Substring(indexOfLastSlash + 1) + "\\";

                        Directory.CreateDirectory(destDir + subdirectoryName);

                        CopyDirectoryContents(subDir, destDir + subdirectoryName, true);
                    }
                }
            }
        }
    }
}
