/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
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
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = PART_SplitElementContainerGrid, Type = typeof(Grid))]
    [TemplatePart(Name = PART_ArrowElement, Type = typeof(Panel))]
    [TemplatePart(Name = PART_LabelTextElement, Type = typeof(TextBlock))]
    public class RibbonDropDownButton : DropDownButton
    {
        private const string PART_SplitElementContainerGrid = "SplitElementContainerGrid";
        private const string PART_ArrowElement = "ArrowElement";
        private const string PART_LabelTextElement = "LabelTextElement";
        private Grid SplitElementContainerGrid;
        private ContentControl ArrowElement;
        private TextBlock LabelTextElement;

        public RibbonDropDownButton()
        {
            this.DefaultStyleKey = typeof(RibbonDropDownButton);            
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            double widthOfArrowElement = 12;
            double widthOfLabelText = Width;
            if (DisplayArrowBesideText)
                widthOfLabelText = (!double.IsNaN(Width) ? (Width - widthOfArrowElement - 4) : 32);


            SplitElementContainerGrid = GetTemplateChild(PART_SplitElementContainerGrid) as Grid;
            if (SplitElementContainerGrid != null)
            {
                if (DisplayArrowBesideText)
                {
                    SplitElementContainerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(widthOfLabelText) });
                    SplitElementContainerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(widthOfArrowElement) });
                }
                else
                {
                    SplitElementContainerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
                    SplitElementContainerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0, GridUnitType.Auto) });
                }
            }

            LabelTextElement = GetTemplateChild(PART_LabelTextElement) as TextBlock;
            if (LabelTextElement != null)
            {
                if (DisplayArrowBesideText)
                    LabelTextElement.Width = widthOfLabelText;
                else
                    LabelTextElement.Width = Width;
            }

            ArrowElement = GetTemplateChild(PART_ArrowElement) as ContentControl;
            if (ArrowElement != null)
            {
                if (DisplayArrowBesideText)
                {
                    ArrowElement.Width = widthOfArrowElement;
                    ArrowElement.SetValue(Grid.ColumnProperty, 1);
                    ArrowElement.Margin = new Thickness(0, 5, 5, 0);
                }
                else
                {
                    ArrowElement.Width = Width;
                    ArrowElement.SetValue(Grid.RowProperty, 1);
                }
            }
        }

        #region ImageSource
        /// <summary>
        /// 
        /// </summary>
        public ImageSource ImageSource
        {
            get { return GetValue(ImageSourceProperty) as ImageSource; }
            set { SetValue(ImageSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the ImageSource dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(
                "ImageSource",
                typeof(ImageSource),
                typeof(RibbonDropDownButton),
                new PropertyMetadata(null));
        #endregion

        #region LabelText
        /// <summary>
        /// 
        /// </summary>
        public string LabelText
        {
            get { return GetValue(LabelTextProperty) as string; }
            set { SetValue(LabelTextProperty, value); }
        }

        /// <summary>
        /// Identifies the LabelText dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register(
                "LabelText",
                typeof(string),
                typeof(RibbonDropDownButton),
                new PropertyMetadata(null));
        #endregion

        #region DisplayArrowNextToImage
        /// <summary>
        /// 
        /// </summary>
        public bool DisplayArrowBesideText
        {
            get { return (bool)GetValue(DisplayArrowNextToImageProperty); }
            set { SetValue(DisplayArrowNextToImageProperty, value); }
        }

        /// <summary>
        /// Identifies the DisplayArrowNextToImage dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayArrowNextToImageProperty =
            DependencyProperty.Register(
                "DisplayArrowNextToImage",
                typeof(bool),
                typeof(RibbonDropDownButton),
                new PropertyMetadata(false));
        #endregion
    }
}
