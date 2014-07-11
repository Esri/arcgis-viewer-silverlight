/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using ESRI.ArcGIS.Mapping.Builder.Common;

namespace ESRI.ArcGIS.Mapping.Builder.Web
{
    public static class FileExplorerUtility
    {
        #region Network API
        // Possible types of servers
        [FlagsAttribute]
        public enum ServerType : uint
        {
            /// <summary>
            /// All workstations
            /// </summary>
            SV_TYPE_WORKSTATION = 0x00000001,
            /// <summary>
            /// All computers that have the server service running
            /// </summary>
            SV_TYPE_SERVER = 0x00000002,
            /// <summary>
            /// Any server running Microsoft SQL Server
            /// </summary>
            SV_TYPE_SQLSERVER = 0x00000004,
            /// <summary>
            /// Primary domain controller
            /// </summary>
            SV_TYPE_DOMAIN_CTRL = 0x00000008,
            /// <summary>
            /// Backup domain controller
            /// </summary>
            SV_TYPE_DOMAIN_BAKCTRL = 0x00000010,
            /// <summary>
            /// Server running the Timesource service
            /// </summary>
            SV_TYPE_TIME_SOURCE = 0x00000020,
            /// <summary>
            /// Apple File Protocol servers
            /// </summary>
            SV_TYPE_AFP = 0x00000040,
            /// <summary>
            /// Novell servers
            /// </summary>
            SV_TYPE_NOVELL = 0x00000080,
            /// <summary>
            /// LAN Manager 2.x domain member
            /// </summary>
            SV_TYPE_DOMAIN_MEMBER = 0x00000100,
            /// <summary>
            /// Server sharing print queue
            /// </summary>
            SV_TYPE_PRINTQ_SERVER = 0x00000200,
            /// <summary>
            /// Server running dial-in service
            /// </summary>
            SV_TYPE_DIALIN_SERVER = 0x00000400,
            /// <summary>
            /// Xenix server
            /// </summary>
            SV_TYPE_XENIX_SERVER = 0x00000800,
            /// <summary>
            /// Windows NT workstation or server
            /// </summary>
            SV_TYPE_NT = 0x00001000,
            /// <summary>
            /// Server running Windows for Workgroups
            /// </summary>
            SV_TYPE_WFW = 0x00002000,
            /// <summary>
            /// Microsoft File and Print for NetWare
            /// </summary>
            SV_TYPE_SERVER_MFPN = 0x00004000,
            /// <summary>
            /// Server that is not a domain controller
            /// </summary>
            SV_TYPE_SERVER_NT = 0x00008000,
            /// <summary>
            /// Server that can run the browser service
            /// </summary>
            SV_TYPE_POTENTIAL_BROWSER = 0x00010000,
            /// <summary>
            /// Server running a browser service as backup
            /// </summary>
            SV_TYPE_BACKUP_BROWSER = 0x00020000,
            /// <summary>
            /// Server running the master browser service
            /// </summary>
            SV_TYPE_MASTER_BROWSER = 0x00040000,
            /// <summary>
            /// Server running the domain master browser
            /// </summary>
            SV_TYPE_DOMAIN_MASTER = 0x00080000,
            /// <summary>
            /// Windows 95 or later
            /// </summary>
            SV_TYPE_WINDOWS = 0x00400000,
            /// <summary>
            /// Root of a DFS tree
            /// </summary>
            SV_TYPE_DFS = 0x00800000,
            /// <summary>
            /// Terminal Server
            /// </summary>
            SV_TYPE_TERMINALSERVER = 0x02000000,
            /// <summary>
            /// Server clusters available in the domain
            /// </summary>
            SV_TYPE_CLUSTER_NT = 0x01000000,
            /// <summary>
            /// Cluster virtual servers available in the domain
            /// (Not supported for Windows 2000/NT)
            /// </summary>			
            SV_TYPE_CLUSTER_VS_NT = 0x04000000,
            /// <summary>
            /// IBM DSS (Directory and Security Services) or equivalent
            /// </summary>
            SV_TYPE_DCE = 0x10000000,
            /// <summary>
            /// Return list for alternate transport
            /// </summary>
            SV_TYPE_ALTERNATE_XPORT = 0x20000000,
            /// <summary>
            /// Return local list only
            /// </summary>
            SV_TYPE_LOCAL_LIST_ONLY = 0x40000000,
            /// <summary>
            /// Lists available domains
            /// </summary>
            SV_TYPE_DOMAIN_ENUM = 0x80000000
        }

        // enumerates network computers
        [DllImport("Netapi32", CharSet = CharSet.Unicode)]
        private static extern int NetServerEnum(
            string servername,        // must be null
            int level,        // 100 or 101
            out IntPtr bufptr,        // pointer to buffer receiving data
            int prefmaxlen,        // max length of returned data
            out int entriesread,    // num entries read
            out int totalentries,    // total servers + workstations
            uint servertype,        // server type filter
            [MarshalAs(UnmanagedType.LPWStr)] string domain,        // domain to enumerate
            IntPtr resume_handle);


        // Holds computer information
        [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SERVER_INFO_101
        {
            public int sv101_platform_id;
            public string sv101_name;
            public int sv101_version_major;
            public int sv101_version_minor;
            public int sv101_type;
            public string sv101_comment;
        }

        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetShareEnum(
             [MarshalAs(UnmanagedType.LPWStr)]string ServerName,
             int level,
             out IntPtr bufPtr,
             uint prefmaxlen,
             out int entriesread,
             out int totalentries,
             IntPtr resume_handle
             );

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct SHARE_INFO_0
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string shi0_netname;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct SHARE_INFO_1
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string shi1_netname;
            [MarshalAs(UnmanagedType.U4)]
            public uint shi1_type;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string shi1_remark;
        }

        const uint MAX_PREFERRED_LENGTH = 0xFFFFFFFF;
        const int NERR_Success = 0;
        const uint STYPE_DISKTREE = 0;
        const uint STYPE_PRINTQ = 1;
        const uint STYPE_DEVICE = 2;
        const uint STYPE_IPC = 3;
        const uint STYPE_DFS = 100;
        const uint STYPE_SPECIAL = 0x80000000;


        // Frees buffer created by NetServerEnum
        [DllImport("netapi32.dll")]
        private extern static int NetApiBufferFree(
            IntPtr buf);

        // Constants
        private const int ERROR_ACCESS_DENIED = 5;
        private const int ERROR_MORE_DATA = 234;
        #endregion

        #region Public Methods
        public static string[] GetShares(string server)
        {
            //Add some code to make this configurable
            //return GetSharesUsingWMI(server);
            return GetSharesUsingNetAPI(server);
        }

        public static string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        public static string[] GetFiles(string path, string[] fileExts)
        {
            ArrayList fileList = new ArrayList();
            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                string ext = fileInfo.Extension.ToLower();
                bool match = false;
                if (fileExts == null)
                    match = true;
                else
                {
                    match = IsInExtensions(fileExts, ext);
                }
                if (match)
                    fileList.Add(fileInfo.FullName);
            }
            string[] files2 = new string[fileList.Count];
            fileList.CopyTo(files2);
            return files2;
        }        

        /// <summary>
        /// Get the name of the domain that the server belongs
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDomainName()
        {
            return System.Environment.UserDomainName;            
        }

        public static string[] GetServers(string domain)
        {
            int entriesread;  // number of entries actually read
            int totalentries; // total visible servers and workstations
            int result;		  // result of the call to NetServerEnum
            string[] servers = null;

            // Pointer to buffer that receives the data
            IntPtr pBuf = IntPtr.Zero;
            Type svType = typeof(SERVER_INFO_101);

            // structure containing info about the server
            SERVER_INFO_101 si;

            try
            {
                result = NetServerEnum(
                    null,
                    101,
                    out pBuf,
                    -1,
                    out entriesread,
                    out totalentries,
                    (int)ServerType.SV_TYPE_SERVER,
                    domain,
                    IntPtr.Zero);

                // Successful?
                if (result != 0)
                {
                    string errorMsg;
                    switch (result)
                    {
                        case ERROR_MORE_DATA:
                            errorMsg = "More data is available";
                            break;
                        case ERROR_ACCESS_DENIED:
                            errorMsg = "Access was denied";
                            break;
                        default:
                            errorMsg = "Unknown error code " + result;
                            break;
                    }
                    throw new Exception(errorMsg);
                }
                else
                {
                    servers = new string[entriesread];
                    int tmp = (int)pBuf;
                    for (int i = 0; i < entriesread; i++)
                    {
                        // fill our struct
                        si = (SERVER_INFO_101)Marshal.PtrToStructure((IntPtr)tmp, svType);
                        servers[i] = si.sv101_name;

                        // next struct
                        tmp += Marshal.SizeOf(svType);
                    }
                    return servers;
                }
            }
            finally
            {
                // free the buffer
                NetApiBufferFree(pBuf);
                pBuf = IntPtr.Zero;
            }
        }

        public static void UploadFile(string filePath, byte[] bytes, out FaultContract Fault)
        {
            Fault = null;
            if (string.IsNullOrEmpty(filePath))
            {
                Fault = new FaultContract
                {
                    FaultType = "Error",
                    Message = "Must enter a valid file path"
                };
                return;
            }
            try
            {
                if (bytes == null || bytes.Length > 0)
                {
                    using (System.IO.FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Close();
                    }
                }
                else
                {
                    Fault = new FaultContract { FaultType = "Warning", Message = "Empty bytes for file" };
                }
            }
            catch (Exception ex)
            {
                Fault = new FaultContract { FaultType = "Error", Message = ex.Message };
            }
        }


        public static bool UploadTextFileContents(string filePhysicalPath, string fileContents, out FaultContract Fault)
        {
            Fault = null;
            if (!File.Exists(filePhysicalPath))
            {
                Fault = new FaultContract { FaultType = "Error", Message = "File doesn't exist" + filePhysicalPath };
                return false;
            }
            try
            {
                using (FileStream stream = new FileStream(filePhysicalPath, FileMode.Truncate))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(fileContents);
                        writer.Flush();
                        writer.Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Fault = new FaultContract { FaultType = "Error", Message = "Error writing file:- " + ex.Message };
            }
            return true;
        }
        #endregion

        #region Private Methods        
        private static bool IsInExtensions(string[] fileExts, string ext)
        {
            bool match = false;

            foreach (string fileExt in fileExts)
            {
                if (ext.Equals(fileExt))
                {
                    match = true;
                    break;
                }
            }
            return match;
        }
        
        private static string[] GetSharesUsingNetAPI(string server)
        {
            List<string> shareList = new List<String>();
            int entriesread = 0;
            int totalentries = 0;
            IntPtr resume_handle = IntPtr.Zero;
            int nStructSize = Marshal.SizeOf(typeof(SHARE_INFO_1));
            IntPtr bufPtr = IntPtr.Zero;
            int ret = NetShareEnum(server, 1, out bufPtr, MAX_PREFERRED_LENGTH, out entriesread, out totalentries, resume_handle);
            if (ret == NERR_Success)
            {
                int currentPtr = (int)bufPtr;
                for (int i = 0; i < entriesread; i++)
                {
                    SHARE_INFO_1 shi1 = (SHARE_INFO_1)Marshal.PtrToStructure((IntPtr)currentPtr, typeof(SHARE_INFO_1));
                    if (shi1.shi1_type == STYPE_DISKTREE)
                    {
                        shareList.Add(shi1.shi1_netname);
                    }
                    currentPtr += nStructSize;
                }
            }
            NetApiBufferFree(bufPtr);
            string[] shares = new string[shareList.Count];
            shareList.CopyTo(shares);
            return shares;
        }
        #endregion
    }
}
