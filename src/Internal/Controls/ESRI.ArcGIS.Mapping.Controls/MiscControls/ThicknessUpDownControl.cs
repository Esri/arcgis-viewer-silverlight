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
using System.Windows.Media.Imaging;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name=PART_INCREMENTBORDERTHICKNESSBUTTON, Type=typeof(Button))]
    [TemplatePart(Name = PART_DECREMENTBORDERTHICKNESSBUTTON, Type = typeof(Button))]
    public class ThicknessUpDownControl : Control
    {
        private const string PART_INCREMENTBORDERTHICKNESSBUTTON = "IncrementBorderThicknessButton";
        private const string PART_DECREMENTBORDERTHICKNESSBUTTON = "DecrementBorderThicknessButton";

        internal Button IncrementBorderThicknessButton;
        internal Button DecrementBorderThicknessButton;

        public ThicknessUpDownControl()
        {
            DefaultStyleKey = typeof(ThicknessUpDownControl);
        }

        #region public Thickness TargetThickness
        /// <summary>
        /// 
        /// </summary>
        public Thickness TargetThickness
        {
            get { return (Thickness)GetValue(TargetThicknessProperty); }
            set { SetValue(TargetThicknessProperty, value); }
        }

        /// <summary>
        /// Identifies the TargetThickness dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetThicknessProperty =
            DependencyProperty.Register(
                "TargetThickness",
                typeof(Thickness),
                typeof(ThicknessUpDownControl),
                new PropertyMetadata(new Thickness(0)));
        #endregion public Thickness TargetThickness
        
        public override void OnApplyTemplate()
        {
            if (DecrementBorderThicknessButton != null)
                DecrementBorderThicknessButton.Click -= DecrementBorderThicknessButton_Clicked;

            if (IncrementBorderThicknessButton != null)
                IncrementBorderThicknessButton.Click -= IncrementBorderThicknessButton_Clicked;
            
            base.OnApplyTemplate();

            IncrementBorderThicknessButton = GetTemplateChild(PART_INCREMENTBORDERTHICKNESSBUTTON) as Button;
            if (IncrementBorderThicknessButton != null)
                IncrementBorderThicknessButton.Click += IncrementBorderThicknessButton_Clicked;

            DecrementBorderThicknessButton = GetTemplateChild(PART_DECREMENTBORDERTHICKNESSBUTTON) as Button;
            if (DecrementBorderThicknessButton != null)
                DecrementBorderThicknessButton.Click += DecrementBorderThicknessButton_Clicked;

            if (InitCompleted != null)
                InitCompleted(this, EventArgs.Empty);
        }

        internal event EventHandler InitCompleted;

        void DecrementBorderThicknessButton_Clicked(object sender, EventArgs e)
        {
            double prevVal = TargetThickness.Bottom;
            if (prevVal < 1) // Dont' let values of thickness go below 1
                return;            
            TargetThickness = new Thickness(prevVal - 1);
            OnThicknessValueChanged(EventArgs.Empty);            
        }

        void IncrementBorderThicknessButton_Clicked(object sender, EventArgs e)
        {
            double prevVal = TargetThickness.Bottom;
            TargetThickness = new Thickness(prevVal + 1);
            OnThicknessValueChanged(EventArgs.Empty);
        }

        protected virtual void OnThicknessValueChanged(EventArgs args)
        {
            if(ThicknessValueChanged != null)
                ThicknessValueChanged(this, args);
        }

        public event EventHandler ThicknessValueChanged;

        #region Orienation
        /// <summary>
        /// 
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrienationProperty); }
            set { SetValue(OrienationProperty, value); }
        }

        /// <summary>
        /// Identifies the Orienation dependency property.
        /// </summary>
        public static readonly DependencyProperty OrienationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(Orientation),
                typeof(ThicknessUpDownControl),
                new PropertyMetadata(Orientation.Vertical));
        #endregion 

        #region DecreaseImage
        /// <summary>
        /// 
        /// </summary>
        public BitmapImage DecreaseImage
        {
            get { return GetValue(DecreaseImageProperty) as BitmapImage; }
            set { SetValue(DecreaseImageProperty, value); }
        }

        /// <summary>
        /// Identifies the DecreaseImage dependency property.
        /// </summary>
        public static readonly DependencyProperty DecreaseImageProperty =
            DependencyProperty.Register(
                "DecreaseImage",
                typeof(BitmapImage),
                typeof(ThicknessUpDownControl),
                new PropertyMetadata(null));
        #endregion 

        #region IncreaseImage
        /// <summary>
        /// 
        /// </summary>
        public BitmapImage IncreaseImage
        {
            get { return GetValue(IncreaseImageProperty) as BitmapImage; }
            set { SetValue(IncreaseImageProperty, value); }
        }

        /// <summary>
        /// Identifies the IncreaseImage dependency property.
        /// </summary>
        public static readonly DependencyProperty IncreaseImageProperty =
            DependencyProperty.Register(
                "IncreaseImage",
                typeof(BitmapImage),
                typeof(ThicknessUpDownControl),
                new PropertyMetadata(null));
        #endregion 

    }
}
