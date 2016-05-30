/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Controls;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Resources;
using System.Xml.Linq;
using System.Reflection;
using System.Xml;
using ESRI.ArcGIS.Mapping.Builder.ApplicationBuilder;
using ESRI.ArcGIS.Mapping.Core;
using ESRI.ArcGIS.Mapping.Builder.Common;

namespace ESRI.ArcGIS.Mapping.Builder
{
    public partial class ExtensionsManager : UserControl
    {
        public ExtensionsManager()
        {
            InitializeComponent();
            ExtensionsListBox.ItemsSource = BuilderApplication.Instance.AllExtensions;
            DataContext = this;
            ExtensionsListVisiblity = BuilderApplication.Instance.AllExtensions.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility ExtensionsListVisiblity
        {
            get { return (Visibility)GetValue(ExtensionsListVisiblityProperty); }
            set { SetValue(ExtensionsListVisiblityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExtensionsListVisiblity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExtensionsListVisiblityProperty =
            DependencyProperty.Register("ExtensionsListVisibility", typeof(Visibility), typeof(ExtensionsManager), new PropertyMetadata(Visibility.Collapsed));
        
        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "XAP Files|*.xap";
            dialog.Multiselect = false;

            if (dialog.ShowDialog() == true && dialog.File != null)
            {
                FileStream fileStream = dialog.File.OpenRead();
                string selectedFileName = dialog.File.Name;
                if (selectedFileName.EndsWith(".xap", StringComparison.OrdinalIgnoreCase))
                    selectedFileName = selectedFileName.Substring(0, selectedFileName.Length - 4);
                if (BuilderApplication.Instance.AllExtensions.FirstOrDefault<Extension>(ex => string.Compare(ex.Name, selectedFileName, StringComparison.InvariantCultureIgnoreCase) == 0) != null)
                {
					MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ExtensionWithSameNameAlreadyExist, ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ExtensionAlreadyExists, MessageBoxButton.OKCancel, (o, args) =>
                    {
                        if (args.Result == MessageBoxResult.OK)
                        {
                            validateXap(fileStream, onValidationComplete, dialog);
                        }
                        else
                        {
                            fileStream.Close();
                        }
                    });
                }
                else
                {
                    validateXap(fileStream, onValidationComplete, dialog);
                }              
            }
        }        

        private void validateXap(FileStream fileStream, EventHandler<ValidationCompleteEventArgs> onComplete, object userState)
        {
            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();
            ObservableCollection<string> assemblyNames = null;
            List<System.Reflection.Assembly> extensionAssemblies = new List<System.Reflection.Assembly>();
            if (fileStream != null)
            {
                StreamResourceInfo xapStreamInfo = new StreamResourceInfo(fileStream, null);                
                IEnumerable<AssemblyPart> deploymentParts = getAssemblyParts(xapStreamInfo, out assemblyNames);                
                foreach (AssemblyPart part in deploymentParts)
                {
                    StreamResourceInfo resourceStream = Application.GetResourceStream(xapStreamInfo, new Uri(part.Source, UriKind.Relative));
                    if (resourceStream == null)
                        continue;
                    try
                    {
                        System.Reflection.Assembly assembly = part.Load(resourceStream.Stream);
                        string assemblyName = assembly.FullName.Split(',')[0];
                        if (AssemblyManager.IsBuiltInAssembly(assemblyName))
                        {
                            warnings.Add(string.Format(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.AssemblyAlreadyIncludeInTheApplication, assemblyName, System.Environment.NewLine));
                        }
                        extensionAssemblies.Add(assembly);
                    }
                    catch (Exception ex)
                    {
                        ReflectionTypeLoadException refEx = ex as ReflectionTypeLoadException;
                        if (refEx != null && refEx.LoaderExceptions != null)
                        {
                            foreach (Exception e in refEx.LoaderExceptions)
                                if (!string.IsNullOrWhiteSpace(e.Message) && !errors.Contains(e.Message))
                                    errors.Add(e.Message);
                        }
                        else if (!errors.Contains(ex.Message))
                            errors.Add(ex.Message);
                    }
                }
                fileStream.Close();
            }

            if (errors.Count == 0) //only if there have been no errors loading the assemblies attempt to add to the catalog
            {
                try
                {
                    foreach (System.Reflection.Assembly assem in extensionAssemblies)
                        AssemblyManager.AddAssembly(assem);
                }
                catch (Exception ex)
                {
                    if (!errors.Contains(ex.Message))
                        errors.Add(ex.Message);
                }
            }

            if (errors.Count < 1 && warnings.Count < 1)
            {
                // No errors or warnings
                if (onComplete != null)
                    onComplete(this, new ValidationCompleteEventArgs() { UserState = userState, AssemblyNames = assemblyNames });
            }
            else
            {
                ExtensionUploadWarningDialog warningDialog = new ExtensionUploadWarningDialog(errors, warnings) { Tag = new object[] { userState, assemblyNames } };
                warningDialog.OkClicked += new EventHandler(warningDialog_OkClicked);
                warningDialog.CancelClicked += new EventHandler(warningDialog_CancelClicked);
                BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.Errors, warningDialog, true);
            }
        }

        void warningDialog_CancelClicked(object sender, EventArgs e)
        {
            // User cancelled the dialog
            BuilderApplication.Instance.HideWindow(sender as FrameworkElement);
        }

        void warningDialog_OkClicked(object sender, EventArgs e)
        {
            // User ignored the warning and wants to continue uploading the .xap file

            // close the window
            BuilderApplication.Instance.HideWindow(sender as FrameworkElement);

            object [] tag = (sender as FrameworkElement).Tag as object[];
            if (tag == null || tag.Length < 2)
                return;            
            
            // upload file
            uploadFile(tag[0] as OpenFileDialog, tag[1] as ObservableCollection<string>);
        }

        void onValidationComplete(object sender, ValidationCompleteEventArgs args)
        {
            OpenFileDialog dialog = args.UserState as OpenFileDialog;
            if (dialog == null)
                return;

            uploadFile(dialog, args.AssemblyNames);
        }        

        private void uploadFile(OpenFileDialog dialog, IEnumerable<string> assemblyNames)
        {
            string selectedFileName;
            byte[] fileBuffer = null;            
            try
            {
                using (FileStream strm = dialog.File.OpenRead())
                {
                    selectedFileName = dialog.File.Name;
                    // remove the .xap extension
                    if (selectedFileName.EndsWith(".xap", StringComparison.OrdinalIgnoreCase))
                        selectedFileName = selectedFileName.Substring(0, selectedFileName.Length - 4);
                    using (BinaryReader rdr = new BinaryReader(strm))
                    {
                        fileBuffer = rdr.ReadBytes((int)strm.Length);
                    }
                }                

                if (fileBuffer != null)
                {
                    Extension extension = new Extension()
                    {
                        Name = selectedFileName,
                        Url = string.Format("{0}/{1}.xap", BuilderApplication.Instance.ExtensionsRepositoryBaseUrl, selectedFileName),
                        Assemblies = new List<ESRI.ArcGIS.Mapping.Builder.Common.Assembly>(),
                    };
                    ObservableCollection<string> assemblies = new ObservableCollection<string>();
                    if (assemblyNames != null)
                    {
                        foreach (string assemblyInExtension in assemblyNames)
                        {
                            if (!AssemblyManager.IsBuiltInAssembly(assemblyInExtension))
                            {
                                assemblies.Add(assemblyInExtension);
                                extension.Assemblies.Add(new ESRI.ArcGIS.Mapping.Builder.Common.Assembly() { Name = assemblyInExtension });
                            }
                        }
                    }
                    showHideProgressIndicator(false);
                    byte[] msgBody = fileBuffer.ToArray();
                    ApplicationBuilder.ApplicationBuilderClient client = WCFProxyFactory.CreateApplicationBuilderProxy();
                    client.UploadExtensionLibraryCompleted += client_UploadExtensionLibraryCompleted;
                    client.UploadExtensionLibraryAsync(selectedFileName, msgBody, assemblies, extension);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError(ex);
            }
        }

        private static IEnumerable<AssemblyPart> getAssemblyParts(StreamResourceInfo xapStreamInfo, out ObservableCollection<string> assemblyNames)
        {
            assemblyNames = new ObservableCollection<string>();
            Uri uri = new Uri("AppManifest.xaml", UriKind.Relative);
            StreamResourceInfo resourceStream = Application.GetResourceStream(xapStreamInfo, uri);
            List<AssemblyPart> list = new List<AssemblyPart>();
            if (resourceStream != null)
            {
                using (XmlReader reader = XmlReader.Create(resourceStream.Stream))
                {
                    if (!reader.ReadToFollowing("AssemblyPart"))
                    {
                        return list;
                    }
                    do
                    {
                        string attribute = reader.GetAttribute("Source");
                        if (attribute != null)
                        {
                            AssemblyPart item = new AssemblyPart();                            
                            item.Source = attribute;
                            list.Add(item);
                            assemblyNames.Add(attribute.Replace(".dll", ""));
                        }
                    }
                    while (reader.ReadToNextSibling("AssemblyPart"));
                }
            }
            return list;
        }

        void showHideProgressIndicator(bool hide)
        {
            if (hide)
                ProgressIndicator.StopProgressAnimation();
            ProgressIndicator.Visibility = hide ? Visibility.Collapsed : Visibility.Visible;
            if (!hide)
                ProgressIndicator.StartProgressAnimation();
        }        

        void client_UploadExtensionLibraryCompleted(object sender, ApplicationBuilder.UploadExtensionLibraryCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;

            if (e.Error != null)
                return;

            Extension extension = e.UserState as Extension;
            if (extension == null)
                return;

            m_extension = extension;
            ESRI.ArcGIS.Mapping.Core.ExtensionsManager.LoadAdditionalExtension(extension.Url, extensionsProvider_LoadAdditionalExtensionCompleted, extensionsProvider_LoadAdditionalExtensionCompleted, extension);
        }

        private Extension m_extension;
        void extensionsProvider_LoadAdditionalExtensionCompleted(object sender, EventArgs e)
        {
            showHideProgressIndicator(true);
            Extension extension = BuilderApplication.Instance.AllExtensions.FirstOrDefault<Extension>(ex => string.Compare(ex.Name, m_extension.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (extension != null) // extension already exists ... only update the assemblies
            {
                extension.Assemblies = m_extension.Assemblies;
            }
            else
            {
                // Add it as new
                BuilderApplication.Instance.AllExtensions.Add(m_extension);
                ExtensionsListVisiblity = BuilderApplication.Instance.AllExtensions.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            OnExtensionsCatalogChanged(EventArgs.Empty);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement elem = sender as FrameworkElement;
            if (elem == null)
                return;

            Extension extension = elem.DataContext as Extension;
            if ( extension == null || string.IsNullOrWhiteSpace(extension.Name))
                return;

			MessageBoxDialog.Show(string.Format(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.DeletingExtensionWillMakeItUnavailableForFurtherUse, System.Environment.NewLine), ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ConfirmDelete, MessageBoxButton.OKCancel, (s, args) =>
            {
                if (args.Result == MessageBoxResult.OK) {
                    ApplicationBuilder.ApplicationBuilderClient client = WCFProxyFactory.CreateApplicationBuilderProxy();
                    client.DeleteExtensionLibraryCompleted += new EventHandler<ApplicationBuilder.DeleteExtensionLibraryCompletedEventArgs>(client_DeleteExtensionLibraryCompleted);
                    client.DeleteExtensionLibraryAsync(extension.Name, extension);
                }
            });            
        }

        void client_DeleteExtensionLibraryCompleted(object sender, ApplicationBuilder.DeleteExtensionLibraryCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;
            if(e.Error != null)
                return;

            Extension extension = e.UserState as Extension;
            if(extension != null)
                BuilderApplication.Instance.AllExtensions.Remove(extension);

            // Check if the assembly is not part of any remaining extension 
            // If not present, then delete it from the AssemblyManager's cache
            if (extension.Assemblies != null)
            {
                // Check each of the assemblies within the extension beind deleted
                foreach (ESRI.ArcGIS.Mapping.Builder.Common.Assembly assembly in extension.Assemblies)
                {
                    // Check if it exists among other extensions
                    bool exists = false;
                    foreach (Extension ex in BuilderApplication.Instance.AllExtensions)
                    {
                        if (ex.Assemblies == null)
                            continue;
                        if (ex.Assemblies.FirstOrDefault<ESRI.ArcGIS.Mapping.Builder.Common.Assembly>(a => a.Name == assembly.Name) != null)
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                    {
                        AssemblyManager.DeleteAssembly(AssemblyManager.GetAssemblyByName(assembly.Name));
                    }
                }
            }
            ExtensionsListVisiblity = BuilderApplication.Instance.AllExtensions.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            OnExtensionsCatalogChanged(EventArgs.Empty);
        }

        protected virtual void OnExtensionsCatalogChanged(EventArgs args)
        {
            if (ExtensionsCatalogChanged != null)
                ExtensionsCatalogChanged(this, args);
        }

        /// <summary>
        /// Event fired when the set of extensions in the builder repository changes. This includes new extensions added
        /// and extensions deleted
        /// </summary>
        public event EventHandler ExtensionsCatalogChanged;

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            Extension extension = (sender as FrameworkElement).DataContext as Extension;
            ExtensionDetailsControl detailsControl = new ExtensionDetailsControl(extension);
            detailsControl.CloseClicked += new EventHandler(detailsControl_OkClicked);
			BuilderApplication.Instance.ShowWindow(string.Format(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ExtensionDetails, extension.Name), detailsControl, true);
        }

        void detailsControl_OkClicked(object sender, EventArgs e)
        {
            BuilderApplication.Instance.HideWindow(sender as FrameworkElement);
        }            
    }

    public class ValidationCompleteEventArgs : EventArgs
    {
        public ObservableCollection<string> AssemblyNames { get; set; }
        public object UserState { get; set; }
    }
}
