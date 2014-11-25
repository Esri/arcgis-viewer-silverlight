/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;
using Microsoft.Win32;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [Export(typeof(ICommand))]
    [DisplayName("ExportSelectionDisplayName")]
	[Category("CategorySelection")]
	[Description("ExportSelectionDescription")]
    public class ExportSelectionCommand : LayerCommandBase
    {
        private static FieldInfo getField(string key, Collection<FieldInfo> fields)
        {            
            if (fields != null)
            {
                FieldInfo field = fields.FirstOrDefault<FieldInfo>(f => f.Name == key);
                if (field != null)
                    return field;
            }

            return null;
        }

        #region ICommand Members

        public override bool CanExecute(object parameter)
        {
            if (Layer == null)
                return false;

            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return false;

            return graphicsLayer.SelectedGraphics.Count() > 0;
        }

        public override void Execute(object parameter)
        {
            if (Layer == null)
                return;

            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            try
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.DefaultExt = ".csv";
                dialog.Filter = "CSV Files|*.csv|Text Files|*.txt|All Files|*.*";

                if (dialog.ShowDialog() == true)
                {
                    using (Stream fs = (Stream)dialog.OpenFile())
                    {
                        using (var writer = new StreamWriter(fs, Encoding.UTF8))
                        {
                            StringBuilder sb = new StringBuilder();

                            bool wroteHeader = false;
                            Collection<FieldInfo> fields = Core.LayerExtensions.GetFields(graphicsLayer);
                            if (fields == null)
                                return;
                            foreach (Graphic record in graphicsLayer.SelectedGraphics)
                            {
                                if (!wroteHeader)
                                {
                                    // Header Row
                                    foreach (string key in record.Attributes.Keys)
                                    {
                                        FieldInfo field = getField(key, fields);
                                        if (field == null || !field.VisibleInAttributeDisplay)
                                            continue;
                                        if (sb.Length > 0)
                                            sb.Append(",");
                                        sb.Append(field.DisplayName);
                                    }
                                    sb.AppendLine();
                                    wroteHeader = true;
                                }

                                StringBuilder line = new StringBuilder();
                                foreach (KeyValuePair<string, object> display in record.Attributes)
                                {
                                    object o = display.Value;
                                    string key = display.Key;
                                    FieldInfo field = getField(key, fields);
                                    if (field == null || !field.VisibleInAttributeDisplay)
                                        continue;
                                    if (o == null)
                                    {
                                        line.Append(",");
                                        continue;
                                    }
                                    string val = Convert.ToString(o);
                                    if (val == null)
                                        val = string.Empty;

                                    // If the value contains commas, enclose it in quotes
                                    if (val.Contains(","))
                                        val = string.Format("\"{0}\"", val.Trim('"'));
                                    line.AppendFormat("{0},", val);
                                }
                                string s = line.ToString();
                                if (s.Length > 0)
                                    s = s.Substring(0, s.Length - 1); // remove trailing ,
                                if (!string.IsNullOrEmpty(s))
                                    sb.AppendLine(s);
                            }
                            writer.Write(sb.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                MessageBoxDialog.Show(Resources.Strings.MsgErrorSavingFile
#if DEBUG
 + ": " + err
#endif
); ;
            }
        }

        #endregion
    }
}
