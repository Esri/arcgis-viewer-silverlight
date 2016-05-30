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
using ESRI.ArcGIS.Mapping.Core;
using System.Windows.Data;
using ESRI.ArcGIS.Client.Symbols;
using ESRI.ArcGIS.Client;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public class SymbologyConfig : Control
    {
        private RadioButton SimpleSymbolOption = null;
        private RadioButton ClassBreaksOption = null;
        private RadioButton PredefinedColorsOption = null;
        private RadioButton CustomColorsOption = null;
        private MultiThumbSlider ClassBreaksSlider = null;
        private TextBlock MinTextBlock = null;
        private TextBox MinTextBox = null;
        private TextBlock MaxTextBlock = null;
        private TextBox MaxTextBox = null;
        private Button AddRangeButton = null;
        private Button DeleteRangeButton = null;
        private Button PreviousRangeButton = null;
        private Button NextRangeButton = null;
        private ComboBox NumericFields = null;
        private ContentControl DefaultSymbolConfigContainer = null;
        private ContentControl ClassBreakSymbologyConfigContainer = null;
        private ContentControl ClassifySection = null;
        private ContentControl FillSymbolSourceTypeSection = null;

        public SymbologyConfig()
        {
            this.DefaultStyleKey = typeof(SymbologyConfig);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (DataContext is ConfigurableHeatMapLayer)
            {
                ClassBreaksOption.IsEnabled = SimpleSymbolOption.IsEnabled = false;
                return;
            }

            findElementsFromTemplate();
            attachBindings();
            attachEventsToUIElements();

            GraphicsLayer lyr = DataContext as GraphicsLayer;
            if (lyr == null)
                return;
            ClassBreaksRenderer renderer = lyr.Renderer as ClassBreaksRenderer;
            if (renderer == null)
            {
                if (SimpleSymbolOption != null)
                    SimpleSymbolOption.IsChecked = true;
            }
            else
            {
                if (ClassBreaksOption != null)
                    ClassBreaksOption.IsChecked = true;
            }
        }

        private void findElementsFromTemplate()
        {
            SimpleSymbolOption = GetTemplateChild("SimpleSymbolOption") as RadioButton;
            ClassBreaksOption = GetTemplateChild("ClassBreaksOption") as RadioButton;
            PredefinedColorsOption = GetTemplateChild("PredefinedColorsOption") as RadioButton;
            CustomColorsOption = GetTemplateChild("CustomColorsOption") as RadioButton;
            ClassBreaksSlider = GetTemplateChild("ClassBreaksSlider") as MultiThumbSlider;
            MinTextBlock = GetTemplateChild("MinTextBlock") as TextBlock;
            MinTextBox = GetTemplateChild("MinTextBox") as TextBox;
            MaxTextBlock = GetTemplateChild("MaxTextBlock") as TextBlock;
            MaxTextBox = GetTemplateChild("MaxTextBox") as TextBox;
            AddRangeButton = GetTemplateChild("AddRangeButton") as Button;
            DeleteRangeButton = GetTemplateChild("DeleteRangeButton") as Button;
            PreviousRangeButton = GetTemplateChild("PreviousRangeButton") as Button;
            NextRangeButton = GetTemplateChild("NextRangeButton") as Button;
            NumericFields = GetTemplateChild("NumericFields") as ComboBox;
            DefaultSymbolConfigContainer = GetTemplateChild("DefaultSymbolConfigContainer") as ContentControl;
            ClassBreakSymbologyConfigContainer = GetTemplateChild("ClassBreakSymbologyConfigContainer") as ContentControl;
            ClassifySection = GetTemplateChild("ClassifySection") as ContentControl;
            FillSymbolSourceTypeSection = GetTemplateChild("FillSymbolSourceTypeSection") as ContentControl;
        }

        private void attachEventsToUIElements()
        {
            if (SimpleSymbolOption != null)
                SimpleSymbolOption.Checked += new RoutedEventHandler(SimpleSymbolButton_Checked);
            if (ClassBreaksOption != null)
                ClassBreaksOption.Checked += new RoutedEventHandler(ClassBreaksButton_Checked);

            if (PredefinedColorsOption != null)
                PredefinedColorsOption.Checked += new RoutedEventHandler(PredefinedColorsOption_Checked);

            if (CustomColorsOption != null)
                CustomColorsOption.Checked += new RoutedEventHandler(CustomColorsOption_Checked);

            if (ClassBreaksSlider != null)
            {
                ClassBreaksSlider.SliderClicked += new EventHandler<SliderClickedEventArgs>(ClassBreaksSlider_SliderClicked);
                ClassBreaksSlider.AddRangeCommand += new EventHandler<AddRangeCommandEventArgs>(ClassBreaksSlider_AddRangeCommand);
                ClassBreaksSlider.DeleteRangeCommand += new EventHandler<DeleteRangeCommandEventArgs>(ClassBreaksSlider_DeleteRangeCommand);
                ClassBreaksSlider.RangeContextMenuChanged += new EventHandler<BaseEventArgs>(ClassBreaksSlider_RangeContextMenuChanged);
            }

            if (MinTextBlock != null)
            {
                MinTextBlock.MouseLeftButtonDown += new MouseButtonEventHandler(MinTextBlock_MouseLeftButtonDown);
                MinTextBlock.MouseLeftButtonUp += new MouseButtonEventHandler(MinTextBlock_MouseLeftButtonUp);
            }
            if (MinTextBox != null)
            {
                MinTextBox.LostFocus += new RoutedEventHandler(MinTextBox_LostFocus);
                MinTextBox.KeyDown += new KeyEventHandler(MinTextBox_KeyDown);
            }

            if (MaxTextBlock != null)
            {
                MaxTextBlock.MouseLeftButtonDown += new MouseButtonEventHandler(MaxTextBlock_MouseLeftButtonDown);
                MaxTextBlock.MouseLeftButtonUp += new MouseButtonEventHandler(MaxTextBlock_MouseLeftButtonUp);
            }
            if (MaxTextBox != null)
            {
                MaxTextBox.LostFocus += new RoutedEventHandler(MaxTextBox_LostFocus);
                MaxTextBox.KeyDown += new KeyEventHandler(MaxTextBox_KeyDown);
            }

            if (AddRangeButton != null)
                AddRangeButton.Click += new RoutedEventHandler(AddRangeButton_Click);
            if (DeleteRangeButton != null)
                DeleteRangeButton.Click += new RoutedEventHandler(DeleteRangeButton_Click);
            if (PreviousRangeButton != null)
                PreviousRangeButton.Click += new RoutedEventHandler(PreviousRangeButton_Click);
            if (NextRangeButton != null)
                NextRangeButton.Click += new RoutedEventHandler(NextRangeButton_Click);

            if (NumericFields != null)
                NumericFields.SelectionChanged += new SelectionChangedEventHandler(NumericFields_SelectionChanged);
        }

        private void attachBindings()
        {
            if (DefaultSymbolConfigContainer != null)
            {
                DefaultSymbolConfigContainer.SetBinding(ContentControl.VisibilityProperty, new Binding()
                {
                    Source = SimpleSymbolOption,
                    Converter = new VisibilityConverter(),
                    Path = new PropertyPath("IsChecked"),
                });
            }

            if (ClassBreakSymbologyConfigContainer != null)
            {
                ClassBreakSymbologyConfigContainer.SetBinding(ContentControl.VisibilityProperty, new Binding()
                {
                    Source = ClassBreaksOption,
                    Converter = new VisibilityConverter(),
                    Path = new PropertyPath("IsChecked"),
                });
            }

            if (ClassifySection != null)
            {
                ClassifySection.SetBinding(ContentControl.VisibilityProperty, new Binding()
                {
                    Source = ClassBreaksOption,
                    Converter = new VisibilityConverter(),
                    Path = new PropertyPath("IsChecked"),
                });
            }

            if (NumericFields != null)
            {
                ISupportsClassification supportsClassification = Layer as ISupportsClassification;
                if (supportsClassification != null)
                {
                    NumericFields.ItemsSource = supportsClassification.NumericFields;
                    NumericFields.DisplayMemberPath = "DisplayName";
                }
            }
        }

        void NumericFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Raise a event??
        }

        void NextRangeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        void PreviousRangeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        void DeleteRangeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        void AddRangeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        void MaxTextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        void MaxTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        void MinTextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }

        void MinTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        void MaxTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        void MaxTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        void MinTextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        void MinTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        void ClassBreaksSlider_RangeContextMenuChanged(object sender, BaseEventArgs e)
        {

        }

        void ClassBreaksSlider_DeleteRangeCommand(object sender, DeleteRangeCommandEventArgs e)
        {

        }

        void ClassBreaksSlider_AddRangeCommand(object sender, AddRangeCommandEventArgs e)
        {

        }

        void ClassBreaksSlider_SliderClicked(object sender, SliderClickedEventArgs e)
        {

        }

        void CustomColorsOption_Checked(object sender, RoutedEventArgs e)
        {

        }

        void PredefinedColorsOption_Checked(object sender, RoutedEventArgs e)
        {

        }

        void ClassBreaksButton_Checked(object sender, RoutedEventArgs e)
        {
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer == null)
                return;

            if (graphicsLayer.Renderer == null)
            {
                graphicsLayer.Renderer = new ClassBreaksRenderer();
            }

            if (DefaultSymbolConfigContainer != null)
                DefaultSymbolConfigContainer.Content = null;
            if (ClassBreakSymbologyConfigContainer != null)
                ClassBreakSymbologyConfigContainer.Content = getConfigControlBasedOnGeometryType(DataContext, true);

            if (FillSymbolSourceTypeSection != null)
                FillSymbolSourceTypeSection.Visibility = GeometryTypeOfLayer == GeometryType.Point ? Visibility.Collapsed : Visibility.Visible;
        }

        void SimpleSymbolButton_Checked(object sender, RoutedEventArgs e)
        {
            // Clear out all class breaks render information
            GraphicsLayer graphicsLayer = Layer as GraphicsLayer;
            if (graphicsLayer != null)
            {
                ClassBreaksRenderer renderer = graphicsLayer.Renderer as ClassBreaksRenderer;
                if (renderer != null)
                {
                    renderer.Classes.Clear();
                }
                graphicsLayer.Renderer = null; // clear out the Renderer                
            }

            if (DefaultSymbolConfigContainer != null)
            {
                Symbol defaultSymbol = SymbologyUtils.GetDefaultSymbolForLayer(Layer);
                if (defaultSymbol == null)
                {
                    // Create and assign a default symbol to the layer ???
                }                
                Control configControl = getConfigControlBasedOnGeometryType(defaultSymbol, false);                
                if (GeometryTypeOfLayer == GeometryType.Point)
                {
                    // In the case of simple renderer, we need to set the layer as the data context
                    // rather than the symbol
                    configControl.DataContext = Layer;
                }
                DefaultSymbolConfigContainer.Content = configControl;
            }
            if (ClassBreakSymbologyConfigContainer != null)
                ClassBreakSymbologyConfigContainer.Content = null;
        }

        private Control getConfigControlBasedOnGeometryType(object dataContextOfConfigControl, bool isConfiguringClassBreak)
        {
            switch (GeometryTypeOfLayer)
            {
                case GeometryType.Point:
                    PointSymbolConfigControl pointSymbolConfigControl = new PointSymbolConfigControl()
                    {
                        DataContext = dataContextOfConfigControl,
                        MarkerSymbolConfigFileUrl = MarkerSymbolConfigFileUrl,
                        MarkerSymbolDirectory = MarkerSymbolDirectory,                        
                    };
                    return pointSymbolConfigControl;
                case GeometryType.Polygon:
                    FillSymbolConfigControl fillSymbolConfigControl = new FillSymbolConfigControl()
                    {
                        DataContext = dataContextOfConfigControl,
                        IsConfiguringClassBreaks = isConfiguringClassBreak,
                    };
                    return fillSymbolConfigControl;
                case GeometryType.PolyLine:
                    LineSymbolConfigControl lineSymbolConfigControl = new LineSymbolConfigControl()
                    {
                        DataContext = dataContextOfConfigControl,
                        IsConfiguringClassBreaks = isConfiguringClassBreak,
                    };
                    return lineSymbolConfigControl;
            }
            return null;
        }

        private GeometryType? geomType;        
        private GeometryType GeometryTypeOfLayer
        {
            get
            {
                if (geomType == null)
                {
                    ConfigurableFeatureLayer featureLayer = Layer as ConfigurableFeatureLayer;
                    if (featureLayer != null)
                    {
                        geomType = featureLayer.GeometryType;
                    }
                    else
                    {
                        ConfigurableGraphicsLayer graphicsLayer = Layer as ConfigurableGraphicsLayer;
                        if (graphicsLayer != null)
                            geomType = graphicsLayer.GeometryType;
                        else
                            geomType = GeometryType.Point;
                    }
                }
                return (GeometryType)geomType;
            }
        }

        public Layer Layer
        {
            get { return DataContext as Layer; }
        }        

        public string MarkerSymbolConfigFileUrl { get; set; }

        public string MarkerSymbolDirectory { get; set; }
    }
}
