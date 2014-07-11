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

namespace ESRI.ArcGIS.Mapping.Controls
{    
    public class RibbonButton : Button
    {
        private const string PART_LayoutGrid = "LayoutGrid";
        private const string PART_LabelTextElement = "LabelTextElement";

        private Grid LayoutGrid;
        private TextBlock LabelTextElement;

        public RibbonButton()
        {
            this.DefaultStyleKey = typeof(RibbonButton);            
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            LayoutGrid = GetTemplateChild(PART_LayoutGrid) as Grid;
            if (LayoutGrid != null)
            {
                if (Orientation == System.Windows.Controls.Orientation.Vertical)
                {
                    LayoutGrid.RowDefinitions.Add(new RowDefinition(){ Height = new GridLength(0, GridUnitType.Auto)});
                    LayoutGrid.RowDefinitions.Add(new RowDefinition(){ Height = new GridLength(0, GridUnitType.Auto)});
                }
                else
                {
                    LayoutGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
                    LayoutGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(0, GridUnitType.Auto) });
                }
            }

            LabelTextElement = GetTemplateChild(PART_LabelTextElement) as TextBlock;
            if (LabelTextElement != null)
            {
                if (Orientation == System.Windows.Controls.Orientation.Vertical)
                {
                    LabelTextElement.SetValue(Grid.RowProperty, 1);
                }
                else
                {
                    LabelTextElement.SetValue(Grid.ColumnProperty, 1);
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
                typeof(RibbonButton),
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
                typeof(RibbonButton),
                new PropertyMetadata(null));
        #endregion

        #region LabelTextWidth
        /// <summary>
        /// 
        /// </summary>
        public double LabelTextWidth
        {
            get { return (double)GetValue(LabelTextWidthProperty); }
            set { SetValue(LabelTextWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the LabelTextWidth dependency property.
        /// </summary>
        public static readonly DependencyProperty LabelTextWidthProperty =
            DependencyProperty.Register(
                "LabelTextWidth",
                typeof(double),
                typeof(RibbonButton),
                new PropertyMetadata(double.NaN));
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
                typeof(RibbonButton),
                new PropertyMetadata(Orientation.Vertical));
        #endregion
    }
}
