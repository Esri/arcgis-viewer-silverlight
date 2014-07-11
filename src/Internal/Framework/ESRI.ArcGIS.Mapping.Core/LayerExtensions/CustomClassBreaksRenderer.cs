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
using ESRI.ArcGIS.Client;
using System.ComponentModel;
using ESRI.ArcGIS.Client.Symbols;
using System.Collections.ObjectModel;

namespace ESRI.ArcGIS.Mapping.Core
{
    [System.Windows.Markup.ContentProperty("Classes")]
    public class CustomClassBreaksRenderer : DependencyObject, IRenderer, INotifyPropertyChanged
    {
        private string attribute;
        private Symbol defaultSymbol;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassBreaksRenderer"/> class.
        /// </summary>
        public CustomClassBreaksRenderer()
        {
            SetValue(ClassesProperty, new ObservableCollection<ClassBreakInfo>());
            Classes.CollectionChanged += (o, e) => { OnPropertyChanged("Classes"); };
        }

        private static readonly DependencyProperty ClassesProperty =
                DependencyProperty.Register
                ("Classes", typeof(ObservableCollection<ClassBreakInfo>),
                typeof(CustomClassBreaksRenderer), null);

        /// <summary>
        /// Gets the collection of <see cref="ClassBreakInfo"/>.
        /// </summary>
        /// <value>The classes.</value>
        public ObservableCollection<ClassBreakInfo> Classes
        {
            get { return (ObservableCollection<ClassBreakInfo>)GetValue(ClassesProperty); }
        }

        /// <summary>
        /// Gets or sets the attribute to use for classes.
        /// </summary>
        /// <value>The attribute.</value>
        public string Attribute
        {
            get { return attribute; }
            set
            {
                if (attribute != value)
                {
                    attribute = value;
                    OnPropertyChanged("Attribute");
                }
            }
        }

        /// <summary>
        /// Gets or sets the default symbol.
        /// </summary>
        /// <value>The default symbol.</value>
        public Symbol DefaultSymbol
        {
            get { return defaultSymbol; }
            set
            {
                if (defaultSymbol != value)
                {
                    defaultSymbol = value;
                    OnPropertyChanged("DefaultSymbol");
                }
            }
        }

        #region IRenderer Members
        /// <summary>
        /// Gets a symbol based on a graphic.
        /// </summary>
        /// <param name="graphic">The graphic.</param>
        /// <returns>Symbol</returns>
        public Symbol GetSymbol(Graphic graphic)
        {
            if (graphic != null && Attribute != null)
            {
                if (graphic.Attributes.ContainsKey(Attribute))
                {
                    object objValue = graphic.Attributes[Attribute];
                    CurrencyFieldValue currencyFieldValue = objValue as CurrencyFieldValue;
                    if (currencyFieldValue != null)
                    {
                        objValue = currencyFieldValue.Value;
                    }
                    double value;
                    if (double.TryParse(
                        string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", objValue),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out value))
                    {
                        foreach (ClassBreakInfo info in Classes)
                        {
                            if (value == info.MinimumValue || value >= info.MinimumValue && value < info.MaximumValue)
                                return info.Symbol;
                        }
                    }
                }
            }
            return DefaultSymbol;
        }
        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
