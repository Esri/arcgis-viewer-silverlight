/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using ESRIControls = ESRI.ArcGIS.Mapping.Controls;
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Builder.Controls
{
    public partial class ManageToolbarControl : UserControl
    {
        private const string MAIN_TOOLBAR_CONTAINER_NAME = "MainToolbarContainer";
        private enum ToolType { ToolTypeButton, ToolTypeSeparator, ToolTypeGallery, ToolTypeGroup, ToolTypeNone };

        public bool IsLoaded { get; set; }

        public ManageToolbarControl()
        {
            InitializeComponent();
            IsLoaded = false;
        }

        private ToolPanels ToolPanels
        {
            get
            {
                ESRIControls.ViewerApplicationControl instance = ESRIControls.ViewerApplicationControl.Instance;
                if (instance == null || instance.ToolPanels == null)
                    return null;

                return instance.ToolPanels;
            }
        }

        private void ToolbarConfigControl_Loaded(object sender, RoutedEventArgs e)
        {
            // NOTE: There are a variety of situations where this loaded event is fired when it should not. After
            // researching it on the Internet, there appears to be issues with the Silverlight Toolkit and the
            // tree control which is used extensively in this control. As a result, I have added this boolean
            // to prevent initialization from happening more than once. This solves the problem of strange behavior
            // in the Viewer toolbar when existing group buttons and dropdown commands are manipulated in this UI.
            if (IsLoaded == false)
            {
                if (ESRIControls.NotificationPanel.Instance.Notifications.Count > 0)
                    lnkExtensionNotifications.Visibility = System.Windows.Visibility.Visible;

                buildToolBarTree();

                // NOTE: Indicate loading has occurred so we can prevent it until this control is displayed again
                // from its corresponding command which will then reset this setting to permit loading the toolbar
                // again, presumably from a different viewer instance which may have a different toolbar configuration.
                IsLoaded = true;
            }
        }

        private void buildToolBarTree()
        {
            if (ToolPanels == null || ToolPanels.Count == 0)
                return;

            // Process tools in the viewer application
            ToolbarConfigurationTree.Items.Clear();
            bool isSelected = true;
            foreach (ToolPanel toolbar in ESRIControls.ViewerApplicationControl.Instance.ToolPanels)
            {
                if (!toolbar.CanSerialize)
                    continue;

                TreeViewItem rootToolbarConfigurationNode = new TreeViewItem()
                {
                    Header = toolbar.Name,
                    Name = toolbar.ContainerName,
                    Tag = ToolType.ToolTypeNone,
                    IsExpanded = true,
                    IsSelected = isSelected,
                    ItemContainerStyle = LayoutRoot.Resources["TreeViewItemStyle"] as Style,
                };

                // This is so that the first "parent node" is selected similar to the "add items" tree instead of the
                // last "parent node" which is how it behaved before (and this item was typically offscreen so the user
                // didn't know it until they scrolled the tree down but could easily add an item to this offscreen
                // node).
                isSelected = false;

                ToolbarConfigurationTree.Items.Add(rootToolbarConfigurationNode);
                foreach (FrameworkElement toolbarItem in toolbar.ToolPanelItems)
                {
                    DropDownButton dropDownButton = toolbarItem as DropDownButton;
                    if (dropDownButton != null)
                    {
                        ButtonBase dropDownContentButton = dropDownButton.Content as ButtonBase;
                        if (dropDownContentButton == null)
                            continue;

                        TreeViewItem groupNode = createTreeViewNodeForToolGroupButton(ToolType.ToolTypeGroup, dropDownContentButton.DataContext as ButtonDisplayInfo);
                        rootToolbarConfigurationNode.Items.Add(groupNode);

                        Panel panel = dropDownButton.PopupContent as Panel;
                        if (panel != null)
                        {
                            // Process each element in the child panel
                            foreach (UIElement elem in panel.Children)
                            {
                                // If the element is a Button then create a button node for the tree
                                ButtonBase nestedButton = elem as ButtonBase;
                                if (nestedButton != null)
                                {
                                    TreeViewItem node = createTreeViewNodeForToolButton(ToolType.ToolTypeButton, nestedButton.DataContext as ButtonDisplayInfo);
                                    groupNode.Items.Add(node);
                                    continue;
                                }

                                // If the element is a ContentControl then create a separator node for the tree
                                ContentControl separator = elem as ContentControl;
                                if (separator != null)
                                {
                                    TreeViewItem separatorNode = createTreeViewNodeForSeparator(ToolType.ToolTypeSeparator);
                                    groupNode.Items.Add(separatorNode);
                                    continue;
                                }

                                // Associate the element with the group button instance and this is a control
                                // itself like "Choose Basemap"
                                FrameworkElement frameworkElem = elem as FrameworkElement;
                                if (frameworkElem != null)
                                {
                                    groupNode.Tag = ToolType.ToolTypeGallery;
                                }
                            }
                        }
                    }
                    else
                    {
                        // The parent element is NOT a group button so it is either a Button or a Separator
                        ButtonBase btnBase = toolbarItem as ButtonBase;
                        if (btnBase != null)
                        {
                            TreeViewItem node = createTreeViewNodeForToolButton(ToolType.ToolTypeButton, btnBase.DataContext as ButtonDisplayInfo);
                            rootToolbarConfigurationNode.Items.Add(node);
                        }
                        else
                        {
                            // If the framework element is not a drop down button and it's not a button base
                            // then it must be a separator
                            TreeViewItem node = createTreeViewNodeForSeparator(ToolType.ToolTypeSeparator);
                            rootToolbarConfigurationNode.Items.Add(node);
                        }
                    }
                }
            }
        }

        private TreeViewItem createTreeViewNodeForToolGroupButton(ToolType toolType, ButtonDisplayInfo dataContext)
        {
            TreeViewItem node = new TreeViewItem()
            {
                Header = dataContext,
                Tag = toolType,
                HeaderTemplate = ToolbarGroupNodeDataTemplate,
                ItemContainerStyle = LayoutRoot.Resources["TreeViewItemStyle"] as Style
            };
            // Use item description (if any) for the tooltip
            if (!String.IsNullOrEmpty(dataContext.Description))
            {
                System.Windows.Data.Binding b = new System.Windows.Data.Binding("Description") { Source = dataContext };
                node.SetBinding(ToolTipService.ToolTipProperty, b);
            }

            return node;
        }

        private TreeViewItem createTreeViewNodeForToolButton(ToolType toolType, ButtonDisplayInfo dataContext)
        {
            TreeViewItem node = new TreeViewItem()
            {
                Header = dataContext,
                Tag = toolType,
                HeaderTemplate = ToolbarControlNodeDataTemplate,
                ItemContainerStyle = LayoutRoot.Resources["TreeViewItemStyle"] as Style
            };
            // Use item description (if any) for the tooltip
            if (!String.IsNullOrEmpty(dataContext.Description))
            {
                System.Windows.Data.Binding b = new System.Windows.Data.Binding("Description") { Source = dataContext };
                node.SetBinding(ToolTipService.ToolTipProperty, b);
            }

            return node;
        }

        private TreeViewItem createTreeViewNodeForSeparator(ToolType toolType)
        {
            TreeViewItem node = new TreeViewItem()
            {
                Header = ESRI.ArcGIS.Mapping.Builder.Resources.Strings.Separator,
                Tag = toolType,
                ItemContainerStyle = LayoutRoot.Resources["TreeViewItemStyle"] as Style
            };
            return node;
        }

        private bool IsDropDownCommand(TreeViewItem item)
        {
            if (item.Tag == null)
                return false;

            ToolType tt = (ToolType)item.Tag;
            return tt == ToolType.ToolTypeGallery;
        }

        private bool IsGroupButton(TreeViewItem item)
        {
            if (item.Tag == null)
                return false;

            ToolType tt = (ToolType)item.Tag;
            return tt == ToolType.ToolTypeGroup;
        }

        private void AddSeparator_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedItem = ToolbarConfigurationTree.SelectedItem as TreeViewItem;
            if (selectedItem == null)
                return;

            TreeViewItem itemParent = null;
            bool expandParent = false;
            int insertPosition = 0;

            GetParentAndPosition(selectedItem, out itemParent, out insertPosition, out expandParent);

            ToolPanel tbar = null;
            if (itemParent != null)
            {
                tbar = ToolPanels[itemParent.Name];
                if (IsGroupButton(selectedItem))
                {
                    // If the selected item is a group button, then the tool should be appended
                    insertPosition = 0;
                }
            }
            else
            {
                TreeViewItem groupParent = selectedItem.Parent as TreeViewItem;
                if (groupParent != null)
                {
                    if (IsGroupButton(groupParent))
                    {
                        GetParentAndPosition(groupParent, out itemParent, out insertPosition, out expandParent);
                        if (itemParent != null)
                        {
                            tbar = ToolPanels[itemParent.Name];
                            insertPosition = groupParent.Items.IndexOf(selectedItem) + 1;
                            selectedItem = groupParent;
                        }
                    }
                }
            }

            if (tbar != null)
            {
                if (IsGroupButton(selectedItem))
                {
                    ButtonBase btn = GetTreeViewItemObject(selectedItem) as ButtonBase;
                    tbar.AddNestedSeparatorElement(btn, false, insertPosition);
                    itemParent = selectedItem;
                    expandParent = true;
                }
                else
                {
                    tbar.AddToolPanelSeparatorElement(tbar.ToolPanelItems, true, insertPosition);
                }

                TreeViewItem newNode = createTreeViewNodeForSeparator(ToolType.ToolTypeSeparator);
                newNode.IsSelected = true;

                // Add node to the designated parent
                if (insertPosition < 0)
                    itemParent.Items.Add(newNode);
                else
                    itemParent.Items.Insert(insertPosition, newNode);

                // If the parent item being added to is not expanded, expand it so we can see this newly
                // added child node.
                if (selectedItem.IsExpanded == false && expandParent == true)
                    selectedItem.IsExpanded = true;
            }
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            // Make sure there is a selected item in the tree
            TreeViewItem selectedItem = ToolbarConfigurationTree.SelectedItem as TreeViewItem;
            if (selectedItem == null)
                return;

            // Make sure the selected item is a root node or an immediate child which is required to add a
            // group button.
            TreeViewItem itemParent = null;
            bool expandParent = false;
            int insertPosition = 0;

            GetParentAndPosition(selectedItem, out itemParent, out insertPosition, out expandParent);
            if (itemParent == null)
            {
                ESRIControls.MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.RootLevelToAddGroup,
                    ESRI.ArcGIS.Mapping.Builder.Resources.Strings.CannotAddGroupButton, MessageBoxButton.OK);
                return;
            }

            ToolPanel tbar = ToolPanels[itemParent.Name];
            // Create display info to encapsulate the new group's display properties
            ButtonDisplayInfo displayInfo = new ButtonDisplayInfo()
            {
                Description = ESRI.ArcGIS.Mapping.Builder.Resources.Strings.DefaultGroupButtonDescription,
                Label = ESRI.ArcGIS.Mapping.Builder.Resources.Strings.DefaultGroupButtonLabel,
                Icon = ToolbarManagement.GetDefaultGroupIconUrl()
            };

            // Create command to configure the group
            ConfigureToolPanelItemCommand configureCommand = new ConfigureToolPanelItemCommand()
            {
                DisplayInfo = displayInfo,
                DialogTitle = ESRI.ArcGIS.Mapping.Builder.Resources.Strings.AddGroupButton
            };

            // Wire to the completed event to add the new group 
            configureCommand.Completed += (o, args) =>
            {
                // Create group button and add it to the proper toolbar
                DropDownButton galleryButton =
                    tbar.AddToolGroupButton(displayInfo, null, tbar.ToolPanelItems, insertPosition) as DropDownButton;

                // Create treeview item and add to the tree control
                TreeViewItem newNode = createTreeViewNodeForToolGroupButton(ToolType.ToolTypeGroup, displayInfo);
                newNode.IsSelected = true;

                // Add node to the designated parent
                if (insertPosition < 0)
                    itemParent.Items.Add(newNode);
                else
                    itemParent.Items.Insert(insertPosition, newNode);

                // Re-apply the header to cause UI update
                DataTemplate dt = newNode.HeaderTemplate;
                newNode.HeaderTemplate = null;
                newNode.HeaderTemplate = dt;
            };

            // Execute the configuration command
            configureCommand.Execute(null);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            PerformAddOperation();
        }

        private void PerformAddOperation()
        {
            // Obtain the selected item, if any, from the event argument
            TreeViewItem selectedToolItem = AvailableToolsControl.AddItemTree.SelectedItem as TreeViewItem;

            // Get the selected item in the parent tree so we can append this new node as its child
            TreeViewItem selectedItem = ToolbarConfigurationTree.SelectedItem as TreeViewItem;
            if (selectedItem == null)
                return;

            // Make sure this operation can be performed by examining both the source item in the add
            // tree and the target item in the toolbar tree.
            if (IsAddOperationEnabled((TreeViewItem)AvailableToolsControl.AddItemTree.SelectedItem, selectedItem) == false)
                return;

            // If the item is not null, then add it to the proper location in the tree of items currently
            // part of the viewer
            Type typeOfElem = AvailableToolsControl.SelectedToolItemType;
            if (typeOfElem != null)
            {
                ButtonDisplayInfo bdi = ToolbarManagement.CreateButtonDisplayInfoForTreeViewItem(selectedToolItem);

                TreeViewItem itemParent = null;
                bool expandParent = false;
                int insertPosition = 0;

                GetParentAndPosition(selectedItem, out itemParent, out insertPosition, out expandParent);

                // If a valid parent was found at the root level (it is the name of the toolbar or it is a child of
                // those parents so we are using the root node as the parent) then we know the toolbar. If the item
                // parent is null, it may still be valid if the node in question has a parent that is a dropdown
                // button that has a panel as its content which means the selected target is the child of a group
                // button and we need to add this tool as a child of the parent that is a group button.
                ToolPanel tbar = null;
                if (itemParent != null)
                {
                    tbar = ToolPanels[itemParent.Name];
                    if (IsGroupButton(selectedItem))
                    {
                        // If the selected item is a group button, then the tool should be appended
                        insertPosition = 0;
                    }
                }
                else
                {
                    TreeViewItem groupParent = selectedItem.Parent as TreeViewItem;
                    if (groupParent != null)
                    {
                        if (IsGroupButton(groupParent))
                        {
                            GetParentAndPosition(groupParent, out itemParent, out insertPosition, out expandParent);
                            if (itemParent != null)
                            {
                                tbar = ToolPanels[itemParent.Name];
                                insertPosition = groupParent.Items.IndexOf(selectedItem) + 1;
                                selectedItem = groupParent;
                            }
                        }
                    }
                }

                if (tbar != null)
                {
                    TreeViewItem newNode = null;
                    object instance = Activator.CreateInstance(typeOfElem);
                    ICommand command = instance as ICommand;
                    if (command != null)
                    {
                        if (IsGroupButton(selectedItem))
                        {
                            ButtonBase groupButton = GetTreeViewItemObject(selectedItem) as ButtonBase;
                            tbar.AddNestedToolButton(bdi, command, groupButton, insertPosition);
                            itemParent = selectedItem;
                            expandParent = true;
                        }
                        else
                        {
                            tbar.AddToolButton(bdi, command, tbar.ToolPanelItems, insertPosition);
                        }

                        newNode = createTreeViewNodeForToolButton(ToolType.ToolTypeButton, bdi);
                        newNode.IsSelected = true;

                        // Add node to the designated parent
                        if (insertPosition < 0)
                            itemParent.Items.Add(newNode);
                        else
                            itemParent.Items.Insert(insertPosition, newNode);
                    }
                    else
                    {
                        // This logic is used when the button being added is Basemap Gallery or a similar command that
                        // uses a drop down button.
                        FrameworkElement frameworkElement = instance as FrameworkElement;
                        if (frameworkElement != null)
                        {
                            DropDownButton galleryButton = tbar.AddToolGroupButton(bdi, null, tbar.ToolPanelItems, insertPosition) as DropDownButton;
                            if (galleryButton != null)
                            {
                                Panel sp = galleryButton.PopupContent as Panel;
                                if (sp != null)
                                    sp.Children.Add(frameworkElement);

                                newNode = createTreeViewNodeForToolGroupButton(ToolType.ToolTypeGallery, bdi);
                                newNode.IsSelected = true;

                                // Add node to the designated parent
                                if (insertPosition < 0)
                                    itemParent.Items.Add(newNode);
                                else
                                    itemParent.Items.Insert(insertPosition, newNode);
                            }
                        }
                    }

                    // If the parent item being added to is not expanded, expand it so we can see this newly
                    // added child node.
                    if (selectedItem.IsExpanded == false && expandParent == true)
                        selectedItem.IsExpanded = true;

                    // Show configuration UI for newly added tool. 
                    if (instance != null && (instance is ISupportsConfiguration || instance is ISupportsWizardConfiguration))
                        editSelectedItem();
                }

                #region Scroll TreeView to show item just added research
                //TreeViewItemAutomationPeer trvItemAutomation = (TreeViewItemAutomationPeer)
                //    TreeViewItemAutomationPeer.CreatePeerForElement(node);
                //IScrollItemProvider scrollingAutomationProvider = (IScrollItemProvider)trvItemAutomation.GetPattern(PatternInterface.ScrollItem);
                //scrollingAutomationProvider.ScrollIntoView();

                //TreeView tv = selectedItem.GetParentTreeView();
                //ScrollViewer sv = tv.GetScrollHost();
                //sv.ScrollToBottom();
                //sv.ScrollToVerticalOffset(20);
                //sv.ScrollIntoView(node);

                //tv.UpdateLayout();
                //sv.UpdateLayout();

                //TreeView tv = node.GetParentTreeView();
                //TreeViewAutomationPeer trvAutomationProvider = (TreeViewAutomationPeer)
                //    TreeViewAutomationPeer.CreatePeerForElement(tv);
                //IScrollProvider scrollingAutomationProvider = (IScrollProvider)trvAutomationProvider.GetPattern(PatternInterface.Scroll);
                //scrollingAutomationProvider.SetScrollPercent(-1.0, 50);                
                #endregion
            }
        }

        private object GetTreeViewItemObject(TreeViewItem item)
        {
            // If the item in question is null, return null
            if (item == null)
                return null;

            // If the item does not have a parent, return null
            TreeViewItem parentItem = item.Parent as TreeViewItem;
            if (parentItem == null)
                return null;

            // Determine the containing toolbar
            ToolPanel tbar = null;
            TreeViewItem rootParent = null;
            if (String.IsNullOrEmpty(parentItem.Name))
            {
                rootParent = parentItem.Parent as TreeViewItem;
                tbar = ToolPanels[rootParent.Name];
            }
            else
            {
                tbar = ToolPanels[parentItem.Name];
            }

            // If no toolbar is determined, return null
            if (tbar == null)
                return null;

            // If the depth is 1 then this is a direct descendant of the toolbar. If the depth is 2, then it is the child
            // of a group button.
            int depth = GetHierarchyDepth(item);

            if (depth == 1)
            {
                int toolbarPos = parentItem.Items.IndexOf(item);
                return tbar.ToolPanelItems[toolbarPos];
            }
            else
            {
                int toolbarPos = rootParent.Items.IndexOf(parentItem);
                FrameworkElement elem = tbar.ToolPanelItems[toolbarPos];

                int groupButtonPos = parentItem.Items.IndexOf(item);
                DropDownButton dropDownButton = elem as DropDownButton;
                if (dropDownButton != null)
                {
                    Panel panel = dropDownButton.PopupContent as Panel;
                    if (panel != null)
                    {
                        return panel.Children[groupButtonPos];
                    }
                }
            }

            return null;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedItem = ToolbarConfigurationTree.SelectedItem as TreeViewItem;
            if (selectedItem == null)
                return;

            TreeViewItem parentItem = selectedItem.Parent as TreeViewItem;
            if (parentItem == null)
                return;

            ToolPanel tbar = null;
            if (String.IsNullOrEmpty(parentItem.Name))
            {
                TreeViewItem rootParent = parentItem.Parent as TreeViewItem;
                tbar = ToolPanels[rootParent.Name];
            }
            else
            {
                tbar = ToolPanels[parentItem.Name];
            }
            if (tbar == null)
                return;

            if (IsGroupButton(selectedItem))
            {
                ESRIControls.MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.AreYouSureYouWantToDeleteGroup, ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ConfirmDelete, MessageBoxButton.OKCancel,
                        new ESRIControls.MessageBoxClosedEventHandler(delegate(object obj, ESRIControls.MessageBoxClosedArgs args)
                        {
                            if (args.Result == MessageBoxResult.OK)
                            {
                                // Determine ordinal value of selected element amongst children
                                int prevPosition = parentItem.Items.IndexOf(selectedItem);

                                // Obtain element from toolbar and then use it to remove from toolbar
                                FrameworkElement elem = tbar.ToolPanelItems[prevPosition];
                                ButtonBase btnBase = elem as ButtonBase;
                                tbar.RemoveToolButton(btnBase, tbar.ToolPanelItems);

                                // Remove item from tree view
                                parentItem.Items.Remove(selectedItem);
                                if (prevPosition >= parentItem.Items.Count)
                                    prevPosition = parentItem.Items.Count - 1;

                                // Restore selection of item which is right after deleted item
                                if (prevPosition > -1 && prevPosition < parentItem.Items.Count())
                                    (parentItem.Items[prevPosition] as TreeViewItem).IsSelected = true;
                            }
                        }));
                return;
            }
            else
            {
                ESRIControls.MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.AreYouSureYouWantToDeleteButton, ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ConfirmDelete, MessageBoxButton.OKCancel,
                    new ESRIControls.MessageBoxClosedEventHandler(delegate(object obj, ESRIControls.MessageBoxClosedArgs args)
                    {
                        if (args.Result == MessageBoxResult.OK)
                        {
                            // Determine ordinal value of selected element amongst children
                            int prevPosition = parentItem.Items.IndexOf(selectedItem);

                            if (IsGroupButton(parentItem))
                            {
                                DropDownButton dropDownButton = GetTreeViewItemObject(parentItem) as DropDownButton;
                                if (dropDownButton != null)
                                {
                                    Panel panel = dropDownButton.PopupContent as Panel;
                                    if (panel != null)
                                    {
                                        UIElement elem = panel.Children[prevPosition];
                                        ButtonBase nestedButton = elem as ButtonBase;
                                        if (nestedButton != null)
                                        {
                                            tbar.RemoveNestedToolButton(nestedButton, dropDownButton);
                                        }
                                        else
                                        {
                                            // This is a separator and can be simply removed, there are no events or other
                                            // elements to worry about.
                                            panel.Children.RemoveAt(prevPosition);
                                        }
                                        dropDownButton.InvalidateArrange();
                                    }
                                }
                            }
                            else
                            {
                                FrameworkElement elem = tbar.ToolPanelItems[prevPosition];
                                ButtonBase btnBase = elem as ButtonBase;

                                // If the item selected for removal is a button of some kind, then use the proper
                                // toolbar method. If it is not, then it must be a separator so use that method instead.
                                if (btnBase != null)
                                {
                                    tbar.RemoveToolButton(btnBase, tbar.ToolPanelItems);
                                }
                                else
                                {
                                    tbar.RemoveToolbarSeparatorElement(elem as ContentControl, tbar.ToolPanelItems);
                                }
                            }

                            // Remove item from tree view
                            parentItem.Items.Remove(selectedItem);
                            if (prevPosition >= parentItem.Items.Count)
                                prevPosition = parentItem.Items.Count - 1;

                            // Restore selection of item which is right after deleted item
                            if (prevPosition > -1 && prevPosition < parentItem.Items.Count())
                                (parentItem.Items[prevPosition] as TreeViewItem).IsSelected = true;
                        }
                    }));
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            editSelectedItem();
        }

        // Show the configuration UI for the selected tool item
        private void editSelectedItem()
        {
            // Make sure there is a selected item in the tree
            TreeViewItem selectedItem = ToolbarConfigurationTree.SelectedItem as TreeViewItem;
            if (selectedItem == null)
                return;

            ButtonDisplayInfo displayInfo = selectedItem.Header as ButtonDisplayInfo;
            string dialogTitle = IsGroupButton(selectedItem) ? ESRI.ArcGIS.Mapping.Builder.Resources.Strings.EditGroup :
                ESRI.ArcGIS.Mapping.Builder.Resources.Strings.EditTool;

            // Get the instance containing the logic behind the tool item
            object toolInstance = null;
            ButtonBase btn = GetTreeViewItemObject(selectedItem) as ButtonBase;
            if (IsDropDownCommand(selectedItem))
            {
                DropDownButton ddb = btn as DropDownButton;
                if (ddb != null)
                {
                    Panel panel = ddb.PopupContent as Panel;
                    if (panel != null)
                    {
                        toolInstance = panel.Children[0];
                    }
                }
            }
            else
            {
                toolInstance = btn.Command;
            }

            // Initialize the configuration command, which will show the configuration UI for the 
            // selected tool panel item
            ConfigureToolPanelItemCommand configureCommand = new ConfigureToolPanelItemCommand()
            {
                DisplayInfo = displayInfo,
                DialogTitle = dialogTitle
            };

            configureCommand.Completed += (o, args) =>
            {
                // Re-apply the header to cause UI update
                DataTemplate dt = selectedItem.HeaderTemplate;
                selectedItem.HeaderTemplate = null;
                selectedItem.HeaderTemplate = dt;
            };

            configureCommand.Execute(toolInstance);
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedItem = ToolbarConfigurationTree.SelectedItem as TreeViewItem;
            if (selectedItem == null)
                return;

            TreeViewItem parentItem = selectedItem.Parent as TreeViewItem;
            if (parentItem == null)
                return;

            int pos = parentItem.Items.IndexOf(selectedItem);
            if (pos < 0)
                return;

            // If the new position is still within the limits of the parent, then simply move it there. However, if
            // the newly proposed position exceeds the limit of the children of the parent, then we are moving a
            // child element of a group button to be outside the group and to instead be a child of the overall root
            // of the toolbar.
            if ((pos - 1) >= 0)
            {
                // If the root element UP from the current position is a group button, then prompt the user
                // if they would like to move the current item into the group or just up one position. It must
                // also be the case that the item being moved into the group is NOT a drop down command nor is it
                // a group button since we do not allow these situations to exist.
                if (IsGroupButton(parentItem.Items[pos - 1] as TreeViewItem) && 
                    ((IsDropDownCommand(selectedItem) == false) && (IsGroupButton(selectedItem) == false)))
                {
                    // Get potential parent item group button to extract its name for use in the dialog
                    TreeViewItem newParent = parentItem.Items[pos - 1] as TreeViewItem;
                    ButtonDisplayInfo bdi = newParent.Header as ButtonDisplayInfo;

                    MoveIntoGroupDialog moveIntoGroupDialog = null;
                    moveIntoGroupDialog = new MoveIntoGroupDialog(false, bdi.Label,
                        new MoveIntoGroupClosedEventHandler(delegate(object obj, MoveIntoGroupEventArgs args)
                        {
                            BuilderApplication.Instance.HideWindow(moveIntoGroupDialog);
                            if (args.MoveAroundGroup)
                            {
                                parentItem.Items.RemoveAt(pos);
                                parentItem.Items.Insert(pos - 1, selectedItem);
                                MoveItemInToolbar(parentItem, pos, -1);
                                selectedItem.IsSelected = true;
                            }
                            else
                            {
                                object parentObject = GetTreeViewItemObject(newParent);
                                parentItem.Items.RemoveAt(pos);
                                MoveToolbarItemIntoGroup(selectedItem, newParent, -1, pos, parentObject);
                                selectedItem.IsSelected = true;
                            }
                        }
                        ));
                    BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.MoveUp, moveIntoGroupDialog, true);
                }
                else
                {
                    parentItem.Items.RemoveAt(pos);
                    parentItem.Items.Insert(pos - 1, selectedItem);
                    MoveItemInToolbar(parentItem, pos, -1);
                    selectedItem.IsSelected = true;
                }
            }
            else
            {
                object selectedObject = GetTreeViewItemObject(selectedItem);
                parentItem.Items.RemoveAt(pos);
                // Yes, I know it seems like the direction parameter should be "-1" but trust me, it needs to be
                // 0 so that the newly inserted item takes the place of the group parent that this used to belong
                // to.
                MoveToolbarGroupItemOutOfGroup(selectedItem, parentItem, pos, 0, selectedObject);
                selectedItem.IsSelected = true;
            }
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedItem = ToolbarConfigurationTree.SelectedItem as TreeViewItem;
            if (selectedItem == null)
                return;

            TreeViewItem parentItem = selectedItem.Parent as TreeViewItem;
            if (parentItem == null)
                return;

            int pos = parentItem.Items.IndexOf(selectedItem);
            if (pos < 0 || pos >= parentItem.Items.Count)
                return;

            // If the new position is still within the limits of the parent, then simply move it there. However, if
            // the newly proposed position exceeds the limit of the children of the parent, then we are moving a
            // child element of a group button to be outside the group and to instead be a child of the overall root
            // of the toolbar.
            if ((pos + 1) < parentItem.Items.Count)
            {
                // If the root element DOWN from the current position is a group button, then prompt the user
                // if they would like to move the current item into the group or just down one position. It must
                // also be the case that the item being moved into the group is NOT a drop down command nor is it
                // a group button itself since we do not allow these situations to exist.
                if (IsGroupButton(parentItem.Items[pos + 1] as TreeViewItem) && 
                    ((IsDropDownCommand(selectedItem) == false) && (IsGroupButton(selectedItem) == false)))
                {
                    // Get potential parent item group button to extract its name for use in the dialog
                    TreeViewItem newParent = parentItem.Items[pos + 1] as TreeViewItem;
                    ButtonDisplayInfo bdi = newParent.Header as ButtonDisplayInfo;

                    MoveIntoGroupDialog moveIntoGroupDialog = null;
                    moveIntoGroupDialog = new MoveIntoGroupDialog(true, bdi.Label,
                        new MoveIntoGroupClosedEventHandler(delegate(object obj, MoveIntoGroupEventArgs args)
                        {
                            BuilderApplication.Instance.HideWindow(moveIntoGroupDialog);
                            if (args.MoveAroundGroup)
                            {
                                parentItem.Items.RemoveAt(pos);
                                parentItem.Items.Insert(pos + 1, selectedItem);
                                MoveItemInToolbar(parentItem, pos, 1);
                                selectedItem.IsSelected = true;
                            }
                            else
                            {
                                object parentObject = GetTreeViewItemObject(newParent);
                                parentItem.Items.RemoveAt(pos);
                                MoveToolbarItemIntoGroup(selectedItem, newParent, 0, pos, parentObject);
                                selectedItem.IsSelected = true;
                            }
                        }
                        ));
                    BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.MoveDown, moveIntoGroupDialog, true);
                }
                else
                {
                    parentItem.Items.RemoveAt(pos);
                    parentItem.Items.Insert(pos + 1, selectedItem);
                    MoveItemInToolbar(parentItem, pos, 1);
                    selectedItem.IsSelected = true;
                }
            }
            else
            {
                object selectedObject = GetTreeViewItemObject(selectedItem);
                parentItem.Items.RemoveAt(pos);
                MoveToolbarGroupItemOutOfGroup(selectedItem, parentItem, pos, 1, selectedObject);
                selectedItem.IsSelected = true;
            }
        }

        private void MoveToolbarItemIntoGroup(TreeViewItem selectedItem, TreeViewItem parentItem, int pos, int origPosition, object parentObject)
        {
            // Get the toolbar root which is the parent of the selected item
            TreeViewItem overallRoot = parentItem.Parent as TreeViewItem;
            if (overallRoot != null)
            {
                DropDownButton dropDownButton = parentObject as DropDownButton;
                if (dropDownButton != null)
                {
                    Panel panel = dropDownButton.PopupContent as Panel;
                    if (panel != null)
                    {
                        ToolPanel tbar = ToolPanels[overallRoot.Name];
                        FrameworkElement elem = tbar.ToolPanelItems[origPosition];
                        ButtonBase currentButton = elem as ButtonBase;

                        if (pos < 0 || pos >= panel.Children.Count)
                            pos = panel.Children.Count;

                        if (currentButton != null)
                        {
                            // This is a button, not a separator
                            tbar.RemoveToolButton(currentButton, tbar.ToolPanelItems);

                            tbar.AddNestedToolButton(currentButton.DataContext as ButtonDisplayInfo,
                                currentButton.Command, dropDownButton, pos);

                            TreeViewItem newNode = createTreeViewNodeForToolButton(ToolType.ToolTypeButton, currentButton.DataContext as ButtonDisplayInfo);
                            newNode.IsSelected = true;
                            parentItem.Items.Insert(pos, newNode);
                        }
                        else
                        {
                            // This is a separator
                            tbar.RemoveToolbarSeparatorElement(elem as ContentControl, tbar.ToolPanelItems);
                            tbar.AddNestedSeparatorElement(dropDownButton, false, pos);
                            parentItem.Items.Insert(pos, selectedItem);
                        }

                        // Expand parent group button if it is not currently expanded
                        if (parentItem.IsExpanded == false)
                            parentItem.IsExpanded = true;
                    }
                }
            }
        }

        private void MoveToolbarGroupItemOutOfGroup(TreeViewItem selectedItem, TreeViewItem parentItem, int pos, int direction, object selectedObject)
        {
            // If the parent of the parent is a tree view item (and not a TreeView) then the selected item is
            // a child of a group button.
            TreeViewItem overallRoot = parentItem.Parent as TreeViewItem;
            if (overallRoot != null)
            {
                DropDownButton dropDownButton = GetTreeViewItemObject(parentItem) as DropDownButton;
                if (dropDownButton != null)
                {
                    Panel panel = dropDownButton.PopupContent as Panel;
                    if (panel != null)
                    {
                        ToolPanel tbar = ToolPanels[overallRoot.Name];
                        int rootPos = overallRoot.Items.IndexOf(parentItem);

                        panel.Children.RemoveAt(pos);

                        ButtonBase currentButton = selectedObject as ButtonBase;
                        if (currentButton != null)
                        {
                            tbar.AddToolButton(currentButton.DataContext as ButtonDisplayInfo,
                                currentButton.Command, tbar.ToolPanelItems, rootPos + direction);

                            selectedItem.Header = currentButton.DataContext;
                            selectedItem.Tag = ToolType.ToolTypeButton;
                        }
                        else
                        {
                            // This is a separator
                            tbar.AddToolPanelSeparatorElement(tbar.ToolPanelItems, true, rootPos + direction);
                            selectedItem.Tag = ToolType.ToolTypeSeparator;
                        }

                        overallRoot.Items.Insert(rootPos + direction, selectedItem);
                    }
                }
            }
        }

        private void MoveItemInToolbar(TreeViewItem parentItem, int pos, int direction)
        {
            if (IsGroupButton(parentItem))
            {
                DropDownButton groupButton = GetTreeViewItemObject(parentItem) as DropDownButton;
                if (groupButton != null)
                {
                    Panel panel = groupButton.PopupContent as Panel;
                    if (panel != null)
                    {
                        UIElement elem = panel.Children[pos];
                        panel.Children.RemoveAt(pos);
                        panel.Children.Insert(pos + direction, elem);
                    }
                }
            }
            else
            {
                // moving a top level node
                ToolPanel tbar = ToolPanels[parentItem.Name];
                if (tbar != null)
                {
                    FrameworkElement elem = tbar.ToolPanelItems[pos];
                    tbar.ToolPanelItems.RemoveAt(pos);
                    tbar.ToolPanelItems.Insert(pos + direction, elem);
                }
            }
        }

        private void ToolbarConfigurationTree_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selectedToolItem = ToolbarConfigurationTree.SelectedItem as TreeViewItem;

            if (selectedToolItem != null)
            {
                // If the node is a root level node, then the user cannot alter it so disable all commands
                if (!string.IsNullOrEmpty(selectedToolItem.Name))
                {
                    btnDelete.IsEnabled = btnEdit.IsEnabled = btnMoveUp.IsEnabled = btnMoveDown.IsEnabled = false;
                }
                else
                {
                    btnDelete.IsEnabled = true;

                    // If the selected item is not a button (it is a separator) then disable the edit command
                    ToolType tt = (ToolType)selectedToolItem.Tag;
                    btnEdit.IsEnabled = tt == ToolType.ToolTypeSeparator ? false : true;

                    enableDisableReorderButtonBasedOnSelectedItem(selectedToolItem);
                }
            }
            else
            {
                btnEdit.IsEnabled = btnDelete.IsEnabled = false;
            }
        }

        private int GetHierarchyDepth(TreeViewItem item)
        {
            int depth = 0;

            while (item.Parent != null)
            {
                if (item.Parent is TreeView)
                    return depth;

                ++depth;
                item = item.Parent as TreeViewItem;
            }

            return depth;
        }

        private bool IsAddOperationEnabled(TreeViewItem fromItem, TreeViewItem toItem)
        {
            if (fromItem == null || toItem == null)
            {
                ESRIControls.MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ChooseAvailableTool, ESRI.ArcGIS.Mapping.Builder.Resources.Strings.CannotAddTool, MessageBoxButton.OK);
                return false;
            }

            // We must determine if the item from the "available tools" is a dropdown command or not. Unfortunately, that
            // information requires some computation...
            Type typeOfElem = AvailableToolsControl.SelectedToolItemType;
            if (typeOfElem != null)
            {
                // If the selected tool is a drop down command then it can only be added to a root node and cannot be
                // added to a group button root node or a child of a group.
                if (typeOfElem.IsSubclassOf(typeof(FrameworkElement)))
                {
                    // Do not allow this to be added to the root node of a group
                    if (IsGroupButton(toItem))
                    {
                        ESRIControls.MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.CannotAddDropdownCommandsToAGroup, ESRI.ArcGIS.Mapping.Builder.Resources.Strings.CannotAddTool, MessageBoxButton.OK);
                        return false;
                    }

                    // If the destination is the root of a toolbar (depth = 0) or an immediate child of the root of a
                    // toolbar (depth = 1) then permit the item to be added. But if it is greater than 1, then it is
                    // the child of a group and this add should not be allowed.
                    if (GetHierarchyDepth(toItem) > 1)
                    {
						ESRIControls.MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.CannotAddDropdownCommandsToAGroup, ESRI.ArcGIS.Mapping.Builder.Resources.Strings.CannotAddTool, MessageBoxButton.OK);
                        return false;
                    }
                }
            }

            // As long as the from item is a tree view item (a tool) we can add. The to item can be either
            // a tree view (root node) or a tool itself that we will add just after.
            if (fromItem.Parent is TreeViewItem)
                return true;

            ESRIControls.MessageBoxDialog.Show(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ChooseAvailableTool, ESRI.ArcGIS.Mapping.Builder.Resources.Strings.CannotAddTool, MessageBoxButton.OK);
            return false;
        }

        private void GetParentAndPosition(TreeViewItem item, out TreeViewItem itemParent, out int insertPosition, out bool expandParent)
        {
            itemParent = null;
            insertPosition = 0;
            expandParent = false;

            if (item == null)
                return;

            // If the parent of this item is the tree view control itself, then return the item itself so something
            // can be added to this toolbar directly. Do not change the inserted position value from 0 as this
            // indicates "insert at top of children" which is correct for this situation.
            if (item.Parent is TreeView)
            {
                itemParent = item;
                expandParent = true;
                return;
            }

            // Get parent item and if that parent is a tree view, then the item is an immediate child of the treeview
            // control and thus a valid element.
            if (item.Parent != null)
            {
                TreeViewItem parent = item.Parent as TreeViewItem;
                if (parent.Parent is TreeView)
                {
                    insertPosition = parent.Items.IndexOf(item) + 1;
                    itemParent = parent;
                    return;
                }
            }
        }

        private void enableDisableReorderButtonBasedOnSelectedItem(TreeViewItem selectedItem)
        {
            // If the parent of this tree view item is a group button, then both move up and move down should be
            // enabled in order to allow the user to move the item out of the group and into the collection of
            // root level child elements of the toolbar.
            TreeViewItem parent = selectedItem.Parent as TreeViewItem;
            if (parent != null)
            {
                if (IsGroupButton(parent))
                {
                    btnMoveUp.IsEnabled = btnMoveDown.IsEnabled = true;
                    return;
                }
            }

            // check based on position
            int pos = (selectedItem.Parent as TreeViewItem).Items.IndexOf(selectedItem);
            int totalCount = (selectedItem.Parent as TreeViewItem).Items.Count;
            btnMoveUp.IsEnabled = pos > 0;
            btnMoveDown.IsEnabled = pos > -1 && (pos < totalCount - 1);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            BuilderApplication.Instance.HideWindow(this);
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            BuilderApplication.Instance.ShowWindow(ESRI.ArcGIS.Mapping.Builder.Resources.Strings.ExtensionNotifications, ESRIControls.NotificationPanel.Instance);
        }

        private void AvailableToolsControl_DoubleClick(object sender, EventArgs e)
        {
            // If the currently selected item to add from the Available Tools tree is actually a tool (and not a root
            // node) then proceed with performing the add operation. Do not add if the parent is a root node as this
            // is how the user can expand/collapse the node in the tree (double click) and should not be considered
            // and attempt by the user to add the parent node as a tool.
            TreeViewItem addToolItem = AvailableToolsControl.AddItemTree.SelectedItem as TreeViewItem;
            if (addToolItem.Parent is TreeViewItem)
                PerformAddOperation();
        }

        internal void Refresh()
        {
            if (AvailableToolsControl != null)
                AvailableToolsControl.Refresh();
        }
    }

    internal static class MouseButtonHelper
    {
        private const long k_DoubleClickSpeed = 500;
        private const double k_MaxMoveDistance = 10;

        private static long _LastClickTicks = 0;
        private static Point _LastPosition;
        private static WeakReference _LastSender;

        internal static bool IsDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(null);
            long clickTicks = DateTime.Now.Ticks;
            long elapsedTicks = clickTicks - _LastClickTicks;
            long elapsedTime = elapsedTicks / TimeSpan.TicksPerMillisecond;
            bool quickClick = (elapsedTime <= k_DoubleClickSpeed);
            bool senderMatch = (_LastSender != null && sender.Equals(_LastSender.Target));

            if (senderMatch && quickClick && position.Distance(_LastPosition) <= k_MaxMoveDistance)
            {
                // Double click!
                _LastClickTicks = 0;
                _LastSender = null;
                return true;
            }

            // Not a double click
            _LastClickTicks = clickTicks;
            _LastPosition = position;
            if (!quickClick)
                _LastSender = new WeakReference(sender);
            return false;
        }

        private static double Distance(this Point pointA, Point pointB)
        {
            double x = pointA.X - pointB.X;
            double y = pointA.Y - pointB.Y;
            return Math.Sqrt(x * x + y * y);
        }
    }
}
