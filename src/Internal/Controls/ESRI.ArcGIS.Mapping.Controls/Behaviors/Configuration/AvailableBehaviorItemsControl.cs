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
using ESRI.ArcGIS.Mapping.Core;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class AddBehaviorItem
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public Type BehaviorType { get; set; }
    }

    [TemplatePart(Name = "AddItemTree", Type = typeof(System.Windows.Controls.TreeView))]
    public class AvailableBehaviorItemsControl : Control
    {
        private System.Windows.Controls.TreeView AddItemTree;
        public AvailableBehaviorItemsControl()
        {
            this.DefaultStyleKey = typeof(AvailableBehaviorItemsControl);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AddItemTree = GetTemplateChild("AddItemTree") as System.Windows.Controls.TreeView;
            if (AddItemTree != null)
            {
                AddItemTree.SelectedItemChanged -= AddItemTree_SelectedItemChanged;
                AddItemTree.SelectedItemChanged += AddItemTree_SelectedItemChanged;
            }
            init();
        }

        List<AddBehaviorItem> addItems;

        void init()
        {
            DataContext = this;
            buildListOfBehaviors();
            buildAddItemTree();
        }

        private void buildListOfBehaviors()
        {
            addItems = new List<AddBehaviorItem>();

            AssemblyManager.AddAssembly(typeof(ESRI.ArcGIS.Mapping.Controls.ConstrainExtentBehavior).Assembly);

            IEnumerable<Type> exportedBehaviors = AssemblyManager.GetExportsForType(typeof(Behavior<Map>));
            if (exportedBehaviors != null)
            {
                foreach (Type type in exportedBehaviors)
                {
                    ProcessICommandType(type);
                }
            }

            // Sort items by category and then by name
            addItems = (from item in addItems
                        orderby item.Category, item.Name
                        select item).ToList();
        }

        /// <summary>
        /// Processes a type that implements the ICommand interface, it detects various special attributes to determine
        /// if they should appear in the generated list of commands that can be added to the toolPanel.
        /// </summary>
        /// <param name="t">The type to process.</param>
        private void ProcessICommandType(Type t)
        {
            AddBehaviorItem cmd = GetICommandAttributes(t);

            // If it assumed that a command MUST have the DisplayName attribute properly assigned and thus we consider
            // these valid items for our generated list.
            if (!String.IsNullOrEmpty(cmd.Name))
            {
                // If the command does not specify a category, then group all of these into the "Uncategorized" category
                // so they are grouped together
                if (String.IsNullOrEmpty(cmd.Category))
                    cmd.Category = "Uncategorized";

                // Store the type in the object so it can be dynamically created when needed later
                cmd.BehaviorType = t;

                addItems.Add(cmd);
            }
        }

        internal static AddBehaviorItem GetICommandAttributes(Type t)
        {
            // Get all custom attributes associated with this type but do not gather those that might be obtained
            // via inheritance.
            object[] attrs = t.GetCustomAttributes(false);

            // Create object to store information
            AddBehaviorItem cmd = new AddBehaviorItem();

            // Process each attribute, looking for the ones we care about and if found, extract information and continue
            // to the next attribute.
            foreach (object att in attrs)
            {
                ESRI.ArcGIS.Client.Extensibility.CategoryAttribute catAttribute = att as ESRI.ArcGIS.Client.Extensibility.CategoryAttribute;
                if (catAttribute != null)
                {
                    cmd.Category = catAttribute.Category;
                    continue;
                }

                ESRI.ArcGIS.Client.Extensibility.DisplayNameAttribute nameAttribute = att as ESRI.ArcGIS.Client.Extensibility.DisplayNameAttribute;
                if (nameAttribute != null)
                {
                    cmd.Name = nameAttribute.Name;
                    continue;
                }

                ESRI.ArcGIS.Client.Extensibility.DescriptionAttribute descAttribute = att as ESRI.ArcGIS.Client.Extensibility.DescriptionAttribute;
                if (descAttribute != null)
                {
                    cmd.Description = descAttribute.Description;
                    continue;
                }
            }
            return cmd;
        }

        internal void Refresh()
        {
            buildListOfBehaviors();

            buildAddItemTree();
        }

        private void buildAddItemTree()
        {
            // Clear all items in the tree control then add those determined in the constructor
            AddItemTree.Items.Clear();

            if (addItems == null)
                return;

            // Process all elements of the item list, dynamically creating parent category nodes and attaching
            // child nodes to these parents.
            string curCategory = null;
            TreeViewItem parent = null;
            bool isSelected = false;
            foreach (AddBehaviorItem item in addItems)
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
                    };
                    AddItemTree.Items.Add(tvi);

                    // Assign the item category as the "current" category and assign the newly created category node as the
                    // parent to which all subsequent items are added.
                    curCategory = item.Category;
                    parent = tvi;
                }

                // Create an instance of the type
                Behavior<Map> behavior = Activator.CreateInstance(item.BehaviorType) as Behavior<Map>;

                // Create a button display info object in order to store all the associated data which is
                // bound to the tree control for proper rendering.
                ExtensionBehavior bdi = new ExtensionBehavior();
                bdi.BehaviorId = Guid.NewGuid().ToString();
                bdi.Title = item.Name;
                bdi.IsEnabled = true;
                bdi.MapBehavior = behavior;

                // Convert the button into a tree view item and associate proper styles, etc.
                TreeViewItem child = createTreeViewNodeForToolButton(bdi);
                child.Name = item.Category + item.Name + item.BehaviorType.ToString(); 

                // Use item description (if any) for the tooltip
                if (!String.IsNullOrEmpty(item.Description))
                    child.SetValue(ToolTipService.ToolTipProperty, item.Description);

                // Add child to parent node
                parent.Items.Add(child);
            }
        }

        private TreeViewItem createTreeViewNodeForToolButton(ExtensionBehavior behavior)
        {
            TreeViewItem node = new TreeViewItem()
            {
                Header = behavior.Title,
                Tag = behavior,
                Style = AddItemTree.ItemContainerStyle
            };
            return node;
        }

        private void AddItemTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ExtensionBehavior behavior = null;
            TreeViewItem selectedAddItem = AddItemTree.SelectedItem as TreeViewItem;
            // There has to be a selected add item and its parent cannot be the tree itself (which implies it is a child of
            // a category parent node) in which case the OK button can be enabled.
            if (selectedAddItem != null && selectedAddItem.Parent != AddItemTree)
            {
                ExtensionBehavior itemToAdd = selectedAddItem.Tag as ExtensionBehavior;
                 if (itemToAdd != null)
                 {
                     // Use the button base, but clone the button info object so that if the user edits the associated
                     // values in this copy, it will not alter the original values in the "Add Tools" treeview.
                     behavior = new ExtensionBehavior();
                     behavior.BehaviorId = Guid.NewGuid().ToString();
                     behavior.MapBehavior = Activator.CreateInstance(itemToAdd.MapBehavior.GetType()) as Behavior<Map>;
                     behavior.IsEnabled = true;
                     behavior.Title = itemToAdd.Title;
                 }
            }
            SelectedItem = behavior;
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }

        #region SelectedItem
        public ExtensionBehavior SelectedItem
        {
            get { return GetValue(SelectedItemProperty) as ExtensionBehavior; }
            set { SetValue(SelectedItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(ExtensionBehavior), typeof(AvailableBehaviorItemsControl), null);
        #endregion

        public event EventHandler SelectedItemChanged;
    }
}
