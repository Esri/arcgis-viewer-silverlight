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
using System.IO;
using System.Text;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class FileOpenCommand : DependencyObject, ICommand
    {
        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return View != null;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (View == null)
                return;

            try
            {
                OpenFileDialog dialog = new OpenFileDialog();                
                dialog.Filter = "XML Files|*.xml|Text Files|*.txt|All Files|*.*";
                dialog.Multiselect = false;

                if (dialog.ShowDialog() == true && dialog.File != null)
                {
                    using (Stream fs = (Stream)dialog.File.OpenRead())
                    {
                        byte[] buffer = new byte[10000];
                        StringBuilder sb = new StringBuilder();
                        while (true)
                        {
                            int count = fs.Read(buffer, 0, buffer.Length);
                            if (count <= 0)
                                break;
                            string s = UTF8Encoding.UTF8.GetString(buffer, 0, count);
                            sb.Append(s);
                        }

                        string xaml = sb.ToString();

                        object parsedObject = null;
                        try
                        {
                            parsedObject = System.Windows.Markup.XamlReader.Load(xaml);
                            Map map = parsedObject as Map;
                            if (map == null)
                            {
                                throw new Exception(Resources.Strings.NotValidMapDocument);
                            }
                            else
                            {
                                View.SelectedLayer = null;
                                View.Map = map;
                            }
                        }
                        catch (Exception error)
                        {
                            string err = error.Message;
							MessageBoxDialog.Show(Resources.Strings.NotValidMapDocument
#if DEBUG
 + Environment.NewLine + err
#endif
); ;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                MessageBoxDialog.Show(Resources.Strings.MsgErrorOpeningFile
#if DEBUG
 + ": " + err
#endif
); ;
            }
        }

        #endregion

        protected void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        #region View
        /// <summary>
        /// 
        /// </summary>
        public View View
        {
            get { return GetValue(ViewProperty) as View; }
            set { SetValue(ViewProperty, value); }
        }

        /// <summary>
        /// Identifies the View dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.Register(
                "View",
                typeof(View),
                typeof(FileOpenCommand),
                new PropertyMetadata(null, OnViewPropertyChanged));

        /// <summary>
        /// ViewProperty property changed handler.
        /// </summary>
        /// <param name="d">FileOpenCommand that changed its View.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FileOpenCommand source = d as FileOpenCommand;
            source.OnViewChanged();
        }
        #endregion 

        private void OnViewChanged()
        {
            OnCanExecuteChanged();
        }
    }
}
