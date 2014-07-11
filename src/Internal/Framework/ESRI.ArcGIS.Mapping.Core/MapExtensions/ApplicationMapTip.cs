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
using System.Windows.Markup;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class ApplicationMapTip 
    {
        private Thickness borderThickness = new Thickness(2);
        public Thickness BorderThickness
        {
            get { return borderThickness; }
            set { borderThickness = value; }
        }

        public Border ToMapTipContainerElement()
        {
            return
#if SILVERLIGHT
 XamlReader.Load(
#else
           XamlReader.Parse(
#endif
ToXaml()) as Border;
        }

        public string ToXaml()
        {
            return  getMapTipXaml(BorderThickness);
        }

        public static ApplicationMapTip FromXaml(string xaml)
        {
            Border border = null;
            if (!string.IsNullOrEmpty(xaml))
            {
                border =

#if SILVERLIGHT
 XamlReader.Load(
#else
           XamlReader.Parse(
#endif
xaml) as Border;
            }

            if (border != null)
            {
                Border outerBorder = border;
                Border childBorder = border.Child as Border; // we actually are tweaking the inner border                
                if (childBorder != null)
                    border = childBorder;
                if (border != null)
                {
                    ApplicationMapTip mapTip = new ApplicationMapTip();
                    mapTip.BorderThickness = border.BorderThickness;
                    return mapTip;
                }
            }
            return null;
        }

        private static string getMapTipXaml(Thickness borderThickness)
        {
            return string.Format(@"<Border xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" 
        xmlns:esriMapping=""http://schemas.esri.com/arcgis/mapping/2009"" 
        xmlns:data=""clr-namespace:System.Windows.Data;assembly=System.Windows"" 
        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
        xmlns:i=""clr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity""
        xmlns:local=""clr-namespace:ESRI.ArcGIS.Mapping.Controls.Behaviors;assembly=ESRI.ArcGIS.Mapping.Controls""
        BorderThickness=""1"" CornerRadius=""5"" BorderBrush=""{{StaticResource BackgroundStartGradientStopColorBrush}}"" 
        Background=""{{StaticResource BackgroundStartGradientStopColorBrush}}"" >
        <Border.Resources>
            <esriMapping:VisibilityConverter x:Key=""VisibilityConverter"" />
            <esriMapping:InvertVisibilityConverter x:Key=""InvertVisibilityConverter"" />
        </Border.Resources>
        <esriMapping:ApplicationMapTipExtensions.GraphicAttributes>
            <data:Binding />
        </esriMapping:ApplicationMapTipExtensions.GraphicAttributes>
        <Border.Effect>
            <DropShadowEffect BlurRadius=""15"" 
					Color=""#99000000""
					Direction=""300"" ShadowDepth=""15"" Opacity=""0.695""/>
        </Border.Effect>
        <Border BorderBrush=""{{StaticResource AccentColorBrush}}"" Background=""{{StaticResource BackgroundGradientBrush}}"" 
            BorderThickness=""{0}"" CornerRadius=""5"">            
                <StackPanel Orientation=""Vertical"" HorizontalAlignment=""Stretch"">
                    <Grid>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width=""*""/>
                            <ColumnDefinition Width=""Auto""/>
                        </Grid.ColumnDefinitions>
                        <Border HorizontalAlignment=""Stretch"" VerticalAlignment=""Stretch"" CornerRadius=""3,3,0,0""
                            Grid.ColumnSpan=""2"" Background=""{{StaticResource BackgroundEndGradientStopColorBrush}}"">
                            <Border.OpacityMask>
                                <LinearGradientBrush StartPoint=""0.15,0"" EndPoint=""0.85,1"">
                                    <GradientStop Color=""#FFFFFFFF"" Offset=""0""/>
                                    <GradientStop Color=""Transparent"" Offset=""1.4""/>
                                </LinearGradientBrush>
                            </Border.OpacityMask>
                        </Border>
                        <ToggleButton x:Name=""TitleToggleButton"" Style=""{{StaticResource ToggleButton_MapTipHeader}}"" 
                                      HorizontalAlignment=""Stretch"" HorizontalContentAlignment=""Stretch"" Cursor=""Hand""
                                      Foreground=""{{StaticResource BackgroundTextColorBrush}}"">
                                <StackPanel x:Name=""TitleContainer"" Margin=""5,2,3,2""/>
                        </ToggleButton>
                        <Grid Visibility=""{{Binding Visibility, ElementName=ScrollViewer}}"" Margin=""0,0,1,0""
                                    Grid.Column=""1"" HorizontalAlignment=""Right"">
                            <ToggleButton Style=""{{StaticResource UpDownToggleButton}}"" 
                                IsChecked=""{{Binding ElementName=TitleToggleButton, Path=IsChecked, Mode=TwoWay}}""                                 
                                Visibility=""{{Binding Visibility, ElementName=TitleContainer}}"" />
                        </Grid>
                    </Grid>
                    <ScrollViewer BorderThickness=""0"" x:Name=""ScrollViewer""
                                    Foreground=""{{StaticResource BackgroundTextColorBrush}}"">
                        <ScrollViewer.Template>
                            <ControlTemplate TargetType=""ScrollViewer"">
                                <Border x:Name=""PopupBodyBorder"" BorderBrush=""{{TemplateBinding BorderBrush}}"" BorderThickness=""{{TemplateBinding BorderThickness}}"" 
                                       CornerRadius=""2"" Visibility=""{{Binding ElementName=TitleToggleButton, Path=IsChecked, Converter={{StaticResource VisibilityConverter}}}}"">
                                    <i:Interaction.Behaviors>
                                        <local:ToggleVisibilityOnClick ToggleButtonName=""TitleToggleButton"" 
                                            VisibilityBindingMode=""Inverse"" VisibilitySourceName=""TitleContainer"" />
                                    </i:Interaction.Behaviors>
                                        <Grid Background=""{{TemplateBinding Background}}"">
                                            <Grid.ColumnDefinitions>
                                                <ColumnDefinition/>
                                                <ColumnDefinition Width=""Auto""/>
                                            </Grid.ColumnDefinitions>
                                            <Grid.RowDefinitions>
                                                <RowDefinition Height=""*""/>
                                                <RowDefinition Height=""Auto""/>
                                                <RowDefinition Height=""Auto""/>
                                            </Grid.RowDefinitions>
                                            <Grid>
                                                <Grid.RowDefinitions>
                                                    <RowDefinition Height=""Auto""/>
                                                    <RowDefinition Height=""*""/>
                                                </Grid.RowDefinitions>
                                                <Rectangle Width=""1"" Height=""5"" Fill=""Transparent"" 
                                                    Visibility=""{{Binding ElementName=TitleContainer, Path=Visibility, 
                                                    Converter={{StaticResource InvertVisibilityConverter}}}}"" />
                                                <ScrollContentPresenter x:Name=""ScrollContentPresenter"" Cursor=""{{TemplateBinding Cursor}}"" 
                                                    ContentTemplate=""{{TemplateBinding ContentTemplate}}"" Margin=""{{TemplateBinding Padding}}""
                                                    Grid.Row=""1""/>
                                            </Grid>
                                            <Rectangle Grid.Column=""1"" Fill=""#FFE9EEF4"" Grid.Row=""1""/>
                                            <ScrollBar x:Name=""VerticalScrollBar"" Grid.Column=""1"" IsTabStop=""False"" Maximum=""{{TemplateBinding ScrollableHeight}}"" 
										            Margin=""0,1,0,0"" Minimum=""0"" Orientation=""Vertical"" Grid.Row=""0"" Visibility=""{{TemplateBinding ComputedVerticalScrollBarVisibility}}"" 
										            Value=""{{TemplateBinding VerticalOffset}}"" ViewportSize=""{{TemplateBinding ViewportHeight}}"" Style=""{{StaticResource ThinScrollBarStyle}}"" Width=""12""/>
                                            <ScrollBar x:Name=""HorizontalScrollBar"" Grid.Column=""0"" IsTabStop=""False"" Maximum=""{{TemplateBinding ScrollableWidth}}"" Margin=""0"" Minimum=""0"" Orientation=""Horizontal"" Grid.Row=""1"" Visibility=""{{TemplateBinding ComputedHorizontalScrollBarVisibility}}"" Value=""{{TemplateBinding HorizontalOffset}}"" ViewportSize=""{{TemplateBinding ViewportWidth}}"" Style=""{{StaticResource ThinScrollBarStyle}}"" Height=""12""/>
                                            <Rectangle Width=""1"" Height=""4"" Fill=""Transparent"" Grid.Row=""2"" 
                                                Visibility=""{{Binding ElementName=TitleContainer, Path=Visibility}}"" />
                                        </Grid>
                                </Border>
                            </ControlTemplate>
                        </ScrollViewer.Template>
                    </ScrollViewer>
                    <ContentControl x:Name=""PopupToolbarContainer""
                                    DataContext=""{{Binding PopupItem}}""
                                    Margin=""2""
                                    VerticalAlignment=""Center""
                                    HorizontalAlignment=""Right"" />
            </StackPanel>
        </Border>
    </Border>", borderThickness);
        }
    }
}
