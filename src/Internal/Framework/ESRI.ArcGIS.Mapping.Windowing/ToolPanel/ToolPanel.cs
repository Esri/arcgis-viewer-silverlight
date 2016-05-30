/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Windows.Input;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.Application.Controls.Toolbars;

namespace ESRI.ArcGIS.Client.Application.Controls
{
    public class ToolPanel : Control, INotifyPropertyChanged
    {
        private const string PART_TOOLPANEL_ITEMS_CONTROL = "ToolPanelItemsControl";
        internal ItemsControl ToolPanelItemsControl;

        // Contains references to all instantiated objects (Commands, DropDownFrameworkElements) on the toolpanel
        // This include internal and external Exports
        private List<object> toolItems;
        private bool isPopupToolPanel = false;

        public ToolPanel()
        {
            DefaultStyleKey = typeof(ToolPanel);
            ToolPanelItems = new ObservableCollection<FrameworkElement>();
            toolItems = new List<object>();

            // Bind the stand-in DependencyProperty to the real DataContext
            SetBinding(ContextProperty, new Binding());
        }

        #region Properties

        #region All Buttons
        private List<ButtonBase> _allButtons;
        public List<ButtonBase> AllButtons
        {
            get
            {
                if (_allButtons == null)
                    _allButtons = new List<ButtonBase>();
                return _allButtons;
            }
        }
        #endregion

        #region Orientation
        /// <summary>
        /// 
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Identifies the Orientation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(Orientation),
                typeof(ToolPanel),
                new PropertyMetadata(Orientation.Horizontal, OnOrientationPropertyChanged));

        /// <summary>
        /// OrientationProperty property changed handler.
        /// </summary>
        /// <param name="d">ToolPanel that changed its Orientation.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolPanel source = d as ToolPanel;
            source.OnOrientationChanged();
        }
        #endregion

        #region ContainerName
        public string ContainerName { get; set; }
        #endregion

        #region ToolPanelElements
        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<FrameworkElement> ToolPanelItems
        {
            get { return GetValue(ToolPanelItemsProperty) as ObservableCollection<FrameworkElement>; }
            set { SetValue(ToolPanelItemsProperty, value); }
        }

        /// <summary>
        /// Identifies the ToolPanelButtons dependency property.
        /// </summary>
        public static readonly DependencyProperty ToolPanelItemsProperty =
            DependencyProperty.Register(
                "ToolPanelItems",
                typeof(ObservableCollection<FrameworkElement>),
                typeof(ToolPanel),
                new PropertyMetadata(null));
        #endregion

        #region CanSerialize
        private bool _canSerialize = true;
        public bool CanSerialize
        {
            get
            {
                return _canSerialize;
            }
            set
            {
                _canSerialize = value;
            }
        }
        #endregion

        //#region Styles

        //public Style MenuButtonStyle
        //{
        //    get { return (Style)GetValue(MenuButtonStyleProperty); }
        //    set { SetValue(MenuButtonStyleProperty, value); }
        //}
        //public static readonly DependencyProperty MenuButtonStyleProperty =
        //    DependencyProperty.Register(
        //        "MenuButtonStyle",
        //        typeof(Style),
        //        typeof(ToolPanel),
        //        new PropertyMetadata(null));

        //public Style MenuContainerStyle
        //{
        //    get { return (Style)GetValue(MenuContainerStyleProperty); }
        //    set { SetValue(MenuContainerStyleProperty, value); }
        //}
        //public static readonly DependencyProperty MenuContainerStyleProperty =
        //    DependencyProperty.Register(
        //        "MenuContainerStyle",
        //        typeof(Style),
        //        typeof(ToolPanel),
        //        new PropertyMetadata(null));

        //public Style ToolButtonStyle
        //{
        //    get { return (Style)GetValue(ToolButtonStyleProperty); }
        //    set { SetValue(ToolButtonStyleProperty, value); }
        //}
        //public static readonly DependencyProperty ToolButtonStyleProperty =
        //    DependencyProperty.Register(
        //        "ToolButtonStyle",
        //        typeof(Style),
        //        typeof(ToolPanel),
        //        new PropertyMetadata(null));

        //public Style ToolToggleButtonStyle
        //{
        //    get { return (Style)GetValue(ToolToggleButtonStyleProperty); }
        //    set { SetValue(ToolToggleButtonStyleProperty, value); }
        //}
        //public static readonly DependencyProperty ToolToggleButtonStyleProperty =
        //    DependencyProperty.Register(
        //        "ToolToggleButtonStyle",
        //        typeof(Style),
        //        typeof(ToolPanel),
        //        new PropertyMetadata(null));

        //public Style GroupContainerStyle
        //{
        //    get { return (Style)GetValue(GroupContainerStyleProperty); }
        //    set { SetValue(GroupContainerStyleProperty, value); }
        //}
        //public static readonly DependencyProperty GroupContainerStyleProperty =
        //    DependencyProperty.Register(
        //        "GroupContainerStyle",
        //        typeof(Style),
        //        typeof(ToolPanel),
        //        new PropertyMetadata(null));

        //public Style MenuItemStyle
        //{
        //    get { return (Style)GetValue(MenuItemStyleProperty); }
        //    set { SetValue(MenuItemStyleProperty, value); }
        //}
        //public static readonly DependencyProperty MenuItemStyleProperty =
        //    DependencyProperty.Register(
        //        "MenuItemStyle",
        //        typeof(Style),
        //        typeof(ToolPanel),
        //        new PropertyMetadata(null));

        //public Style MenuItemToggleButtonStyle
        //{
        //    get { return (Style)GetValue(MenuItemToggleButtonStyleProperty); }
        //    set { SetValue(MenuItemToggleButtonStyleProperty, value); }
        //}
        //public static readonly DependencyProperty MenuItemToggleButtonStyleProperty =
        //    DependencyProperty.Register(
        //        "MenuItemToggleButtonStyle",
        //        typeof(Style),
        //        typeof(ToolPanel),
        //        new PropertyMetadata(null));

        //public Style ToolSeparatorStyle
        //{
        //    get { return (Style)GetValue(ToolSeparatorStyleProperty); }
        //    set { SetValue(ToolSeparatorStyleProperty, value); }
        //}
        //public static readonly DependencyProperty ToolSeparatorStyleProperty =
        //    DependencyProperty.Register(
        //        "ToolSeparatorStyle",
        //        typeof(Style),
        //        typeof(ToolPanel),
        //        new PropertyMetadata(null));

        //public Style MenuItemSeparatorStyle
        //{
        //    get { return (Style)GetValue(MenuItemSeparatorStyleProperty); }
        //    set { SetValue(MenuItemSeparatorStyleProperty, value); }
        //}
        //public static readonly DependencyProperty MenuItemSeparatorStyleProperty =
        //    DependencyProperty.Register(
        //        "MenuItemSeparatorStyle",
        //        typeof(Style),
        //        typeof(ToolPanel),
        //        new PropertyMetadata(null));

        //public Style PopupLeaderStyle
        //{
        //    get { return (Style)GetValue(PopupLeaderStyleProperty); }
        //    set { SetValue(PopupLeaderStyleProperty, value); }
        //}
        //public static readonly DependencyProperty PopupLeaderStyleProperty =
        //    DependencyProperty.Register(
        //        "PopupLeaderStyle",
        //        typeof(Style),
        //        typeof(ToolPanel),
        //        new PropertyMetadata(null));

        //public Style PopupContentControlStyle
        //{
        //    get { return (Style)GetValue(PopupContentControlStyleProperty); }
        //    set { SetValue(PopupContentControlStyleProperty, value); }
        //}
        //public static readonly DependencyProperty PopupContentControlStyleProperty =
        //    DependencyProperty.Register(
        //        "PopupContentControlStyle",
        //        typeof(Style),
        //        typeof(ToolPanel),
        //        new PropertyMetadata(null));

        //public Style LayerConfigContextMenuStyle
        //{
        //    get { return (Style)GetValue(LayerConfigContextMenuStyleProperty); }
        //    set { SetValue(LayerConfigContextMenuStyleProperty, value); }
        //}
        //public static readonly DependencyProperty LayerConfigContextMenuStyleProperty =
        //    DependencyProperty.Register(
        //        "LayerConfigContextMenuStyle",
        //        typeof(Style),
        //        typeof(ToolPanel),
        //        new PropertyMetadata(null));

        //#endregion

        #endregion

        internal IEnumerable<object> GetToolItemObjects()
        {
            return toolItems;
        }

        private void OnOrientationChanged()
        {
            assignOrientationToItemPanel();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ToolPanelItemsControl = GetTemplateChild(PART_TOOLPANEL_ITEMS_CONTROL) as ItemsControl;

            assignOrientationToItemPanel();
        }

        private void assignOrientationToItemPanel()
        {
            if (ToolPanelItemsControl != null)
                ToolPanelItemsControl.ItemsPanel = XamlReader.Load(string.Format(@"<ItemsPanelTemplate xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""><StackPanel Orientation=""{0}"" /></ItemsPanelTemplate>", (Orientation == Orientation.Vertical ? "Vertical" : "Horizontal"))) as ItemsPanelTemplate;
        }

        #region Serialization

        public void PopulateToolPanelFromXml(XElement rootElement)
        {
            try
            {
                if (rootElement.HasAttributes)
                {
                    XAttribute att = rootElement.Attribute(Constants.TOOLPANEL_ORIENTATION);
                    if (att != null && string.Compare(att.Value, Orientation.Vertical.ToString(), StringComparison.InvariantCultureIgnoreCase) == 0)
                        this.Orientation = Orientation.Vertical;

                    att = rootElement.Attribute(Constants.TOOLPANEL_CONTAINER_NAME);
                    if (att != null)
                        this.ContainerName = att.Value;

                    att = rootElement.Attribute(Constants.TOOLPANEL_NAME);
                    if (att != null)
                        this.Name = att.Value;
                    this.isPopupToolPanel = this.ContainerName.ToLower().StartsWith("popup", StringComparison.Ordinal);
                }
                if (rootElement.HasElements)
                {
                    XElement tools = rootElement.Element(Constants.TOOLS);
                    if (tools != null && tools.HasElements)
                    {
                        foreach (XElement childNode in tools.Elements())
                        {
                            if (Constants.SEPARATOR.Equals(childNode.Name.LocalName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                this.AddToolPanelSeparatorElement(this.ToolPanelItems, true);
                            }
                            else if (Constants.GROUP.Equals(childNode.Name.LocalName, StringComparison.InvariantCultureIgnoreCase))
                            {

                                ButtonDisplayInfo displayInfo = getButtonDisplayInfoForXmlNode(childNode);
                                HeaderedItemsControl toolContainer = new HeaderedItemsControl()
                                {
                                    Header = displayInfo,
                                    DataContext = displayInfo,
                                    Style = ToolPanelLayoutStyleHelper.Instance.GetStyle("GroupContainerStyle"),
                                };
                                this.ToolPanelItems.Add(toolContainer);

                                if (childNode.HasElements)
                                {
                                    ObservableCollection<FrameworkElement> toolItems = new ObservableCollection<FrameworkElement>();
                                    foreach (XElement element in childNode.Elements())
                                    {
                                        if (Constants.MENU.Equals(element.Name.LocalName, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            this.parseMenu(element, toolItems);
                                        }
                                        else if (Constants.TOOL.Equals(element.Name.LocalName, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            string path = !string.IsNullOrEmpty(this.Name) ? this.Name : "";
                                            if (!string.IsNullOrEmpty(displayInfo.Label))
                                                path = path + "->" + displayInfo.Label;
                                            this.parseTool(element, toolItems, path);
                                        }
                                        else if (Constants.SEPARATOR.Equals(element.Name.LocalName, StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            this.AddToolPanelSeparatorElement(toolItems, true);
                                        }
                                    }
                                    toolContainer.ItemsSource = toolItems;
                                }
                            }
                            else if (Constants.MENU.Equals(childNode.Name.LocalName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                this.parseMenu(childNode, this.ToolPanelItems);
                            }
                            else if (Constants.TOOL.Equals(childNode.Name.LocalName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                this.parseTool(childNode, this.ToolPanelItems, !string.IsNullOrEmpty(this.Name) ? this.Name : "");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError(ex);
            }
        }

        public XElement GetToolPanelXml(List<XNamespace> namespaces, out List<XAttribute> additionalAttributes)
        {
            additionalAttributes = null;
            try
            {
                if (this.ToolPanelItems != null)
                {
                    XElement toolPanelElement = new XElement(Constants.TOOLPANEL);

                    XElement toolsElement = new XElement(Constants.TOOLS);
                    toolPanelElement.Add(toolsElement);

                    //add attributes
                    addAttributesForToolPanel(toolPanelElement, this);

                    //add tools
                    foreach (FrameworkElement element in this.ToolPanelItems)
                    {
                        HeaderedItemsControl headeredItemsControl = element as HeaderedItemsControl;
                        if (headeredItemsControl != null)
                        {
                            XElement toolContainerElement = new XElement(Constants.GROUP);
                            IList<FrameworkElement> toolContainerElements = headeredItemsControl.ItemsSource as IList<FrameworkElement>;
                            if (toolContainerElement != null)
                            {
                                foreach (FrameworkElement elem in toolContainerElements)
                                {
                                    addXElementForFrameworkElement(namespaces, out additionalAttributes, toolContainerElement, elem);
                                }
                            }
                            toolsElement.Add(toolContainerElement);
                        }
                        else
                        {
                            addXElementForFrameworkElement(namespaces, out additionalAttributes, toolsElement, element);
                        }
                    }
                    return toolPanelElement;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogError(ex);
            }

            return null;
        }
        #endregion

        #region Create toolPanel logic

        #region Tool Events
        private void command_CanExecuteChanged(object sender, EventArgs e)
        {
            this.Refresh();

            ICommand command = sender as ICommand;
            if (command != null)
                OnToolStateChanged(command);
        }
        private void nestedToolNode_Click(object sender, RoutedEventArgs e)
        {
            ButtonBase button = sender as ButtonBase;
            if (button == null)
                return;
            DropDownButton dropDownButton = button.Tag as DropDownButton;
            if (dropDownButton == null)
                return;

            dropDownButton.IsContentPopupOpen = false;
        }
        private void groupNodeButton_Click(object sender, RoutedEventArgs e)
        {
            ButtonBase button = sender as ButtonBase;
            if (button == null)
                return;
            DropDownButton dropDownButton = button.Parent as DropDownButton;
            if (dropDownButton == null)
                return;

            dropDownButton.IsContentPopupOpen = !dropDownButton.IsContentPopupOpen;
        }

        protected virtual void OnToolClassLoadException(CoreExceptionEventArgs args)
        {
            EventHandler<CoreExceptionEventArgs> handler = ToolClassLoadException;
            if (handler != null)
                handler(this, args);
        }
        public event EventHandler<CoreExceptionEventArgs> ToolClassLoadException;

        protected virtual void OnToolClassSaveConfigurationException(CoreExceptionEventArgs args)
        {
            EventHandler<CoreExceptionEventArgs> handler = ToolClassSaveConfigurationException;
            if (handler != null)
                handler(this, args);
        }
        public event EventHandler<CoreExceptionEventArgs> ToolClassSaveConfigurationException;

        protected virtual void OnToolClassLoadConfigurationException(CoreExceptionEventArgs args)
        {
            EventHandler<CoreExceptionEventArgs> handler = ToolClassLoadConfigurationException;
            if (handler != null)
                handler(this, args);
        }
        public event EventHandler<CoreExceptionEventArgs> ToolClassLoadConfigurationException;

        protected virtual void OnToolStateChanged(ICommand command)
        {
            if (ToolStateChanged != null)
                ToolStateChanged(this, new CommandEventArgs() { Command = command });
        }

        public event EventHandler<CommandEventArgs> ToolStateChanged;
        #endregion

        #region Public

        public ButtonBase AddToolButton(ButtonDisplayInfo buttonDisplayInfo, ICommand buttonCommand,
            IList<FrameworkElement> parentContainer, int position = -1)
        {
            string toolToggleButtonStyle = isPopupToolPanel ? "PopupToolToggleButtonStyle" : "ToolToggleButtonStyle";
            string toolButtonStyle = isPopupToolPanel ? "PopupToolButtonStyle" : "ToolButtonStyle";

            bool isToggleCommand = buttonCommand is IToggleCommand;
            ButtonBase btn = createButton(buttonDisplayInfo, buttonCommand, isToggleCommand ? ToolPanelLayoutStyleHelper.Instance.GetStyle(toolToggleButtonStyle) : ToolPanelLayoutStyleHelper.Instance.GetStyle(toolButtonStyle), isToggleCommand);
            if (position < 0 || (parentContainer != null && position >= parentContainer.Count))
            {
                if(parentContainer != null)
                    parentContainer.Add(btn);
                if (buttonCommand != null)
                    toolItems.Add(buttonCommand);
            }
            else
            {
                if (parentContainer != null)
                    parentContainer.Insert(position, btn);
                if (buttonCommand != null)
                    toolItems.Insert(position, buttonCommand);
            }

            // Coded UI support.
            btn.Content = buttonDisplayInfo.Label;

            return btn;
        }

        public ButtonBase AddToolGroupButton(ButtonDisplayInfo buttonDisplayInfo, ICommand buttonCommand, IList<FrameworkElement> parentContainer, int position = -1)
        {
            string menuButtonStyle = isPopupToolPanel ? "PopupMenuButtonStyle" : "MenuButtonStyle";
            ButtonBase btn = createButton(buttonDisplayInfo, buttonCommand, ToolPanelLayoutStyleHelper.Instance.GetStyle(menuButtonStyle), false);
            btn.Click += groupNodeButton_Click;

            StackPanel stackPanel = new StackPanel();
            DropDownButton dropDownButton = new DropDownButton()
            {
                Content = btn,
                PopupContent = stackPanel,
                PopupContentContainerStyle = ToolPanelLayoutStyleHelper.Instance.GetStyle(PopupStyleName.PopupContentControl.ToString()),
                PopupLeaderStyle = ToolPanelLayoutStyleHelper.Instance.GetStyle(PopupStyleName.PopupLeader.ToString()),
            };
            if (position < 0 || position >= parentContainer.Count)
            {
                parentContainer.Add(dropDownButton);
                if (buttonCommand != null)
                    toolItems.Add(buttonCommand);
            }
            else
            {
                parentContainer.Insert(position, dropDownButton);
                if (buttonCommand != null)
                    toolItems.Insert(position, buttonCommand);
            }

            return dropDownButton;
        }

        public void AddNestedToolButton(ButtonDisplayInfo buttonDisplayInfo, ICommand buttonCommand,
            ButtonBase parentButton, int position = -1)
        {
            DropDownButton parentDropDownButton = parentButton as DropDownButton;
            if (parentDropDownButton != null)
            {
                Panel panel = parentDropDownButton.PopupContent as Panel;
                if (panel != null)
                {
                    bool isToggleCommand = buttonCommand is IToggleCommand;
                    ButtonBase btn = createButton(buttonDisplayInfo, buttonCommand, isToggleCommand ? ToolPanelLayoutStyleHelper.Instance.GetStyle("MenuItemToggleButtonStyle") : ToolPanelLayoutStyleHelper.Instance.GetStyle("MenuItemStyle"), isToggleCommand);
                    btn.Click += nestedToolNode_Click;
                    btn.Tag = parentDropDownButton;

                    if (position < 0 || position >= panel.Children.Count)
                    {
                        panel.Children.Add(btn);
                        if (buttonCommand != null)
                            toolItems.Add(buttonCommand);
                    }
                    else
                    {
                        panel.Children.Insert(position, btn);
                        if (buttonCommand != null)
                            toolItems.Insert(position, buttonCommand);
                    }
                }
            }
        }

        public ContentControl AddNestedSeparatorElement(ButtonBase parentButton, bool isTopLevel, int position = -1)
        {
            ContentControl elem = null;

            DropDownButton parentDropDownButton = parentButton as DropDownButton;
            if (parentDropDownButton != null)
            {
                Panel panel = parentDropDownButton.PopupContent as Panel;
                if (panel != null)
                {
                    elem = new ContentControl()
                    {
                        Style = isTopLevel ? ToolPanelLayoutStyleHelper.Instance.GetStyle("ToolSeparatorStyle") : ToolPanelLayoutStyleHelper.Instance.GetStyle("MenuItemSeparatorStyle"),
                    };

                    if (position < 0 || position >= panel.Children.Count)
                    {
                        if (panel.Children != null)
                            panel.Children.Add(elem);
                    }
                    else
                    {
                        if (panel.Children != null)
                            panel.Children.Insert(position, elem);
                    }
                }
            }

            return elem;
        }

        public ContentControl AddToolPanelSeparatorElement(IList<FrameworkElement> parentContainer, bool isTopLevel,
            int position = -1)
        {
            ContentControl elem = new ContentControl()
            {
                Style = isTopLevel ? ToolPanelLayoutStyleHelper.Instance.GetStyle("ToolSeparatorStyle") : ToolPanelLayoutStyleHelper.Instance.GetStyle("MenuItemSeparatorStyle"),
            };

            if (position < 0 || position >= parentContainer.Count)
            {
                if (parentContainer != null)
                    parentContainer.Add(elem);
            }
            else
            {
                if (parentContainer != null)
                    parentContainer.Insert(position, elem);
            }

            return elem;
        }

        public void RemoveToolbarSeparatorElement(ContentControl elem, IList<FrameworkElement> parentContainer)
        {
            if (parentContainer != null)
                parentContainer.Remove(elem);
        }

        public void RemoveToolButton(ButtonBase button, IList<FrameworkElement> parentContainer)
        {
            if (button == null)
                return;
            if (parentContainer != null)
                parentContainer.Remove(button);
            ICommand buttonCommand = button.Command;
            if (buttonCommand != null)
                toolItems.Remove(buttonCommand);
            DropDownButton dropDownButton = button as DropDownButton;
            if (dropDownButton != null)
            {
                dropDownButton.Click -= groupNodeButton_Click;
                Panel panel = dropDownButton.PopupContent as Panel;
                if (panel != null)
                {
                    foreach (UIElement uiElem in panel.Children)
                    {
                        ButtonBase btn = uiElem as ButtonBase;
                        if (btn == null)
                            continue;
                        btn.Click -= nestedToolNode_Click;
                        buttonCommand = btn.Command;
                        if (buttonCommand != null)
                            toolItems.Remove(buttonCommand);
                    }
                    panel.Children.Clear();
                }
            }
        }

        public void RemoveNestedToolButton(ButtonBase button, ButtonBase parentButton)
        {
            if (button == null)
                return;
            ICommand buttonCommand = button.Command;
            if (buttonCommand != null)
                toolItems.Remove(buttonCommand);
            DropDownButton parentDropDownButton = parentButton as DropDownButton;
            if (parentDropDownButton != null)
            {
                Panel panel = parentDropDownButton.PopupContent as Panel;
                if (panel != null)
                {
                    button.Click -= nestedToolNode_Click;
                    panel.Children.Remove(button);
                }
            }
        }

        /// <summary>
        /// Refresh the state of the tools on the toolPanel.  Fires the underlying commands' CanExecute methods.
        /// </summary>
        public void Refresh()
        {
            if (AllButtons == null)
                return;

            foreach (ButtonBase button in AllButtons)
            {
                ICommand buttonCommand = button.Command;
                if (buttonCommand == null)
                    continue;

                // Re-assign the command to the button - doing so will cause the CanExecute to fire and the button be enabled/disabled appropriately.
                button.Command = null;
                button.Command = buttonCommand;

                IToggleCommand toggleCommand = buttonCommand as IToggleCommand;
                if (toggleCommand != null)
                {
                    ToggleButton toggleButton = button as ToggleButton;
                    if (toggleButton != null)
                        toggleButton.IsChecked = toggleCommand.IsChecked();
                }
            }
        }

        #endregion

        #region Private

        #region Context (DataContext notification workaround)

        /// <summary>
        /// This property is here as a workaround because binding to the DataContext for some
        /// reason never fires a propertyChanged event.  Instead, if we create a stand-in property
        /// that is bound to the DataContext, the Tools can be bound to this property and receive
        /// notifications when the DataContext changes.
        /// </summary>
        protected static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(object), typeof(ToolPanel),
                new PropertyMetadata((object)null,
                    new PropertyChangedCallback(OnContextChanged)));

        protected object Context
        {
            get { return (object)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        protected static void OnContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToolPanel target = (ToolPanel)d;
            object oldMyDataContext = (object)e.OldValue;
            object newMyDataContext = target.Context;
            target.OnContextChanged(oldMyDataContext, newMyDataContext);
        }

        protected virtual void OnContextChanged(object oldMyDataContext, object newMyDataContext)
        {
            OnClickPopupInfo newPopupInfo = newMyDataContext as OnClickPopupInfo;
            if (newPopupInfo != null)
            {
                OnClickPopupInfo oldPopupInfo = oldMyDataContext as OnClickPopupInfo;
                foreach (var btn in AllButtons)
                {
                    btn.CommandParameter = newPopupInfo;
                }
            }
        }

        #endregion
        
        private ButtonBase createButton(ButtonDisplayInfo buttonDisplayInfo, ICommand buttonCommand, Style buttonStyle, bool isToggleButton)
        {
            ButtonBase btn = null;
            try
            {
                if (isToggleButton)
                {
                    btn = new ToggleButton();
                    btn.Click += OnToggleButtonClicked;
                    IToggleCommand toggleCommand = buttonCommand as IToggleCommand;
                    if (toggleCommand != null)
                    {
                        ToggleButton toggleButton = btn as ToggleButton;
                        if (toggleButton != null)
                            toggleButton.IsChecked = toggleCommand.IsChecked();
                    }
                }
                else
                    btn = new Button();

                btn.Style = buttonStyle;
                btn.DataContext = buttonDisplayInfo;
                btn.Command = buttonCommand;
                btn.CommandParameter = Context;

                if (buttonCommand != null)
                    buttonCommand.CanExecuteChanged += command_CanExecuteChanged;
                AllButtons.Add(btn);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return btn;
        }

        void OnToggleButtonClicked(object sender, RoutedEventArgs e)
        {
            // make sure the toggle button state matches the command setting
            ToggleButton btn = sender as ToggleButton;
            if (btn != null)
            {
                IToggleCommand toggleCommand = btn.Command as IToggleCommand;
                if (toggleCommand != null)
                {
                    bool isChecked = toggleCommand.IsChecked();
                    if (btn.IsChecked != isChecked)
                        btn.IsChecked = isChecked;
                }
            }
        }

        private void getLabelAndDescription(Type t, out string label, out string description)
        {
            label = description = null;
            // Get all custom attributes associated with this type but do not gather those that might be obtained
            // via inheritance.
            object[] attrs = t.GetCustomAttributes(false);

            // Process each attribute, looking for the ones we care about and if found, extract information and continue
            // to the next attribute.
            foreach (object att in attrs)
            {
                if (!string.IsNullOrEmpty(label) && !string.IsNullOrEmpty(description))
                    break;
                ESRI.ArcGIS.Client.Extensibility.DisplayNameAttribute nameAttribute = att as ESRI.ArcGIS.Client.Extensibility.DisplayNameAttribute;
                if (nameAttribute != null)
                {
                    label = nameAttribute.Name;
                    continue;
                }

                ESRI.ArcGIS.Client.Extensibility.DescriptionAttribute descAttribute = att as ESRI.ArcGIS.Client.Extensibility.DescriptionAttribute;
                if (descAttribute != null)
                {
                    description = descAttribute.Description;
                    continue;
                }
            }
        }

        private void parseTool(XElement childNode, IList<FrameworkElement> parentContainer, string path)
        {
            ButtonDisplayInfo info = getButtonDisplayInfoForXmlNode(childNode);
            if (info != null)
            {
                object elementImplementation = getToolImplementationForXmlNode(childNode, info.Label, !string.IsNullOrEmpty(path) ? path + "->" + info.Label : info.Label);
                ICommand elementCommand = elementImplementation as ICommand;
                if (elementCommand != null)
                {
                    if (string.IsNullOrEmpty(info.Description) || string.IsNullOrEmpty(info.Label))
                    {
                        string label, description;
                        getLabelAndDescription(elementCommand.GetType(), out label, out description);
                        if (string.IsNullOrEmpty(info.Label))
                            info.Label = label;
                        if (string.IsNullOrEmpty(info.Description))
                            info.Description = description;
                    }
                    AddToolButton(info, elementCommand, parentContainer);
                }
                else
                {
                    FrameworkElement frameworkElement = elementImplementation as FrameworkElement;
                    if (frameworkElement != null)
                    {
                        DropDownButton galleryButton = AddToolGroupButton(info, null, parentContainer) as DropDownButton;
                        if (galleryButton != null)
                        {
                            Panel sp = galleryButton.PopupContent as Panel;
                            if (sp != null)
                                sp.Children.Add(frameworkElement);
                        }
                    }
                }
            }
        }

        private void parseMenu(XElement childNode, IList<FrameworkElement> parentContainer)
        {
            ButtonDisplayInfo groupInfo = getButtonDisplayInfoForXmlNode(childNode);
            if (groupInfo != null)
            {
                ButtonBase toolGroupButton = AddToolGroupButton(groupInfo, getToolImplementationForXmlNode(childNode, groupInfo.Label, groupInfo.Label) as ICommand, parentContainer);

                if (childNode.HasElements)
                {
                    foreach (XElement element in childNode.Elements())
                    {
                        if (Constants.TOOL.Equals(element.Name.LocalName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            ButtonDisplayInfo info = getButtonDisplayInfoForXmlNode(element);
                            if (info != null)
                            {
                                object elementImplementation = getToolImplementationForXmlNode(element, info.Label, info.Label);
                                ICommand elementCommand = elementImplementation as ICommand;
                                if (elementCommand != null)
                                    AddNestedToolButton(info, elementCommand, toolGroupButton);
                            }
                        }
                        else if (Constants.SEPARATOR.Equals(element.Name.LocalName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            DropDownButton dropDownBtn = toolGroupButton as DropDownButton;
                            if (dropDownBtn != null)
                            {
                                Panel panel = dropDownBtn.PopupContent as Panel;
                                if (panel != null)
                                {
                                    ContentControl elem = new ContentControl()
                                    {
                                        Style = ToolPanelLayoutStyleHelper.Instance.GetStyle("MenuItemSeparatorStyle"),
                                    };
                                    panel.Children.Add(elem);
                                }
                            }
                        }
                    }
                }
            }
        }

        private object getToolImplementationForXmlNode(XElement childNode, string toolName, string toolPath)
        {
            object classImplementation = null;
            if (childNode.HasElements)
            {
                XElement classNode = childNode.Element(Constants.TOOL_CLASS);
                if (classNode != null && classNode.HasElements)
                {
                    XElement implementationNode = classNode.FirstNode as XElement;
                    if (implementationNode != null)
                    {
                        try
                        {
                            string configData = null;
                            classImplementation = XamlReader.Load(implementationNode.ToString());
                            ISupportsConfiguration supportsConfiguration = classImplementation as ISupportsConfiguration;
                            if (supportsConfiguration != null)
                            {
                                XElement configDataNode = childNode.Element(Constants.TOOL_CONFIGDATA);
                                if (configDataNode != null && configDataNode.FirstNode != null)
                                {
                                    XNode configNode = configDataNode.FirstNode;
                                    if (configNode != null)
                                    {
                                        XText configNodeAsText = configNode as XText;
                                        if (configNodeAsText != null)
                                        {
                                            configData = configNodeAsText.Value;
                                        }
                                        else
                                            configData = configNode.ToString();
                                    }
                                }

                                try
                                {
                                    supportsConfiguration.LoadConfiguration(configData);
                                }
                                catch (Exception innerEx)
                                {
                                    // Error in Load Configuration of command
                                    Logger.Instance.LogError(innerEx);
                                    OnToolClassLoadConfigurationException(new CoreExceptionEventArgs(new Exception(string.Format("Failed to load configuration of tool \"{0}\" (Path: {1}).", toolName, toolPath), innerEx), null));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.LogError(ex);
                            OnToolClassLoadException(new CoreExceptionEventArgs(new Exception(string.Format("Failed to instantiate tool \"{0}\" (Path: {1}).", toolName, toolPath), ex), null));
                        }
                    }
                }
            }
            return classImplementation;
        }

        private XElement toolControlToXmlElement(FrameworkElement frameworkElement, List<XNamespace> namespaces, out XAttribute additionalTagPrefix)
        {
            additionalTagPrefix = null;
            if (frameworkElement == null)
                return null;

            // handle the case of separator
            ContentControl contentCtrl = frameworkElement as ContentControl;
            if (contentCtrl != null && contentCtrl.Content == null && contentCtrl.ContentTemplate == null
                && (contentCtrl.Style == ToolPanelLayoutStyleHelper.Instance.GetStyle("ToolSeparatorStyle") || contentCtrl.Style == ToolPanelLayoutStyleHelper.Instance.GetStyle("MenuItemSeparatorStyle")))
            {
                return new XElement(Constants.SEPARATOR);
            }

            XElement toolElement = new XElement(Constants.TOOL);
            ButtonDisplayInfo toolButtonDisplayInfo = frameworkElement.DataContext as ButtonDisplayInfo;
            if (toolButtonDisplayInfo != null)
                setButtonDisplayAttributesOnElement(toolButtonDisplayInfo, toolElement);

            object toolImplementation = frameworkElement;

            // if it is a button, we actually want it command
            ButtonBase btnBase = frameworkElement as ButtonBase;
            if (btnBase != null)
                toolImplementation = btnBase.Command;

            if (toolImplementation != null)
            {
                XElement toolCommandElement = new XElement(Constants.TOOL_CLASS);
                toolElement.Add(toolCommandElement);

                Type toolType = toolImplementation.GetType();
                XmlnsDefinitionAttribute defnAttribute = null;
                object[] attribs = toolType.Assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), false);
                if (attribs != null && attribs.Length > 0)
                    defnAttribute = attribs[0] as XmlnsDefinitionAttribute;

                string toolNamespace = toolType.Namespace;
                string namespaceMapping = null;
                if (defnAttribute != null)
                    namespaceMapping = defnAttribute.XmlNamespace;
                else
                    namespaceMapping = string.Format("clr-namespace:{0};assembly={1}", toolNamespace, toolType.Assembly.FullName.Split(',')[0]);

                XNamespace namespaceForCommand = namespaces.FirstOrDefault<XNamespace>(x => x.NamespaceName == namespaceMapping);
                if (namespaceForCommand == null)
                {
                    namespaceForCommand = namespaceMapping;
                    string tagPrefix = toolNamespace.Replace('.', '_');
                    additionalTagPrefix = new XAttribute(XNamespace.Xmlns + tagPrefix, namespaceMapping);
                }

                XElement toolCommandImplElement = new XElement(namespaceForCommand + toolType.Name);
                toolCommandElement.Add(toolCommandImplElement);

                ISupportsConfiguration supportsConfiguration = toolImplementation as ISupportsConfiguration;
                if (supportsConfiguration != null)
                {
                    string configData = null;
                    try
                    {
                        configData = supportsConfiguration.SaveConfiguration();
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.LogError(ex);
                        OnToolClassSaveConfigurationException(new CoreExceptionEventArgs(ex, null));
                    }
                    if (!string.IsNullOrWhiteSpace(configData))
                    {
                        configData = configData.Trim();
                        XElement configDataElement = new XElement(Constants.TOOL_CONFIGDATA);
                        if (configData.StartsWith("<", StringComparison.Ordinal) && configData.EndsWith(">", StringComparison.Ordinal))
                        {
                            try
                            {
                                XDocument xDoc = XDocument.Parse(configData);
                                configDataElement.Add(xDoc.Root);
                            }
                            catch
                            {
                                configDataElement.Value = configData;
                            }
                        }
                        else
                        {
                            configDataElement.Value = configData;
                        }
                        toolElement.Add(configDataElement);
                    }
                }
            }
            return toolElement;
        }

        private void addXElementForFrameworkElement(List<XNamespace> namespaces, out List<XAttribute> additionalAttributes, XElement parentElement, FrameworkElement element)
        {
            additionalAttributes = null;
            DropDownButton toolGroupDropDownButton = element as DropDownButton;
            if (toolGroupDropDownButton != null)
            {
                ButtonBase toolGroupButton = toolGroupDropDownButton.Content as ButtonBase;
                if (toolGroupButton != null)
                {
                    Panel panel = toolGroupDropDownButton.PopupContent as Panel;
                    if (panel != null)
                    {
                        // Custom elements are things such as BaseMap gallery
                        bool isDropDownWithCustomElement = panel.Children.Count == 1 && !(panel.Children[0] is ButtonBase);
                        if (isDropDownWithCustomElement)
                        {
                            XElement toolElement = addXElementForTool(namespaces, out additionalAttributes, parentElement, panel.Children[0] as Control);
                            if (toolElement != null)
                            {
                                ButtonDisplayInfo toolGroupButtonDisplayInfo = toolGroupButton.DataContext as ButtonDisplayInfo;
                                if (toolGroupButtonDisplayInfo != null)
                                    setButtonDisplayAttributesOnElement(toolGroupButtonDisplayInfo, toolElement);
                            }
                        }
                        else
                        {
                            XElement toolGroupElement = new XElement(Constants.MENU);
                            ButtonDisplayInfo toolGroupButtonDisplayInfo = toolGroupButton.DataContext as ButtonDisplayInfo;
                            if (toolGroupButtonDisplayInfo != null)
                                setButtonDisplayAttributesOnElement(toolGroupButtonDisplayInfo, toolGroupElement);

                            foreach (UIElement elem in panel.Children)
                            {
                                FrameworkElement control = elem as FrameworkElement;
                                if (control == null)
                                    continue;

                                addXElementForTool(namespaces, out additionalAttributes, toolGroupElement, control);
                            }
                            if (parentElement != null)
                                parentElement.Add(toolGroupElement);
                        }
                    }
                }
            }
            else
            {
                ButtonBase button = element as ButtonBase;
                if (button != null)
                {
                    addXElementForTool(namespaces, out additionalAttributes, parentElement, button);
                }
                else
                {
                    ContentControl ctrl = element as ContentControl;
                    if (ctrl != null)
                    {
                        if (parentElement != null)
                            parentElement.Add(new XElement(Constants.SEPARATOR));
                    }
                }
            }
        }

        private XElement addXElementForTool(List<XNamespace> namespaces, out List<XAttribute> additionalAttributes, XElement parentElement, FrameworkElement element)
        {
            additionalAttributes = null;
            XAttribute additionalTagPrefix = null;
            XElement toolElement = toolControlToXmlElement(element, namespaces, out additionalTagPrefix);
            if (parentElement != null)
                parentElement.Add(toolElement);
            if (additionalTagPrefix != null)
            {
                if (additionalAttributes == null)
                    additionalAttributes = new List<XAttribute>();

                additionalAttributes.Add(additionalTagPrefix);
            }
            return toolElement;
        }

        #region Helper Functions
        private static void addAttributesForToolPanel(XElement toolPanelElement, ToolPanel toolPanel)
        {
            if (toolPanel != null && toolPanelElement != null)
            {
                if (!string.IsNullOrEmpty(toolPanel.ContainerName))
                    toolPanelElement.Add(new XAttribute(Constants.TOOLPANEL_CONTAINER_NAME, toolPanel.ContainerName));
                if (!string.IsNullOrEmpty(toolPanel.Name))
                    toolPanelElement.Add(new XAttribute(Constants.TOOLPANEL_NAME, toolPanel.Name));
                if (toolPanel.Orientation != Orientation.Horizontal)
                    toolPanelElement.Add(new XAttribute(Constants.TOOLPANEL_ORIENTATION, toolPanel.Orientation.ToString()));
            }
        }

        private static string getAttributeValue(XElement elem, string attributeName)
        {
            return elem.Attribute(attributeName) != null ? elem.Attribute(attributeName).Value : null;
        }

        private static ButtonDisplayInfo getButtonDisplayInfoForXmlNode(XElement childNode)
        {
            string labelText = getAttributeValue(childNode, Constants.LABEL) ?? "";
            string icon = getAttributeValue(childNode, Constants.ICON) ?? "";
            string tooltipText = getAttributeValue(childNode, Constants.DESCRIPTION) ?? "";
            ButtonDisplayInfo displayInfo = new ButtonDisplayInfo() { Label = labelText, Icon = icon, Description = tooltipText };
            return displayInfo;
        }

        private static void setButtonDisplayAttributesOnElement(ButtonDisplayInfo buttonDisplayInfo, XElement element)
        {
            if (element == null)
                return;
            if (!string.IsNullOrWhiteSpace(buttonDisplayInfo.Label))
                element.SetAttributeValue(Constants.LABEL, buttonDisplayInfo.Label);
            if (!string.IsNullOrWhiteSpace(buttonDisplayInfo.Icon))
                element.SetAttributeValue(Constants.ICON, buttonDisplayInfo.Icon);
            if (!string.IsNullOrWhiteSpace(buttonDisplayInfo.Description))
                element.SetAttributeValue(Constants.DESCRIPTION, buttonDisplayInfo.Description);
        }
        #endregion

        #endregion

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


    }
}
