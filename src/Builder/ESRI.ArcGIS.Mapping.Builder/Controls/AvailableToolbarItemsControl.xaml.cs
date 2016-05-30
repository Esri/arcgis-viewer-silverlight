/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Mapping.Core;
using ESRIControls = ESRI.ArcGIS.Mapping.Controls;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Builder.Controls
{

    public partial class AvailableToolbarItemsControl : UserControl
    {
        public List<AddToolbarItem> AvailableItems { get; private set; }

        public Visibility OKButtonVisibility
        {
            get { return (Visibility)GetValue(OKButtonVisibilityProperty); }
            set { SetValue(OKButtonVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OKButtonVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OKButtonVisibilityProperty =
            DependencyProperty.Register("OKButtonVisibility", typeof(Visibility), typeof(AvailableToolbarItemsControl), new PropertyMetadata(Visibility.Collapsed));

        
        public AvailableToolbarItemsControl(bool showOKButton)
        {
            OKButtonVisibility = showOKButton ? Visibility.Visible : System.Windows.Visibility.Collapsed;
            init();
        }

        public AvailableToolbarItemsControl()
        {
            init();
        }

        void init()
        {
            DataContext = this;
            buildListOfCommands();
            InitializeComponent();
            buildAddItemTree();
        }

        private void buildListOfCommands()
        {
            AvailableItems = new List<AddToolbarItem>();

            // Add the internal controls assembly            
            AssemblyManager.AddAssembly(typeof(ESRI.ArcGIS.Mapping.Controls.CommandBase).Assembly);

            // Add the GP assembly            
            AssemblyManager.AddAssembly(typeof(ESRI.ArcGIS.Mapping.GP.GeoprocessingCommand).Assembly);

            IEnumerable<Type> exportedCommands = AssemblyManager.GetExportsForType(typeof(ICommand));
            if (exportedCommands != null)
            {
                foreach (Type type in exportedCommands)
                {
                    ProcessType(type);
                }
            }

            IEnumerable<Type> exportedControls = AssemblyManager.GetExportsForType(typeof(FrameworkElement));
            if (exportedControls != null)
            {
                foreach (Type type in exportedControls)
                {
                    ProcessType(type);
                }
            }

            // Sort items by category and then by name
            AvailableItems = (from item in AvailableItems
                        orderby item.Category, item.Name
                        select item).ToList();
        }

        /// <summary>
        /// Processes a type that implements the ICommand interface, it detects various special attributes to determine
        /// if they should appear in the generated list of commands that can be added to the toolbar.
        /// </summary>
        /// <param name="t">The type to process.</param>
        private void ProcessType(Type t)
        {
            AddToolbarItem cmd = ToolbarManagement.CreateToolbarItemForType(t);

            // If it assumed that a command MUST have the DisplayName attribute properly assigned and thus we consider
            // these valid items for our generated list.
            if (!String.IsNullOrEmpty(cmd.Name))
            {
                // If the command does not specify a category, then group all of these into the "Uncategorized" category
                // so they are grouped together
                if (String.IsNullOrEmpty(cmd.Category))
                    cmd.Category = "Uncategorized";

                // Store the type in the object so it can be dynamically created when needed later
                cmd.ToolbarItemType = t;

                AvailableItems.Add(cmd);
            }
        }        

        internal void Refresh()
        {
            buildListOfCommands();

            buildAddItemTree();
        }

        private void buildAddItemTree()
        {
            // Clear all items in the tree control then add those determined in the constructor
            AddItemTree.Items.Clear();

            if (AvailableItems == null)
                return;

            // Process all elements of the item list, dynamically creating parent category nodes and attaching
            // child nodes to these parents.
            string curCategory = null;
            TreeViewItem parent = null;
            bool isSelected = false;
            foreach (AddToolbarItem item in AvailableItems)
            {
                // If the current category does not match the category of the current item then create a new category node
                if (String.Compare(curCategory, item.Category) != 0)
                {
                    // Make this parent node selected only if the current category value is null so that the top category is selected
                    // but none of the subsequent category nodes are selected.
                    isSelected = curCategory == null ? true : false;
                    TreeViewItem tvi = new TreeViewItem()
                    {
                        Name = item.Category,
                        Header = item.Category,
                        IsExpanded = true,
                        IsSelected = isSelected,
                        ItemContainerStyle = LayoutRoot.Resources["TreeViewItemStyle"] as Style,
                    };
                    ToolbarManagement.SetAssociatedToolbarItem(tvi, item);
                    AddItemTree.Items.Add(tvi);

                    // Assign the item category as the "current" category and assign the newly created category node as the
                    // parent to which all subsequent items are added.
                    curCategory = item.Category;
                    parent = tvi;
                }

                // Create a button display info object in order to store all the associated data which is
                // bound to the tree control for proper rendering.
                ButtonDisplayInfo bdi = ToolbarManagement.CreateButtonDisplayInfoForToolbarItem(item);              

                // Convert the button into a tree view item and associate proper styles, etc.
                TreeViewItem child = createTreeViewNodeForToolButton(bdi);
                child.Name = item.Category + item.Name + item.ToolbarItemType.ToString(); 
                ToolbarManagement.SetAssociatedToolbarItem(child, item);

                // Use item description (if any) for the tooltip
                if (!String.IsNullOrEmpty(item.Description))
                    child.SetValue(ToolTipService.ToolTipProperty, item.Description);

                // Add child to parent node
                parent.Items.Add(child);
            }
        }

        private TreeViewItem createTreeViewNodeForToolButton(ButtonDisplayInfo btnDisplayInfo)
        {
            TreeViewItem node = new TreeViewItem()
            {
                Header = btnDisplayInfo,
                HeaderTemplate = ToolbarControlNodeDataTemplate,
                ItemContainerStyle = LayoutRoot.Resources["TreeViewItemStyle"] as Style
            };
            return node;
        }

        private void ToolbarConfigurationTree_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {         
            TreeViewItem selectedAddItem = AddItemTree.SelectedItem as TreeViewItem;
            // There has to be a selected add item and its parent cannot be the tree itself (which implies it is a child of
            // a category parent node) in which case the OK button can be enabled.
            if (selectedAddItem != null && selectedAddItem.Parent != AddItemTree)
            {
                SelectedToolItemType = ToolbarManagement.GetAssociatedToolbarItem(selectedAddItem).ToolbarItemType;
            }
            else 
                SelectedToolItemType = null;
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }

        public Type SelectedToolItemType { get; set; }

        /// <summary>
        /// An instance of the class (e.g. command, control, etc) underlying the selected toolbar item
        /// </summary>
        public object SelectedClass
        {
            get
            {
                TreeViewItem selectedItem = AddItemTree.SelectedItem as TreeViewItem;
                if (selectedItem.Tag as Dictionary<string, object> == null && SelectedToolItemType != null)
                    populateTagForSelectedItem();

                return selectedItem.Tag as Dictionary<string, object> != null ? 
                    ((Dictionary<string, object>)selectedItem.Tag)["Class"] : null;
            }
        }

        public ButtonDisplayInfo SelectedItemDisplayInfo
        {
            get
            {
                TreeViewItem selectedItem = AddItemTree.SelectedItem as TreeViewItem;
                if (selectedItem.Tag as Dictionary<string, object> == null && SelectedToolItemType != null)
                    populateTagForSelectedItem();

                return selectedItem.Tag as Dictionary<string, object> != null ? 
                    ((Dictionary<string, object>)selectedItem.Tag)["DisplayInfo"] as ButtonDisplayInfo : null;
            }
        }

        private void AddItemTree_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
             bool doubleClick = MouseButtonHelper.IsDoubleClick(sender, e);
             if (doubleClick)
             {
                 if (DoubleClick != null)
                     DoubleClick(this, e);
             }
        }

         private void btnOk_Click(object sender, RoutedEventArgs e)
         {
             if (OKClicked != null)
                 OKClicked(this, EventArgs.Empty);
         }
    
        public event EventHandler SelectedItemChanged;
        public event EventHandler DoubleClick;
        public event EventHandler OKClicked;

        private void populateTagForSelectedItem()
        {
            TreeViewItem selectedItem = AddItemTree.SelectedItem as TreeViewItem;
            Dictionary<string, object> tagProps = new Dictionary<string, object>();
            tagProps.Add("Class", Activator.CreateInstance(SelectedToolItemType));
            tagProps.Add("DisplayInfo", ToolbarManagement.CreateButtonDisplayInfoForTreeViewItem(selectedItem));
            selectedItem.Tag = tagProps;
        }
    }
}
