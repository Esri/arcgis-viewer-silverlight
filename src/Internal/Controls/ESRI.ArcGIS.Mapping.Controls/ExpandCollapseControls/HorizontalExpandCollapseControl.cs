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
    [TemplatePart(Name=PART_EXPANDSIDEPANELBUTTON, Type=typeof(Button))]
    [TemplatePart(Name = PART_COLLAPSESIDEPANELBUTTON, Type = typeof(Button))]
    public class HorizontalExpandCollapseControl : Control
    {
        private const string PART_EXPANDSIDEPANELBUTTON = "ExpandSidePanelButton";
        private const string PART_COLLAPSESIDEPANELBUTTON = "CollapseSidePanelButton";

        Button ExpandSidePanelButton;
        Button CollapseSidePanelButton;

        public HorizontalExpandCollapseControl()
        {
            DefaultStyleKey = typeof(HorizontalExpandCollapseControl);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if(ExpandSidePanelButton != null)
                ExpandSidePanelButton.Click -= ExpandSidePanelButton_Click;

            ExpandSidePanelButton = GetTemplateChild(PART_EXPANDSIDEPANELBUTTON) as Button;
            if (ExpandSidePanelButton != null)
                ExpandSidePanelButton.Click += ExpandSidePanelButton_Click;


            if (CollapseSidePanelButton != null)
                CollapseSidePanelButton.Click -= CollapseSidePanelButton_Click;

            CollapseSidePanelButton = GetTemplateChild(PART_COLLAPSESIDEPANELBUTTON) as Button;

            if(CollapseSidePanelButton != null)
                CollapseSidePanelButton.Click += CollapseSidePanelButton_Click;
        }

        void CollapseSidePanelButton_Click(object sender, RoutedEventArgs e)
        {
            Collapse();
        }

        void ExpandSidePanelButton_Click(object sender, RoutedEventArgs e)
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
                typeof(HorizontalExpandCollapseControl),
                new PropertyMetadata(null));
        #endregion public FrameworkElement TargetFrameworkElement        

        #region public ColumnDefinition TargetColumnDefinition
        /// <summary>
        /// 
        /// </summary>
        public ColumnDefinition TargetColumnDefinition
        {
            get { return GetValue(TargetColumnDefinitionProperty) as ColumnDefinition; }
            set { SetValue(TargetColumnDefinitionProperty, value); }
        }

        /// <summary>
        /// Identifies the TargetColumnDefinition dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetColumnDefinitionProperty =
            DependencyProperty.Register(
                "TargetColumnDefinition",
                typeof(ColumnDefinition),
                typeof(HorizontalExpandCollapseControl),
                new PropertyMetadata(null));
        #endregion public ColumnDefinition TargetColumnDefinition
                        
        public void Expand()
        {
            FrameworkElement fe = TargetFrameworkElement;
            if (fe == null)
                return;

            ColumnDefinition cDef = TargetColumnDefinition;
            if (cDef == null)
                return;            

            double targetWidth = ColumnDefinitionBinder.GetTargetWidth(cDef);
            targetWidth = targetWidth > 0 ? targetWidth : 255;

            double from = fe.Width;
            if (double.IsNaN(from))
                from = 0;

            fe.Visibility = Visibility.Visible;

            Storyboard sb = new Storyboard();
            sb.Duration = new Duration(TimeSpan.FromSeconds(0.65));
            DoubleAnimation de = new DoubleAnimation();
            Storyboard.SetTargetProperty(de, new PropertyPath(ColumnDefinitionBinder.ColumnWidthProperty));
            de.From = from;
            de.To = targetWidth;
            de.Duration = sb.Duration;
            de.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseOut, Power = 3.0 };

			sb.Children.Add(de);

            Storyboard.SetTarget(de, cDef);

            sb.Completed += (o, e) =>
            {
                cDef.Width = new GridLength(targetWidth);
                cDef.MinWidth = 235;
            };
            
            sb.Begin();  
        }

        public void Collapse()
        {
            FrameworkElement fe = TargetFrameworkElement;
            if (fe == null)
                return;

            ColumnDefinition cDef = TargetColumnDefinition;
            if (cDef == null)
                return;
            
            double targetWidth = 0;
            double from = fe.Width;
            if (double.IsNaN(from))
            {
                from = cDef.Width.Value;                
            }

            Storyboard sb = new Storyboard();
            sb.Duration = new Duration(TimeSpan.FromSeconds(0.65));

            DoubleAnimation de = new DoubleAnimation();            
            Storyboard.SetTargetProperty(de, new PropertyPath(ColumnDefinitionBinder.ColumnWidthProperty));
            de.To = targetWidth;
            de.From = from;
            de.Duration = sb.Duration;
            de.EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseIn, Power = 3.0 };
			sb.Children.Add(de);
            Storyboard.SetTarget(de, fe);

            Storyboard.SetTarget(de, cDef);
            cDef.MinWidth = 0;
            ColumnDefinitionBinder.SetTargetWidth(cDef, cDef.Width.Value);

            sb.Completed += (o, e) =>
            {
                cDef.Width = new GridLength(targetWidth);
            };

            ColumnDefinitionBinder.SetColumnWidth(cDef, targetWidth);
            sb.Completed += (o, e) =>
            {
                fe.Visibility = Visibility.Collapsed;
            };
            sb.Begin();     
        }
    }
}
