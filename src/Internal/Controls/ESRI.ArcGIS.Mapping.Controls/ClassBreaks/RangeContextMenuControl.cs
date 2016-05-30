/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = "RangeContextMenu", Type = typeof(ContentControl))]
    [TemplatePart(Name = "ItemHighlight", Type = typeof(ContentControl))]
    [TemplatePart(Name = "AddRangeButton", Type = typeof(Button))]
    [TemplatePart(Name = "DeleteRangeButton", Type = typeof(Button))]
    [TemplatePart(Name = "ConfigureRangeButton", Type = typeof(Button))]
    [TemplatePart(Name = "MinText", Type = typeof(TextBlock))]
    [TemplatePart(Name = "MaxText", Type = typeof(TextBlock))]
    [TemplatePart(Name = "MenuGrid", Type = typeof(Grid))]
    public partial class RangeContextMenuControl : Control
    {
        ContentControl RangeContextMenu = null;
        ContentControl ItemHighlight = null;
        Button AddRangeButton = null;
        Button DeleteRangeButton = null;
        Button ConfigureRangeButton = null;
        TextBlock MinText = null;
        TextBlock MaxText = null;
        Grid MenuGrid = null;        

        public RangeContextMenuControl()
        {
            DefaultStyleKey = typeof(RangeContextMenuControl);
        }

        public void SetRanges(double minValue, double maxValue)
        {
            if (MinText != null)
                MinText.Text = minValue.ToString("N2");
            if (MaxText != null)
                MaxText.Text = maxValue.ToString("N2");
        }

        public void HighlightRow(int rowNumber)
        {
            colorText(Colors.White, rowNumber);
        }
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            RangeContextMenu = GetTemplateChild("RangeContextMenu") as ContentControl;
            if (RangeContextMenu != null)
            {
                RangeContextMenu.MouseEnter += new MouseEventHandler(contextMenu_MouseEnter);
                RangeContextMenu.MouseLeave += new MouseEventHandler(contextMenu_MouseLeave);
            }

            ItemHighlight = GetTemplateChild("ItemHighlight") as ContentControl;

            AddRangeButton = GetTemplateChild("AddRangeButton") as Button;
            if (AddRangeButton != null)
            {
                AddRangeButton.Click += new RoutedEventHandler(AddRangeButton_Click);
                AddRangeButton.MouseEnter += new MouseEventHandler(MenuItem_MouseEnter);
                AddRangeButton.MouseLeave += new MouseEventHandler(MenuItem_MouseLeave);
            }

            DeleteRangeButton = GetTemplateChild("DeleteRangeButton") as Button;
            if (ConfigureRangeButton != null)
            {
                DeleteRangeButton.Click += new RoutedEventHandler(DeleteRangeButton_Click);
                DeleteRangeButton.MouseEnter += new MouseEventHandler(MenuItem_MouseEnter);
                DeleteRangeButton.MouseLeave += new MouseEventHandler(MenuItem_MouseLeave);
            }

            ConfigureRangeButton = GetTemplateChild("ConfigureRangeButton") as Button;
            if (ConfigureRangeButton != null)
            {
                ConfigureRangeButton.Click += new RoutedEventHandler(ConfigureRangeButton_Click);
                ConfigureRangeButton.MouseEnter += new MouseEventHandler(MenuItem_MouseEnter);
                ConfigureRangeButton.MouseLeave += new MouseEventHandler(MenuItem_MouseLeave);
            }

            MinText = GetTemplateChild("MinText") as TextBlock;
            MaxText = GetTemplateChild("MaxText") as TextBlock;

        }

        void ConfigureRangeButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement elem = sender as FrameworkElement;
            if (elem != null)
                colorText(Colors.White, Grid.GetRow(elem));
            if (ItemHighlight != null)
                ItemHighlight.Visibility = Visibility.Collapsed;

            if (ConfigureRangeClick != null)
                ConfigureRangeClick(this, e);
        }

        void DeleteRangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemHighlight != null)
                ItemHighlight.Visibility = Visibility.Collapsed;
            FrameworkElement elem = sender as FrameworkElement;
            if (elem != null)
                colorText(Colors.White, Grid.GetRow(elem));

            if (DeleteRangeClick != null)
                DeleteRangeClick(this, e);
        }        

        void AddRangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ItemHighlight != null)
                ItemHighlight.Visibility = Visibility.Collapsed;
            FrameworkElement elem = sender as FrameworkElement;
            if (elem != null)
                colorText(Colors.White, Grid.GetRow(elem));

            if (AddRangeClick != null)
                AddRangeClick(this, e);
        }        

        private void contextMenu_MouseEnter(object sender, MouseEventArgs e)
        {
            if (ContextMenuMouseEnter != null)
                ContextMenuMouseEnter(this, e);
        }

        private void contextMenu_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ContextMenuMouseLeave != null)
                ContextMenuMouseLeave(this, e);
        }

        private void MenuItem_MouseEnter(object sender, MouseEventArgs e)
        {
            FrameworkElement elem = sender as FrameworkElement;
            if (elem == null)
                return;
            int row = Grid.GetRow(elem);
            if (ItemHighlight != null)
            {
                Grid.SetRow(ItemHighlight, row);
                ItemHighlight.Visibility = Visibility.Visible;
            }

            if (MenuItemMouseEnter != null)
                MenuItemMouseEnter(sender, e);

            //colorText(Colors.Black, row);
        }

        private void MenuItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ItemHighlight != null)
                ItemHighlight.Visibility = Visibility.Collapsed;

            if (MenuItemMouseLeave != null)
                MenuItemMouseLeave(sender, e);
            //FrameworkElement elem = sender as FrameworkElement;
            //if (elem == null)
            //    return;
            //colorText(Colors.White, Grid.GetRow(elem));            
        }

        private void colorText(Color textColor, int row)
        {
            if (MenuGrid != null)
            {
#if SILVERLIGHT
                TextBlock text = (from element in MenuGrid.Children
                                  where Grid.GetRow(element as FrameworkElement) == row && element is TextBlock
                                  select element).First() as TextBlock;
                if (text != null)
                    text.Foreground = new SolidColorBrush(textColor);
#endif
            }
        }

        internal event RoutedEventHandler AddRangeClick;
        internal event RoutedEventHandler DeleteRangeClick;
        internal event RoutedEventHandler ConfigureRangeClick;
        internal event MouseEventHandler ContextMenuMouseEnter;
        internal event MouseEventHandler ContextMenuMouseLeave;
        internal event MouseEventHandler MenuItemMouseEnter;
        internal event MouseEventHandler MenuItemMouseLeave;
    }
}
