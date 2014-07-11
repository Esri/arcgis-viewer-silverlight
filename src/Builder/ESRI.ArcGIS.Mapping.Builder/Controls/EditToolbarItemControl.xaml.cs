/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Builder.Controls
{
    public partial class EditToolbarItemControl : UserControl
    {
        public EditToolbarItemControl()
        {
            InitializeComponent();

            DataContext = this;

            cboToolbar.Items.Clear();
            foreach (ToolPanel toolPanel in ToolPanels)
            {
                if (!toolPanel.CanSerialize)
                    continue; //do not include toolpanels that will not be serialized

                ComboBoxItem item = new ComboBoxItem()
                {
                    Tag = toolPanel.ContainerName,
                    Content = new TextBlock() { Text = toolPanel.Name }
                };
                cboToolbar.Items.Add(item);
            }
            if (cboToolbar.Items.Count > 0)
                cboToolbar.SelectedIndex = 0;
        }


        #region Properties

        public string ToolbarName { get; private set; }
        public object ToolInstance { get; set; }
        public Type ToolType { get; set; }
        public ToolPanel SelectedToolPanel { get; private set; }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Label.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register("Label", typeof(string), typeof(EditToolbarItemControl), new PropertyMetadata(null, OnPropertyChanged));

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(EditToolbarItemControl), new PropertyMetadata(null, OnPropertyChanged));

        public string IconUrl
        {
            get { return (string)GetValue(IconUrlProperty); }
            set { SetValue(IconUrlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconUrl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconUrlProperty =
            DependencyProperty.Register("IconUrl", typeof(string), typeof(EditToolbarItemControl), new PropertyMetadata(null, OnPropertyChanged));

        static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditToolbarItemControl o = d as EditToolbarItemControl;
            o.InputValid = o.validateInput();
        }

        public Visibility ToolbarSelectionVisibility
        {
            get { return (Visibility)GetValue(ToolbarSelectionVisibilityProperty); }
            set { SetValue(ToolbarSelectionVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ToolbarSelectionVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToolbarSelectionVisibilityProperty =
            DependencyProperty.Register("ToolbarSelectionVisibility", typeof(Visibility), typeof(EditToolbarItemControl), new PropertyMetadata(Visibility.Visible));

        public Visibility OkCancelButtonVisibility
        {
            get { return (Visibility)GetValue(OkCancelButtonVisibilityProperty); }
            set { SetValue(OkCancelButtonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OkCancelButtonVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OkCancelButtonVisibilityProperty =
            DependencyProperty.Register("OkCancelButtonVisibility", typeof(Visibility), typeof(EditToolbarItemControl),
            new PropertyMetadata(Visibility.Visible));

        public bool SupportsConfiguration
        {
            get { return (bool)GetValue(SupportsConfigurationProperty); }
            set { SetValue(SupportsConfigurationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SupportsConfiguration.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SupportsConfigurationProperty =
            DependencyProperty.Register("SupportsConfiguration", typeof(bool), typeof(EditToolbarItemControl), new PropertyMetadata(false));

        public bool InputValid
        {
            get { return (bool)GetValue(InputValidProperty); }
            set { SetValue(InputValidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InputValid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputValidProperty =
            DependencyProperty.Register("InputValid", typeof(bool), typeof(EditToolbarItemControl), new PropertyMetadata(false, OnInputValidChanged));

        static void OnInputValidChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditToolbarItemControl o = d as EditToolbarItemControl;
            o.OnValidationStateChanged();
        }

        #endregion

        #region Button click handlers - Browse, OK, Cancel, Configure

        FileBrowser browser;
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            // Create browser control if not already created.
            if (browser == null)
            {
                browser = new FileBrowser()
                {
                    FileExtensions = new System.Collections.ObjectModel.ObservableCollection<string>() { 
                        ".png",".jpg", ".jpeg"
                    },
                    StartupRelativeUrl = "Images"
                };
                browser.CancelClicked += new EventHandler(browser_CancelClicked);
                browser.UrlChosen += new EventHandler<FileChosenEventArgs>(browser_UrlChosen);
            }
            BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.BrowseForImage, browser, true);
        }

        public event EventHandler OkClicked;
        public event EventHandler CancelClicked;
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (OkClicked != null)
                OkClicked(this, EventArgs.Empty);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (CancelClicked != null)
                CancelClicked(this, EventArgs.Empty);
        }

        private void btnConfigure_Click(object sender, RoutedEventArgs e)
        {

            // Instantiate control 
            ToolInstance = ToolInstance ?? Activator.CreateInstance(ToolType);

            // Get the "supports configuration" interface in order to invoke the configure method.
            ISupportsConfiguration supportsConfiguration = ToolInstance as ISupportsConfiguration;
            if (supportsConfiguration != null)
                supportsConfiguration.Configure();
        }

        #endregion

        #region File (image) browse handling

        private void browser_UrlChosen(object sender, FileChosenEventArgs e)
        {
            // Assign the chosen path to the textbox control which in turn is bound to an object and updated
            // should the user dismiss our dialog with OK.
            txtImage.Text = e.RelativePath;

            BuilderApplication.Instance.HideWindow(browser);
        }

        private void browser_CancelClicked(object sender, EventArgs e)
        {
            BuilderApplication.Instance.HideWindow(browser);
        }

        #endregion

        #region Input Validation

        public event EventHandler<EventArgs> ValidationStateChanged;
        private void OnValidationStateChanged()
        {
            if (ValidationStateChanged != null)
                ValidationStateChanged(this, EventArgs.Empty);
        }

        private bool validateInput()
        {
            bool inputValid = true;
            // Make sure the proposed label does not contain any unacceptable characters
            if (System.Windows.Browser.HttpUtility.HtmlEncode(Label) != Label)
            {
                MessageBoxDialog.Show("Label has invalid characters like &, <, > or \".", "Invalid Label", MessageBoxButton.OK, (a, b) =>
                {
                    txtTitle.Focus();
                });
                inputValid = false;
            }

            if (string.IsNullOrWhiteSpace(Label) || string.IsNullOrWhiteSpace(IconUrl))
                inputValid = false;

            return inputValid;
        }

        #endregion

        private ToolPanels ToolPanels
        {
            get
            {
                ViewerApplicationControl instance = ViewerApplicationControl.Instance;
                if (instance == null || instance.ToolPanels == null)
                    return null;

                return instance.ToolPanels;
            }
        }

        private void cboToolbar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboToolbar.SelectedItem is ComboBoxItem)
            {
                ToolbarName = (cboToolbar.SelectedItem as ComboBoxItem).Tag as string;
                SelectedToolPanel = ToolPanels[ToolbarName];
            }
        }
    }
}
