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
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Xml.Linq;

namespace ESRI.ArcGIS.Mapping.Core
{
    [DataContract]
    public class ApplicationColorSet : INotifyPropertyChanged
    {
        private const string ACCENT_COLOR = "AccentColor";
        private const string ACCENT_TEXT_COLOR = "AccentTextColor";
        private const string BACKGROUND_END_GRADIENT_STOP_COLOR = "BackgroundEndGradientStopColor";
        private const string BACKGROUND_START_GRADIENT_STOP_COLOR = "BackgroundStartGradientStopColor";
        private const string BACKGROUND_TEXT_COLOR = "BackgroundTextColor";
        private const string SELECTION_COLOR = "SelectionColor";
        private const string SELECTION_OUTLINE_COLOR = "SelectionOutlineColor";

        private Color backgroundStartGradientColor = Color.FromArgb(255, 0xFB, 0xFC, 0xFD);

        private bool syncDesignHostBrushes = false;
        public bool SyncDesignHostBrushes 
        {
            get { return syncDesignHostBrushes; } 
            set
            {
                if (value != syncDesignHostBrushes)
                {
                    syncDesignHostBrushes = value;
                    if (syncDesignHostBrushes)
                    {
                        applyAccentColor();
                        applyAccentTextColor();
                        applyBackgroundEndGradient();
                        applyBackgroundStartGradient();
                        applyBackgroundText();
                        applySelectionColor();
                        applySelectionOutlineColor();
                    }
                }
            } 
        }

        [DataMember]
        public Color BackgroundStartGradientColor
        {
            get
            {
                return backgroundStartGradientColor;
            }
            set
            {
                if (backgroundStartGradientColor != value)
                {
                    setAppResource(BACKGROUND_START_GRADIENT_STOP_COLOR, value);
                    backgroundStartGradientColor = value;
                    applyBackgroundStartGradient();
                    OnPropertyChanged(new PropertyChangedEventArgs("BackgroundStartGradientColor"));
                }
            }
        }

        private void setAppResource(string key, object value)
        {
            if (Application.Current.Resources.Contains(key))
                Application.Current.Resources.Remove(key);
            Application.Current.Resources.Add(key, value);
        }

        private void applyBackgroundStartGradient()
        {
            applyColorToSolidBrush("BackgroundStartGradientStopColorBrush", backgroundStartGradientColor);
            applyColorToLinearGradientStop("BackgroundGradientBrush", 0, backgroundStartGradientColor);

            if (SyncDesignHostBrushes)
            {
                applyColorToSolidBrush("DesignHostBackgroundStartBrush", backgroundStartGradientColor);
                applyColorToLinearGradientStop("DesignHostBackgroundBrush", 0, backgroundStartGradientColor);
            }
        }

        private Color backgroundEndGradientColor = Color.FromArgb(255, 0xE0, 0xE3, 0xE7);
        [DataMember]
        public Color BackgroundEndGradientColor
        {
            get
            {
                return backgroundEndGradientColor;
            }
            set
            {
                if (backgroundEndGradientColor != value)
                {
                    setAppResource(BACKGROUND_END_GRADIENT_STOP_COLOR, value);
                    backgroundEndGradientColor = value;
                    applyBackgroundEndGradient();
                    OnPropertyChanged(new PropertyChangedEventArgs("BackgroundEndGradientColor"));
                }
            }
        }

        private void applyBackgroundEndGradient()
        {
            applyColorToSolidBrush("BackgroundEndGradientStopColorBrush", backgroundEndGradientColor);
            applyColorToLinearGradientStop("BackgroundGradientBrush", 1, backgroundEndGradientColor);

            if (SyncDesignHostBrushes)
            {
                applyColorToSolidBrush("DesignHostBackgroundEndBrush", backgroundEndGradientColor);
                applyColorToLinearGradientStop("DesignHostBackgroundBrush", 1, backgroundEndGradientColor);
            }
        }

        private Color accentColor = Color.FromArgb(255, 0x24, 0x39, 0x4E);
        [DataMember]
        public Color AccentColor
        {
            get
            {
                return accentColor;
            }
            set
            {
                if (accentColor != value)
                {
                    setAppResource(ACCENT_COLOR, value);
                    accentColor = value;
                    applyAccentColor();
                    OnPropertyChanged(new PropertyChangedEventArgs("AccentColor"));
                }
            }
        }

        private void applyAccentColor()
        {
            applyColorToSolidBrush("AccentColorBrush", accentColor);
            if (SyncDesignHostBrushes)
                applyColorToSolidBrush("DesignHostAccentBrush", accentColor);

            applyColorToLinearGradientStop("AccentColorGradientBrush", 0, accentColor);
        }

        private Color selectionColor = Color.FromArgb(255, 0xA6, 0xA6, 0xA6);
        [DataMember]
        public Color SelectionColor
        {
            get
            {
                return selectionColor;
            }
            set
            {
                if (selectionColor != value)
                {
                    setAppResource(SELECTION_COLOR, value);
                    selectionColor = value;
                    applySelectionColor();
                    OnPropertyChanged(new PropertyChangedEventArgs("SelectionColor"));
                }
            }
        }

        private void applySelectionColor()
        {
            applyColorToSolidBrush("SelectionColorBrush", selectionColor);

            // Mouse over is 50% transparent of selected color
            Color mouseOverColor = Color.FromArgb((byte)(selectionColor.A * 0.5), selectionColor.R, 
                selectionColor.G, selectionColor.B);
            applyColorToSolidBrush("MouseOverColorBrush", mouseOverColor);

            if (SyncDesignHostBrushes)
            {
                applyColorToSolidBrush("DesignHostSelectionBrush", selectionColor);
                applyColorToSolidBrush("DesignHostMouseOverBrush", mouseOverColor);
            }
        }

        private Color backgroundTextColor = Color.FromArgb(255, 0, 0, 0);
        [DataMember]
        public Color BackgroundTextColor
        {
            get
            {
                return backgroundTextColor;
            }
            set
            {
                if (backgroundTextColor != value)
                {
                    setAppResource(BACKGROUND_TEXT_COLOR, value);
                    backgroundTextColor = value;
                    applyBackgroundText();
                    OnPropertyChanged(new PropertyChangedEventArgs("BackgroundTextColor"));
                }
            }
        }

        private void applyBackgroundText()
        {
            applyColorToSolidBrush("BackgroundTextColorBrush", backgroundTextColor);
            if (SyncDesignHostBrushes)
                applyColorToSolidBrush("DesignHostBackgroundTextBrush", backgroundTextColor);
        }

        private Color accentTextColor = Color.FromArgb(255, 0xFF, 0xFF, 0xFF);
        [DataMember]
        public Color AccentTextColor
        {
            get
            {
                return accentTextColor;
            }
            set
            {
                if (accentTextColor != value)
                {
                    setAppResource(ACCENT_TEXT_COLOR, value);
                    accentTextColor = value;
                    applyAccentTextColor();
                    OnPropertyChanged(new PropertyChangedEventArgs("AccentTextColor"));
                }
            }
        }

        private void applyAccentTextColor()
        {
            applyColorToSolidBrush("AccentTextColorBrush", accentTextColor);
            if (SyncDesignHostBrushes)
                applyColorToSolidBrush("DesignHostAccentTextBrush", accentTextColor);
        }

        private Color selectionOutlineColor = Color.FromArgb(255, 0x00, 0x00, 0x00);
        [DataMember]
        public Color SelectionOutlineColor
        {
            get
            {
                return selectionOutlineColor;
            }
            set
            {
                if (selectionOutlineColor != value)
                {
                    setAppResource(SELECTION_OUTLINE_COLOR, value);
                    selectionOutlineColor = value;
                    applySelectionOutlineColor();
                    OnPropertyChanged(new PropertyChangedEventArgs("SelectionOutlineColor"));
                }
            }
        }

        private void applySelectionOutlineColor()
        {
            applyColorToSolidBrush("SelectionOutlineColorBrush", selectionOutlineColor);
            if (SyncDesignHostBrushes)
                applyColorToSolidBrush("DesignHostSelectionOutlineBrush", selectionOutlineColor);
        }
        
        public void RestoreDefaultsColorsToApplication()
        {
            applyAccentColor();
            applyAccentTextColor();
            applyBackgroundStartGradient();
            applyBackgroundEndGradient();
            applyBackgroundText();
            applySelectionColor();
            applySelectionOutlineColor();
        }

        private void applyColorToSolidBrush(string brushKey, Color brushColor)
        {
            SolidColorBrush solidBrush = Application.Current.Resources[brushKey] as SolidColorBrush;
            if (solidBrush != null)
                solidBrush.Color = brushColor;
        }

        private void applyColorToLinearGradientStop(string brushKey, int stopIndex, Color stopColor)
        {
            LinearGradientBrush linearBrush = Application.Current.Resources[brushKey] as LinearGradientBrush;
            if (linearBrush != null && linearBrush.GradientStops.Count > stopIndex)
                linearBrush.GradientStops[stopIndex].Color = stopColor;
        }

        #region INotifyPropertyChanged Members

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {            
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion            
    }
}
