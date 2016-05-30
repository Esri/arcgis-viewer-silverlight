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

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name = PART_EXPANDBOTTOMPANELBUTTON, Type = typeof(Button))]
    [TemplatePart(Name = PART_COLLAPSEBOTTOMPANELBUTTON, Type = typeof(Button))]
    public class VerticalExpandCollapseControl : Control
    {
        private const string PART_EXPANDBOTTOMPANELBUTTON = "ExpandBottomPanelButton";
        private const string PART_COLLAPSEBOTTOMPANELBUTTON = "CollapseBottomPanelButton";

        Button ExpandBottomPanelButton;
        Button CollapseBottomPanelButton;

        public VerticalExpandCollapseControl()
        {
            DefaultStyleKey = typeof(VerticalExpandCollapseControl);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ExpandBottomPanelButton != null)
                ExpandBottomPanelButton.Click -= ExpandBottomPanelButton_Click;

            ExpandBottomPanelButton = GetTemplateChild(PART_EXPANDBOTTOMPANELBUTTON) as Button;
            if (ExpandBottomPanelButton != null)
                ExpandBottomPanelButton.Click += ExpandBottomPanelButton_Click;


            if (CollapseBottomPanelButton != null)
                CollapseBottomPanelButton.Click -= CollapseBottomPanelButton_Click;

            CollapseBottomPanelButton = GetTemplateChild(PART_COLLAPSEBOTTOMPANELBUTTON) as Button;

            if (CollapseBottomPanelButton != null)
                CollapseBottomPanelButton.Click += CollapseBottomPanelButton_Click;
        }

        void CollapseBottomPanelButton_Click(object sender, RoutedEventArgs e)
        {
            Collapse();
        }

        void ExpandBottomPanelButton_Click(object sender, RoutedEventArgs e)
        {
            Expand();
        }

        #region public FrameworkElement TargetFrameworkElement
        /// <summary>
        /// 
        /// </summary>
        public FrameworkElement TargetFrameworkElement
        {
            get { return GetValue(TargetFrameworkElementProperty) as FrameworkElement; }
            set { SetValue(TargetFrameworkElementProperty, value); }
        }

        /// <summary>
        /// Identifies the TargetFrameworkElement dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetFrameworkElementProperty =
            DependencyProperty.Register(
                "TargetFrameworkElement",
                typeof(FrameworkElement),
                typeof(VerticalExpandCollapseControl),
                new PropertyMetadata(null));
        #endregion public FrameworkElement TargetFrameworkElement
        
        #region public RowDefinition TargetRowDefinition
        /// <summary>
        /// 
        /// </summary>
        public RowDefinition TargetRowDefinition
        {
            get { return GetValue(TargetRowDefinitionProperty) as RowDefinition; }
            set { SetValue(TargetRowDefinitionProperty, value); }
        }

        /// <summary>
        /// Identifies the TargetRowDefinition dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetRowDefinitionProperty =
            DependencyProperty.Register(
                "TargetRowDefinition",
                typeof(RowDefinition),
                typeof(VerticalExpandCollapseControl),
                new PropertyMetadata(null));
        #endregion public RowDefinition TargetRowDefinition
        
        public void Expand()
        {
            FrameworkElement fe = TargetFrameworkElement;
            if (fe == null)
                return;

            RowDefinition rDef = TargetRowDefinition;
            if (rDef == null)
                return;

            double targetHeight = RowDefinitionBinder.GetTargetHeight(rDef);
            targetHeight = targetHeight > 0 ? targetHeight : 255;

            double from = fe.Height;
            if (double.IsNaN(from))
                from = 0;

            fe.Visibility = Visibility.Visible;

            Storyboard sb = new Storyboard();
            sb.Duration = new Duration(TimeSpan.FromSeconds(0.65));
            DoubleAnimation de = new DoubleAnimation();            
            Storyboard.SetTargetProperty(de, new PropertyPath(RowDefinitionBinder.RowHeightProperty));
            de.From = from;
            de.To = targetHeight;
            de.Duration = sb.Duration;
            de.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 3.0 };
			sb.Children.Add(de);

            Storyboard.SetTarget(de, rDef);

            sb.Completed += (o, e) =>
            {
                rDef.Height = new GridLength(targetHeight);
                rDef.MinHeight = 200;
            };
            
            sb.Begin();   
        }

        public void Collapse()
        {
            FrameworkElement fe = TargetFrameworkElement;
            if (fe == null)
                return;

            RowDefinition rDef = TargetRowDefinition;
            if (rDef == null)
                return;
            
            double targetHeight = 0;
            double from = fe.Height;
            if (double.IsNaN(from))
            {
                from = rDef.Height.Value;
            }

            Storyboard sb = new Storyboard();
            sb.Duration = new Duration(TimeSpan.FromSeconds(0.65));

            DoubleAnimation de = new DoubleAnimation();            
            Storyboard.SetTargetProperty(de, new PropertyPath(RowDefinitionBinder.RowHeightProperty));
            de.To = targetHeight;
            de.From = from;
            de.Duration = sb.Duration;
            de.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseIn, Power = 3.0 };
            sb.Children.Add(de);
            Storyboard.SetTarget(de, fe);
            
            Storyboard.SetTarget(de, rDef);
            rDef.MinHeight = 0;
            RowDefinitionBinder.SetTargetHeight(rDef, rDef.Height.Value);

            sb.Completed += (o, e) =>
            {
                rDef.Height = new GridLength(targetHeight);
            };

            RowDefinitionBinder.SetRowHeight(rDef, targetHeight);

            sb.Completed += (o, e) =>
            {
                fe.Visibility = Visibility.Collapsed;
            };
            sb.Begin();   
        }
    }
}
