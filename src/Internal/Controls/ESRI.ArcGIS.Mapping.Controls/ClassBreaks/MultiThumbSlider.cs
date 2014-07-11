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
using System.Collections.Generic;
using System.Windows.Controls.Primitives;

namespace ESRI.ArcGIS.Mapping.Controls
{
    [TemplatePart(Name="CanvasRoot", Type=typeof(Canvas))]
    public partial class MultiThumbSlider : Control
    {
        Canvas CanvasRoot;
        private const double sliderRectangleThickness = 20;

        public MultiThumbSlider()
        {
            DefaultStyleKey = typeof(MultiThumbSlider);

            _sliders = new List<Slider>();
            _rectangles = new List<Rectangle>();

            this.ScaleThumbPositions = true;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            CanvasRoot = GetTemplateChild("CanvasRoot") as Canvas;            
        }

        private bool _rectanglesInitialized;

        private List<Slider> _sliders;
        public List<Slider> Sliders { get { return _sliders; } }

        private List<Rectangle> _rectangles;
        internal List<Rectangle> Rectangles { get { return _rectangles; } }

        public Slider AddSlider(Color rectangleColor)
        {
            Slider slider = new Slider()
            {
                Tag = _sliders.Count,
                Minimum = this.Minimum,
                Maximum = this.Maximum,
                Orientation = this.Orientation,                
                Cursor = Orientation == Orientation.Vertical ? Cursors.SizeNS : Cursors.SizeWE
            };
            Style sliderStyle = Application.Current.Resources["SliderStyle"] as Style;
            if (sliderStyle != null)
                slider.Style = sliderStyle;
            if (Orientation == Orientation.Vertical)
            {
                slider.Height = this.Height;
                slider.Width = 22;
            }
            else
            {
                if (_sliders.Count > 0)
                    slider.Width = _sliders[0].Width;
                //else if (_maxLabel != null)
                //    slider.Width = this.Width - _maxLabel.ActualWidth;
                else
                    slider.Width = this.Width;
                slider.Height = 26;
            }

            if (_sliders.Count == 0)
                slider.Value = (this.Minimum + this.Maximum) / 2;
            else
                slider.Value = (_sliders[_sliders.Count - 1].Value + this.Maximum) / 2;

            slider.MouseEnter += new MouseEventHandler(slider_MouseEnter);
            slider.MouseLeave += new MouseEventHandler(slider_MouseLeave);
            slider.MouseMove += new MouseEventHandler(slider_MouseMove);
            if (sliderTooltip == null)
            {
                sliderTooltip = new Popup();
                sliderTooltipControl = new SliderTooltipControl();
                sliderTooltip.Child = sliderTooltipControl;
                sliderTooltipControl.SetTooltipText(slider.Value.ToString("N2"));
            }

            _sliders.Add(slider);

            if(CanvasRoot != null)
                CanvasRoot.Children.Add(slider);

            addRectangle(rectangleColor);

            slider.ValueChanged += Slider_ValueChanged;
            return slider;
        }

        public void RemoveThumb(int index, bool leftRectangle)
        {
            if (index < _sliders.Count)
            {
                if (CanvasRoot != null)
                    CanvasRoot.Children.Remove(_sliders[index]);
                _sliders.Remove(_sliders[index]);
            }
            for (int i = index; i < _sliders.Count; i++)
                _sliders[i].Tag = i;

            if (leftRectangle)
            {
                if (index < _rectangles.Count)
                {
                    if (CanvasRoot != null)
                        CanvasRoot.Children.Remove(_rectangles[index]);
                    _rectangles.Remove(_rectangles[index]);
                }
                for (int i = index; i < _rectangles.Count; i++)
                    _rectangles[i].Tag = i;
            }
            else
            {
                if (index < _rectangles.Count - 1)
                {
                    if (CanvasRoot != null)
                        CanvasRoot.Children.Remove(_rectangles[index + 1]);
                    _rectangles.Remove(_rectangles[index + 1]);
                }
            }

            if (_sliders.Count > 0)
            {
                resizeRectangles();
            }
            else
            {
                if (_rectangles.Count > 0)
                {
                    if (CanvasRoot != null)
                        CanvasRoot.Children.Remove(_rectangles[0]);
                    _rectangles.Remove(_rectangles[0]);
                }
                _rectanglesInitialized = false;
            }
        }

        public void Reset()
        {
            while (_sliders.Count > 0)
                RemoveThumb(0, true);
        }

        private bool _allowValueOverlap = false;
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_allowValueOverlap)
            {
                Slider slider = (Slider)sender;
                int index = (int)slider.Tag;
                if (_sliders.Count == 1)
                {
                }
                else if ((index < _sliders.Count - 1) && (slider.Value >= _sliders[index + 1].Value))
                {
                    slider.Value = _sliders[index + 1].Value;
                }
                else if ((index > 0) && (slider.Value <= _sliders[index - 1].Value))
                {
                    slider.Value = _sliders[index - 1].Value;
                }
                if (sliderTooltipControl != null)
                {
                    sliderTooltipControl.SetTooltipText(slider.Value.ToString("N2"));
                }
            }

            resizeRectangles();
        }

        public bool ScaleThumbPositions { get; set; }

        private double _maximum;
        public double Maximum
        {
            get { return _maximum; }
            set
            {
                if (_maximum != value)
                {
                    _maximum = value;

                    _allowValueOverlap = true;
                    Slider s;
                    for (int i = 0; i < _sliders.Count; i++)
                    {
                        s = _sliders[i];
                        if (this.ScaleThumbPositions)
                        {
                            double ratio = (s.Value - s.Minimum) / (s.Maximum - s.Minimum);
                            s.Maximum = value;
                            if (s.Minimum == s.Maximum)
                                s.Minimum -= 0.1;
                            s.Value = double.IsNaN(ratio) ? value : (ratio * (s.Maximum - s.Minimum)) + s.Minimum;
                        }
                        else
                        {
                            s.Maximum = value;
                        }
                    }
                    _allowValueOverlap = false;

                    //if (_maxLabel != null)
                    //   _maxLabel.Text = _maximum.ToString("N2");
                }
            }
        }

        private double _minimum;
        public double Minimum
        {
            get { return _minimum; }
            set
            {
                if (_minimum != value)
                {
                    _minimum = value;

                    _allowValueOverlap = true;
                    Slider s;
                    for (int i = 0; i < _sliders.Count; i++)
                    {
                        s = _sliders[i];
                        if (this.ScaleThumbPositions)
                        {
                            double ratio = (s.Value - s.Minimum) / (s.Maximum - s.Minimum);
                            s.Minimum = value;
                            if (s.Minimum == s.Maximum)
                                s.Maximum += 0.1;
                            s.Value = double.IsNaN(ratio) ? value : (ratio * (s.Maximum - s.Minimum)) + s.Minimum;
                        }
                        else
                        {
                            s.Minimum = value;
                        }
                    }
                    _allowValueOverlap = false;

                    //if (_minLabel != null)
                    //  _minLabel.Text = _minimum.ToString("N2");
                }
            }
        }

        public Orientation Orientation { get; set; }

        private Color _defaultColor = Colors.LightGray;
        public Color DefaultColor
        {
            get { return _defaultColor; }
            set { _defaultColor = value; }
        }

        private void addRectangle(Color rectangleColor)
        {
            bool vertical = Orientation == Orientation.Vertical;
            Rectangle rectangle = new Rectangle()
            {
                Fill = new SolidColorBrush(rectangleColor),
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 1,
                Cursor = Cursors.Hand
            };

            if (selectedRectangle == null)
            {
                selectedRectangle = rectangle;
                selectedRectangle.StrokeThickness = 3;
            }
            if (vertical)
            {
                rectangle.Width = sliderRectangleThickness;
                rectangle.Margin = new Thickness(2, 0, 0, 0);
            }
            else
            {
                rectangle.Height = sliderRectangleThickness;
            }

            rectangle.Tag = _rectangles.Count;
            rectangle.MouseEnter += new MouseEventHandler(rectangle_MouseEnter);
            rectangle.MouseLeave += new MouseEventHandler(rectangle_MouseLeave);
            rectangle.MouseMove += new MouseEventHandler(rectangle_MouseMove);
            rectangle.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            rectangle.MouseLeftButtonUp += Element_MouseLeftButtonUp;

            if (!_rectanglesInitialized)
            {
                rectangle.Fill = new SolidColorBrush(DefaultColor);
                _rectangles.Add(rectangle);
                if(CanvasRoot != null)
                    CanvasRoot.Children.Insert(0, rectangle);

                rectangle = new Rectangle()
                {
                    Fill = new SolidColorBrush(rectangleColor),
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1,
                    Cursor = Cursors.Hand
                };
                //tooltip = new TextBlock() { FontSize = 11, Text = LocalizableStrings.SelectRangeTooltip };
                //ToolTipService.SetToolTip(rectangle, tooltip);            

                if (vertical)
                {
                    rectangle.Width = sliderRectangleThickness;
                    rectangle.Margin = new Thickness(2, 0, 0, 0);
                }
                else
                {
                    rectangle.Height = sliderRectangleThickness;
                }

                rectangle.Tag = _rectangles.Count;
                rectangle.MouseEnter += new MouseEventHandler(rectangle_MouseEnter);
                rectangle.MouseLeave += new MouseEventHandler(rectangle_MouseLeave);
                rectangle.MouseMove += new MouseEventHandler(rectangle_MouseMove);
                rectangle.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                rectangle.MouseLeftButtonUp += Element_MouseLeftButtonUp;

                _rectanglesInitialized = true;
            }

            _rectangles.Add(rectangle);
            if (CanvasRoot != null)
                CanvasRoot.Children.Insert(0, rectangle);

            resizeRectangles();
        }

        internal void UpdateBackgroundColorForSlider(int index, Color color)
        {
            if (index < 0 || index >= _rectangles.Count)
                return;
            if (_rectangles[index].Fill is SolidColorBrush)
                (_rectangles[index].Fill as SolidColorBrush).Color = color;
        }

        private FrameworkElement _mouseOverElement;
        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseOverElement = (FrameworkElement)sender;
        }

        Rectangle selectedRectangle;
        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            if (element == _mouseOverElement)
            {
                if (sender is Rectangle)
                {
                    if (selectedRectangle != null)
                    {
                        // Clear out previous highlight .. give it that appearance
                        // by resetting the stroke thickness to mornal
                        selectedRectangle.StrokeThickness = 1;
                    }
                    // highlight the selected rectangle by giving it a larger border
                    selectedRectangle = sender as Rectangle;
                    selectedRectangle.StrokeThickness = 3;
                    OnSliderClicked((Rectangle)sender);
                }
                //else if (sender is TextBlock)
                //    zoomIn(Convert.ToDouble(((TextBlock)sender).Text));
            }
        }

        private int _zoomLevel = 0;
        private void zoomIn(double zoomCenter)
        {
            _zoomLevel++;

            double zoomRange = ((this.Maximum - this.Minimum) / (_sliders.Count + 1)) / _zoomLevel;
            double zoomMin = zoomCenter - (zoomRange / 2);
            double zoomMax = zoomCenter + (zoomRange / 2);

            if (zoomMin < this.Minimum)
            {
                zoomMax += this.Minimum - zoomMin;
                zoomMin = this.Minimum;
            }
            else if (zoomMax > this.Maximum)
            {
                zoomMin -= zoomMax - this.Maximum;
                zoomMax = this.Maximum;
            }

            Slider slider = null;
            for (int i = 0; i < _sliders.Count; i++)
            {
                slider = _sliders[i];
                if (slider.Value < zoomMin)
                {
                    slider.Visibility = Visibility.Collapsed;
                    //_labels[i].Visibility = Visibility.Collapsed;
                    _rectangles[i].Visibility = Visibility.Collapsed;
                }
                else if (slider.Value > zoomMax)
                {
                    slider.Visibility = Visibility.Collapsed;
                    //_labels[i].Visibility = Visibility.Collapsed;
                    _rectangles[i + 1].Visibility = Visibility.Collapsed;
                }
                else
                {
                    slider.Minimum = zoomMin;
                    slider.Maximum = zoomMax;
                }
            }

            resizeRectangles();

            //_minLabel.Text = zoomMin.ToString("N2");
            //_maxLabel.Text = zoomMax.ToString("N2");

            OnZoomedIn();
        }

        public void ZoomOut()
        {
            Slider slider = null;
            for (int i = 0; i < _sliders.Count; i++)
            {
                slider = _sliders[i];
                slider.Visibility = Visibility.Visible;
                slider.Minimum = this.Minimum;
                slider.Maximum = this.Maximum;

                //_labels[i].Visibility = Visibility.Visible;
                _rectangles[i].Visibility = Visibility.Visible;
            }

            _rectangles[_rectangles.Count - 1].Visibility = Visibility.Visible;

            resizeRectangles();

            //_minLabel.Text = this.Minimum.ToString("N2");
            //_maxLabel.Text = this.Maximum.ToString("N2");

            _zoomLevel = 0;
        }

        private void resizeRectangles()
        {
            bool vertical = Orientation == Orientation.Vertical;
            double maxLabelHeight = updateLabels();
            if (!vertical)
                this.Height = maxLabelHeight + 37;

            double ratio = 0;

            double pos = Height; // vertical ? 0 : 0;            
            Rectangle rectangle = null;
            Slider slider = null;
            //TextBlock label = null;
            int lastVisibleIndex = 0;
            bool firstVisible = true;

            double width = _sliders[0].Width;

            for (int i = 0; i < _sliders.Count; i++)
            {
                slider = _sliders[i];

                if (slider.Visibility == Visibility.Collapsed)
                    continue;

                lastVisibleIndex = i;

                rectangle = _rectangles[i];
                //label = _labels[i];

                ratio = getSliderPosition(slider);
                if (!firstVisible)
                {
                    double previousSliderRatio = getSliderPosition(_sliders[i - 1]);
                    if (ratio - previousSliderRatio > 0)
                        ratio -= previousSliderRatio;
                }
                firstVisible = false;

                if (vertical)
                {
                    rectangle.Height = this.Height * ratio;
                    pos -= rectangle.Height;
                    Canvas.SetTop(rectangle, pos);
                    //Canvas.SetTop(label, pos);
                }
                else
                {
                    rectangle.Width = ratio < 0 ? 0 : width * ratio;
                    Canvas.SetLeft(rectangle, pos);
                    pos += rectangle.Width;

                    //Canvas.SetLeft(label, pos);
                    //Canvas.SetTop(label, maxLabelHeight + 8);
                    Canvas.SetTop(slider, maxLabelHeight + 18);
                    Canvas.SetTop(rectangle, maxLabelHeight + 21);
                }
            }

            rectangle = _rectangles[lastVisibleIndex + 1];
            if (vertical)
            {
                if (pos > 0)
                {
                    rectangle.Height = pos;
                    Canvas.SetTop(rectangle, 0);
                }
            }
            else
            {
                rectangle.Width = pos <= width ? width - pos : 0;
                Canvas.SetLeft(rectangle, pos);
                Canvas.SetTop(rectangle, maxLabelHeight + 21);
            }

            //if (vertical)
            //    Canvas.SetLeft(_minLabel, 39);
            //else
            //{
            //    Canvas.SetTop(_minLabel, maxLabelHeight + 8);
            //    Canvas.SetLeft(_minLabel, 0);
            //}
            //Canvas.SetTop(_maxLabel, maxLabelHeight + 8);
            //if (vertical)
            //    Canvas.SetLeft(_maxLabel, 39);                
            //else
            //    Canvas.SetLeft(_maxLabel, width);
        }

        private double updateLabels()
        {
            //TextBlock label = null;
            double maxLabelHeight = 0;
            //double labelHeight;
            //for (int i = 0; i < _sliders.Count; i++)
            //{
            //    label = _labels[i];
            //    label.Text = _sliders[i].Value.ToString("N2");

            //    labelHeight = label.ActualWidth / 2;
            //    if (labelHeight > maxLabelHeight)
            //        maxLabelHeight = labelHeight;
            //}

            return maxLabelHeight;
        }

        private static double getSliderPosition(Slider slider)
        {
            double pos = (slider.Value - slider.Minimum) / (slider.Maximum - slider.Minimum);
            if (Double.IsNaN(pos))
                return 0;
            else
                return pos;
        }

        #region Events
        public event EventHandler<BaseEventArgs> RangeContextMenuChanged;
        public event EventHandler ZoomedIn;
        public event EventHandler<SliderClickedEventArgs> SliderClicked;
        public event EventHandler<AddRangeCommandEventArgs> AddRangeCommand;
        public event EventHandler<DeleteRangeCommandEventArgs> DeleteRangeCommand;
        private void OnSliderClicked(Rectangle clickedRectangle)
        {
            if (SliderClicked != null)
                SliderClicked(this, getSliderClickedArgsForRectange(clickedRectangle));
        }
        private void OnZoomedIn()
        {
            if (ZoomedIn != null)
                ZoomedIn(this, EventArgs.Empty);
        }
        #endregion

        #region Helper Functions
        private SliderClickedEventArgs getSliderClickedArgsForRectange(Rectangle clickedRectangle)
        {
            int index = (int)clickedRectangle.Tag;
            return new SliderClickedEventArgs()
            {
                ClickedRectangle = clickedRectangle,
                LowSlider = index > 0 ? _sliders[index - 1] : null,
                HighSlider = index < _rectangles.Count - 1 ? _sliders[index] : null
            };
        }

        private AddRangeCommandEventArgs getAddRangeCommandArgsForRectangle(Rectangle clickedRectangle)
        {
            SliderClickedEventArgs args = getSliderClickedArgsForRectange(clickedRectangle);
            return new AddRangeCommandEventArgs()
            {
                ClickedRectangle = args.ClickedRectangle,
                LowSlider = args.LowSlider,
                HighSlider = args.HighSlider
            };
        }

        private DeleteRangeCommandEventArgs getDeleteRangeCommandArgsForRectangle(Rectangle clickedRectangle)
        {
            SliderClickedEventArgs args = getSliderClickedArgsForRectange(clickedRectangle);
            return new DeleteRangeCommandEventArgs()
            {
                ClickedRectangle = args.ClickedRectangle,
                LowSlider = args.LowSlider,
                HighSlider = args.HighSlider
            };
        }
        #endregion

        #region Internal
        internal void HighlightFirstBreak()
        {
            // Highlight the first one upon start
            if (_rectangles.Count > 0)
            {
                selectedRectangle = _rectangles[0];
                selectedRectangle.StrokeThickness = 3;
            }
        }

        internal void SelectSectionByIndex(int index)
        {
            if (selectedRectangle != null)
            {
                // Clear out previous highlight .. give it that appearance
                // by resetting the stroke thickness to mornal
                selectedRectangle.StrokeThickness = 1;
            }
            if (index > -1 && index < _rectangles.Count)
            {
                selectedRectangle = _rectangles[index];
            }

            selectedRectangle.StrokeThickness = 3;
            OnSliderClicked(selectedRectangle);
        }

        internal void SelectLastSection()
        {
            if (_rectangles != null && _rectangles.Count > 0)
                SelectSectionByIndex(_rectangles.Count - 1);
        }
        #endregion

        #region Tooltip Popup for Slider thumb
        Popup sliderTooltip;
        SliderTooltipControl sliderTooltipControl;
        double sliderTooltipHorizontalOffset;
        void slider_MouseLeave(object sender, MouseEventArgs e)
        {
            sliderTooltip.IsOpen = false;
        }

        void slider_MouseEnter(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(
#if SILVERLIGHT
                Application.Current.RootVisual
#else
                Application.Current.MainWindow
#endif
                );
            sliderTooltipHorizontalOffset = p.X;
            showSliderTooltip(p);
            Slider slider = sender as Slider;
            if (slider != null && sliderTooltipControl != null)
                sliderTooltipControl.SetTooltipText(slider.Value.ToString("N2"));
        }

        void slider_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(
#if SILVERLIGHT
                Application.Current.RootVisual
#else
                Application.Current.MainWindow
#endif
                );
            showSliderTooltip(p);
        }

        void showSliderTooltip(Point p)
        {
            if (sliderTooltip == null)
            {
                sliderTooltip = new Popup();
                sliderTooltipControl = new SliderTooltipControl();
                sliderTooltip.Child = sliderTooltip;
            }
            sliderTooltip.HorizontalOffset = sliderTooltipHorizontalOffset;
            sliderTooltip.VerticalOffset = p.Y;
            sliderTooltip.IsOpen = true;
        }
        #endregion

        #region Tooltip Popup for Range
        Popup rangeContextMenuPopup;
        RangeContextMenuControl rangeContextMenuControl;
        bool _isMouseOverContextMenu = false;
        double horizontalOffset;
        private void rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            Point mousePos = e.GetPosition(
#if SILVERLIGHT
                Application.Current.RootVisual
#else
                Application.Current.MainWindow
#endif
                );
            horizontalOffset = mousePos.X;
            showRangeContextMenu(sender as Rectangle, e);
        }

        private void showRangeContextMenu(Rectangle rectangle, MouseEventArgs e)
        {
            if (rectangle == null)
                return;
            Point mousePos = e.GetPosition(
#if SILVERLIGHT
                Application.Current.RootVisual
#else
                Application.Current.MainWindow
#endif
                );
            // Create a popup.
            if (rangeContextMenuPopup == null)
            {
                rangeContextMenuPopup = new Popup();
                rangeContextMenuControl = new RangeContextMenuControl();
                rangeContextMenuControl.AddRangeClick += new RoutedEventHandler(addRange_Click);
                rangeContextMenuControl.ConfigureRangeClick += new RoutedEventHandler(configureRange_Click);
                rangeContextMenuControl.DeleteRangeClick += new RoutedEventHandler(deleteRange_Click);
                rangeContextMenuControl.ContextMenuMouseEnter += new MouseEventHandler(contextMenu_MouseEnter);
                rangeContextMenuControl.ContextMenuMouseLeave += new MouseEventHandler(contextMenu_MouseLeave);
                rangeContextMenuPopup.Child = rangeContextMenuControl;
            }
            rangeContextMenuControl.Tag = rectangle;
            if (RangeContextMenuChanged != null)
                RangeContextMenuChanged(this, new BaseEventArgs { ClickedRectangle = rectangle });

            // Set where the popup will show up on the screen.
            rangeContextMenuPopup.VerticalOffset = mousePos.Y + 1;
            rangeContextMenuPopup.HorizontalOffset = horizontalOffset;

            // Open the popup.
            rangeContextMenuPopup.IsOpen = true;
        }


        internal void SetRangeContextMenuValues(double minValue, double maxValue)
        {
            if (rangeContextMenuControl != null)
                rangeContextMenuControl.SetRanges(minValue, maxValue);
        }

        void rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
            ThrottleTimer timer = new ThrottleTimer(200, delegate()
            {
                if (!_isMouseOverContextMenu)
                {
                    rangeContextMenuPopup.IsOpen = false;
                }
                timer = null;
            });
            timer.Invoke();
        }


        private void contextMenu_MouseLeave(object sender, MouseEventArgs e)
        {
            _isMouseOverContextMenu = false;
            rangeContextMenuPopup.IsOpen = false;
        }

        void rectangle_MouseMove(object sender, MouseEventArgs e)
        {
            showRangeContextMenu(sender as Rectangle, e);
        }

        private void contextMenu_MouseEnter(object sender, MouseEventArgs e)
        {
            _isMouseOverContextMenu = true;
        }
        #endregion

        private void addRange_Click(object sender, RoutedEventArgs e)
        {
            rangeContextMenuPopup.IsOpen = false;
            if (AddRangeCommand != null)
                AddRangeCommand(this, getAddRangeCommandArgsForRectangle(rangeContextMenuControl.Tag as Rectangle));
        }

        private void deleteRange_Click(object sender, RoutedEventArgs e)
        {
            rangeContextMenuPopup.IsOpen = false;
            if (DeleteRangeCommand != null)
                DeleteRangeCommand(this, getDeleteRangeCommandArgsForRectangle(rangeContextMenuControl.Tag as Rectangle));
        }

        private void configureRange_Click(object sender, RoutedEventArgs e)
        {
            Rectangle rectangle = rangeContextMenuControl.Tag as Rectangle;
            if (rectangle == null)
                return;
            rangeContextMenuPopup.IsOpen = false;
            SelectSectionByIndex((int)rectangle.Tag);
        }

        internal void CloseAllPopups()
        {
            if (rangeContextMenuPopup != null)
                rangeContextMenuPopup.IsOpen = false;
            if (sliderTooltip != null)
                sliderTooltip.IsOpen = false;
        }
    }    
}
