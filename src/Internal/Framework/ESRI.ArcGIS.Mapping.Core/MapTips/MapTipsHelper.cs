/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using ESRI.ArcGIS.Client.FeatureService;

namespace ESRI.ArcGIS.Mapping.Core
{
	public class MapTipsHelper
	{
        public static ICollection<FieldSettings> ToFieldSettings(IEnumerable<FieldInfo> infos)
        {
            if (infos == null)
                return null;
            ICollection<FieldSettings> settings = new List<FieldSettings>();
            foreach (FieldInfo item in infos)
            {
                settings.Add(item);
            }
            return settings;
        }

        public static object GetFieldValue(PopupItem popupItem, string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName) || popupItem == null || popupItem.Graphic == null || popupItem.Graphic.Attributes == null
                || popupItem.FieldInfos == null)
                return null;

            Layer layer = popupItem.Layer;
            FeatureLayer featureLayer = layer as FeatureLayer;
            Collection<LayerInformation> layerInfos = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetLayerInfos(layer);

            // Try to find feature layer from layer info
            foreach (LayerInformation item in layerInfos)
            {
                if (item.ID == popupItem.LayerId)
                {
                    featureLayer = item.FeatureLayer;
                    break;
                }
            }

            // Try to find field information from field settings collection (which are FieldInfo objects)
            FieldInfo fldInfo = null;
            foreach (FieldSettings fld in popupItem.FieldInfos)
            {
                if (fld.Name.ToLower().Equals(fieldName.ToLower()))
                {
                    fldInfo = fld as FieldInfo;
                    break;
                }
            }

            if (fldInfo == null)
                return null;

            // Create local variable for attributes to improve performance since collection is referenced multiple
            // times in logic below.
            IDictionary<string, object> attributes = popupItem.Graphic.Attributes;

            // If the field is a subdomain type, then lookup the value and return
            if (FieldInfo.GetDomainSubTypeLookup(featureLayer, fldInfo) != DomainSubtypeLookup.None)
                return GetDomainValue(featureLayer, attributes, fldInfo);

            // Try to extract value from field using Name and then DisplayName
            object fieldValue = null;
            if (!attributes.TryGetValue(fldInfo.Name, out fieldValue))
            {
                if (!attributes.TryGetValue(fldInfo.DisplayName, out fieldValue))
                    return null;
            }

            // Special formatting if the field type is Currency or Date/Time
            if (fldInfo.FieldType == FieldType.Currency)
            {
                CurrencyFieldValue currencyFieldValue = fieldValue as CurrencyFieldValue;
                if (currencyFieldValue != null)
                    fieldValue = currencyFieldValue.FormattedValue;
            }
            else if (fldInfo.FieldType == FieldType.DateTime)
            {
                DateTimeFieldValue dateFieldValue = fieldValue as DateTimeFieldValue;
                if (dateFieldValue != null)
                    fieldValue = dateFieldValue.FormattedValue;
            }

            return fieldValue;
        }

        public static void GetTitle(PopupItem popupItem, LayerInformation info)
        {
            if (popupItem == null)
                return;

            string titleExpression = string.Empty;
            string title = string.Empty;
            
            Layer layer = popupItem.Layer;

            #region Get from developer override/webmap override
            //First get from developer override
            IDictionary<int, string> titleExpressions = ESRI.ArcGIS.Client.Extensibility.LayerProperties.GetPopupTitleExpressions(layer);
            //then if defined in web map
            if (titleExpressions == null && ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetUsePopupFromWebMap(layer))
                titleExpressions = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetWebMapPopupTitleExpressions(layer);
            if (titleExpressions != null)
            {
                if (layer is GraphicsLayer)
                    titleExpression = (titleExpressions.ContainsKey(-1)) ? titleExpressions[-1] : null;
                else
                    titleExpression = (titleExpressions.ContainsKey(popupItem.LayerId)) ? titleExpressions[popupItem.LayerId] : null;
            }
            #endregion

            #region Get title from expression
            if (!string.IsNullOrWhiteSpace(titleExpression))
                title = ConvertExpressionWithFieldNames(popupItem, titleExpression);
            #endregion

            if (string.IsNullOrWhiteSpace(title))
            {
                if (layer is GraphicsLayer) //get from display field set in layer configuration
                {
                    #region Get from display field - Graphics layer
                    string displayFieldName = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetDisplayField(layer as GraphicsLayer);
                    if (!string.IsNullOrWhiteSpace(displayFieldName))
                    {
                        object o = GetFieldValue(popupItem, displayFieldName);
                        title = (o != null) ? o.ToString() : string.Empty;
                        if (string.IsNullOrWhiteSpace(popupItem.TitleExpression))
                            titleExpression = "{" + displayFieldName + "}";
                    }
                    #endregion
                }
                else if (!ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetUsePopupFromWebMap(layer))
                {
                    #region Get from display field - dynamic layer
                    if (!string.IsNullOrEmpty(info.DisplayField))//get from display field set in layer configuration
                    {
                        object o = GetFieldValue(popupItem, info.DisplayField);
                        title = (o != null) ? o.ToString() : string.Empty;
                        titleExpression = "{" + info.DisplayField + "}";
                    }
                    #endregion
                }
            }

            if (title != null)
                title = title.Trim();
            popupItem.Title = title;
            popupItem.TitleExpression = titleExpression;
        }

        const string attributeBindingRegex = @"({)([^}]*)(})";	// Regular expression to identify attribute bindings

        public static string ConvertExpressionWithFieldNames(PopupItem item, string formatter)
        {
            if (!string.IsNullOrEmpty(formatter))
            {
                StringBuilder sb = new StringBuilder();
                string[] splitStringArray = Regex.Split(formatter, attributeBindingRegex);
                bool isAttributeName = false;
                foreach (string str in splitStringArray)
                {
                    if (str == "{") { isAttributeName = true; continue; }
                    if (str == "}") { isAttributeName = false; continue; }
                    if (isAttributeName && item.Graphic.Attributes.ContainsKey(str))
                    {
                        var temp = MapTipsHelper.GetFieldValue(item, str);
                        sb.AppendFormat("{0}", temp);
                    }
                    else if (!isAttributeName)
                        sb.AppendFormat("{0}", str);
                }
                return sb.ToString().Replace("$LINEBREAK$", "<br/>").Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&apos;", "'").Replace("&quot;", "\"");
            }
            return null;
        }

		public static DataTemplate BuildMapTipDataTemplate(PopupItem popupItem, out bool hasContent, LayerInformation layerInfo = null)
		{
			hasContent = false;
            if (popupItem == null || popupItem.Graphic == null || popupItem.Graphic.Attributes == null || popupItem.Layer == null)
                return null;

            Layer layer = popupItem.Layer;
			string popupDataTemplatesXaml = null;
            #region Get from developer override/webmap override
            //First get from developer override
            IDictionary<int, string> templates = ESRI.ArcGIS.Client.Extensibility.LayerProperties.GetPopupDataTemplates(layer);
            //then if defined in web map
            if (templates == null && ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetUsePopupFromWebMap(layer))
                templates = ESRI.ArcGIS.Mapping.Core.LayerExtensions.GetWebMapPopupDataTemplates(layer);
            if (templates != null)
            {
                if (layer is GraphicsLayer)
                    popupDataTemplatesXaml = (templates.ContainsKey(-1)) ? templates[-1] : null;
                else
                    popupDataTemplatesXaml = (templates.ContainsKey(layerInfo.ID)) ? templates[layerInfo.ID] : null;
            }
            #endregion

			DataTemplate dt = null;
			StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(popupDataTemplatesXaml))
            {
                // Use popup data template xaml.
                hasContent = true;
                sb.Append(popupDataTemplatesXaml);
            }
            else
            {
                sb.Append("<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" ");
                sb.Append("xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" ");
                sb.Append("xmlns:mapping=\"http://schemas.esri.com/arcgis/mapping/2009\" ");
                sb.Append("xmlns:local=\"clr-namespace:ESRI.ArcGIS.Mapping.Controls;assembly=ESRI.ArcGIS.Mapping.Controls\" >");
                sb.Append("<Grid HorizontalAlignment=\"Stretch\"><Grid.Resources>");
                sb.Append("<mapping:LabelAttributeConverter x:Key=\"LabelAttributeConverter\" />");
                sb.Append("<mapping:UrlLocationAttributeConverter x:Key=\"UrlLocationAttributeConverter\" />");
                sb.Append("<mapping:UrlDescriptionAttributeConverter x:Key=\"UrlDescriptionAttributeConverter\" />");
                sb.Append("</Grid.Resources>");

                if (popupItem.FieldInfos != null)
                {
                    // Grid with three columns:
                    sb.Append("<Grid.ColumnDefinitions>");
                    sb.Append("<ColumnDefinition Width=\"2*\"/>");
                    sb.Append("<ColumnDefinition Width=\"Auto\"/>");
                    sb.Append("<ColumnDefinition Width=\"3*\" />");
                    sb.Append("</Grid.ColumnDefinitions>");

                    string gridRowDefinitions = string.Empty;
                    StringBuilder content = new StringBuilder();
                    int numRows = 0;
                    foreach (FieldSettings field in popupItem.FieldInfos)
                    {
                        // If field is not to be displayed, skip this field
                        if (!field.VisibleOnMapTip)
                            continue;

                        hasContent = true;
                        gridRowDefinitions += "<RowDefinition Height=\"Auto\" />";
                        numRows++;

                        // Apply alternating row background to every other row
                        if (numRows % 2 == 0)
                        {
                            content.AppendFormat(@"<Rectangle Fill=""{{StaticResource BackgroundTextColorBrush}}""
                                                HorizontalAlignment=""Stretch"" VerticalAlignment=""Stretch""
                                                Grid.ColumnSpan=""3"" Grid.Row=""{0}"" Opacity=""0.15"" />", numRows - 1);
                        }

                        if (!string.IsNullOrEmpty(field.DisplayName))
                        {
                            // This is where we will eventually add code that uses an expression to plug in the actual
                            // value for a field name when curly braces are detected (or something).
                            content.AppendFormat(@"<Grid Grid.Row=""{0}""><TextBlock Grid.Column=""0"" Text=""{1}"" 
                                                Margin=""5,2"" Opacity="".6"" VerticalAlignment=""Center"" 
                                                TextWrapping=""Wrap"" /></Grid>",
                                numRows - 1, field.DisplayName);
                        }

                        // Process this field regardless of value. We need to create a control that has binding so that as
                        // the value changes during editing the control will adapt and display null and not-null values
                        // properly.
                        object o = GetFieldValue(popupItem, field.Name);
                        StringBuilder element = GetFieldValueElement(field, o, numRows - 1);
                        if (element != null)
                        {
                            content.Append(element);
                        }

                        // Column separator
                        content.AppendFormat(@"<Rectangle Fill=""{{StaticResource BackgroundTextColorBrush}}""
                                                HorizontalAlignment=""Stretch"" VerticalAlignment=""Stretch""
                                                Grid.Column=""1"" Grid.Row=""{0}"" Opacity=""0.3"" Width=""1""/>", 
                                                numRows - 1);
                    }

                    #region Add Open Item hyperlink
                    if (layer is CustomGraphicsLayer)
                    {
                        ESRI.ArcGIS.Client.Application.Layout.Converters.LocalizationConverter converter = new Client.Application.Layout.Converters.LocalizationConverter();
                        gridRowDefinitions += "<RowDefinition Height=\"Auto\" />";
                        numRows++;
                        content.AppendFormat("<HyperlinkButton Content=\"{0}\" Grid.ColumnSpan=\"2\" Grid.Row=\"{1}\" CommandParameter=\"{{Binding}}\"><HyperlinkButton.Command><local:OpenItemCommand/></HyperlinkButton.Command></HyperlinkButton>",
                            converter.Get("OpenItem"), numRows - 1);
                    }
                    #endregion

                    if (!string.IsNullOrEmpty(gridRowDefinitions))
                        sb.AppendFormat("<Grid.RowDefinitions>{0}</Grid.RowDefinitions>", gridRowDefinitions);
                    sb.Append(content.ToString());
                }
                sb.Append("</Grid></DataTemplate>");
            }
			try
			{
				dt = (DataTemplate)System.Windows.Markup.XamlReader.Load(sb.ToString());
            }
			catch { /* No content */ }

			return dt;
		}

		public static ESRI.ArcGIS.Client.Field GetField(string name, List<Client.Field> fields)
		{
			foreach (Client.Field item in fields)
			{
				if (item.Name.Equals(name))
					return item;
			}
			return null;
		}

        public static object GetDomainValue(FeatureLayer featureLayer, IDictionary<string, object> attibutes, FieldInfo field)
		{
			object originalValue = string.Empty;
			string fieldName = string.Empty;
            if (featureLayer == null)
                return originalValue;
			if (field != null)
				fieldName = field.Name;
			if (attibutes != null)
			{
				originalValue = attibutes[field.Name];
				if (originalValue == null)
				{
					if (attibutes.ContainsKey(field.DisplayName))
					{
						fieldName = field.DisplayName;
						originalValue = attibutes[field.DisplayName];
					}
					else
						return originalValue;
				}
			}
			if (featureLayer == null)
				return originalValue;
			if (field.DomainSubtypeLookup == DomainSubtypeLookup.None || field.DomainSubtypeLookup == DomainSubtypeLookup.NotDefined)
				return originalValue;
			ESRI.ArcGIS.Client.FeatureService.FeatureLayerInfo featureLayerInfo = featureLayer.LayerInfo;
			if (featureLayerInfo == null)
				return originalValue;
			IDictionary<object, FeatureType> featureTypes = featureLayerInfo.FeatureTypes;
			if ((featureTypes == null) && (field.DomainSubtypeLookup != DomainSubtypeLookup.FieldDomain)) return originalValue;
			if (attibutes == null) return originalValue;
			string domainCodedValue = string.Empty;
			object typeIdValue = null;
			switch (field.DomainSubtypeLookup)
			{
				case DomainSubtypeLookup.NotDefined:
				case DomainSubtypeLookup.None:
					break;
				case DomainSubtypeLookup.FieldDomain:
					Client.Field f = GetField(field.Name, featureLayerInfo.Fields);
					if (f == null) return originalValue;
					return GetDomainValue(originalValue, f.Domain as CodedValueDomain);
				case DomainSubtypeLookup.TypeIdField:
					if (!attibutes.ContainsKey(featureLayerInfo.TypeIdField))
						return originalValue;
					typeIdValue = attibutes[featureLayerInfo.TypeIdField];
					foreach (KeyValuePair<object, FeatureType> item in featureLayerInfo.FeatureTypes)
					{
						if (item.Key.Equals(typeIdValue) && item.Value != null)
							return item.Value.Name;
					}
					break;
				case DomainSubtypeLookup.FeatureTypeDomain:
					if (!attibutes.ContainsKey(featureLayerInfo.TypeIdField))
						return originalValue;
					typeIdValue = attibutes[featureLayerInfo.TypeIdField];
					foreach (KeyValuePair<object, FeatureType> fTypePair in featureLayerInfo.FeatureTypes)
					{
						if (fTypePair.Key.Equals(typeIdValue) && fTypePair.Value != null && fTypePair.Value.Domains != null)
						{
							foreach (KeyValuePair<string, Domain> pair in fTypePair.Value.Domains)
							{
								if (pair.Key.Equals(field.Name) && pair.Value is CodedValueDomain)
								{
									return GetDomainValue(originalValue, pair.Value as CodedValueDomain);
								}
							}
						}
					}
					break;
				default:
					break;
			}
			return originalValue;
		}

		public static object GetDomainValue(object fieldValue, ESRI.ArcGIS.Client.FeatureService.CodedValueDomain cvDomain)
		{
            if (fieldValue == null)
                return fieldValue;
			if (cvDomain != null)
			{
				if (cvDomain.CodedValues.ContainsKey(fieldValue))
					return cvDomain.CodedValues[fieldValue];
			}
			return fieldValue;
		}

		private static StringBuilder GetFieldValueElement(FieldSettings field, object fieldValue, int rowNumber)
		{
			StringBuilder sb = null;

			switch (field.FieldType)
			{
				case FieldType.Attachment:
					#region
					AttachmentFieldValue attachmentFieldValue = fieldValue as AttachmentFieldValue;
					if (attachmentFieldValue != null && attachmentFieldValue.AttachmentsProvider != null)
					{
						sb = new StringBuilder();
                        sb.AppendFormat("<mapping:AttachmentsPanel Grid.Column=\"2\" Grid.Row=\"{0}\" FieldValue=\"{{Binding PopupItem.Graphic.Attributes[{1}]}}\" />",
                            rowNumber, field.Name);
					}
					#endregion
					break;

				case FieldType.Entity:
				case FieldType.Hyperlink:
					#region
					sb = new StringBuilder();
					sb.AppendFormat(@"<StackPanel Grid.Column=""2"" Grid.Row=""{0}"" Orientation=""Vertical"" 
                                      VerticalAlignment=""Center"" Margin=""5,2"" >",
						rowNumber);
                    sb.AppendFormat("<HyperlinkButton NavigateUri=\"{{Binding PopupItem, Converter={{StaticResource UrlLocationAttributeConverter}}, ConverterParameter={0}}}\" TargetName=\"_blank\" Background=\"Transparent\" VerticalAlignment=\"Top\" >",
                        field.Name);
                    sb.AppendFormat("<TextBlock Text=\"{{Binding PopupItem, Converter={{StaticResource UrlDescriptionAttributeConverter}}, ConverterParameter={0}}}\" FontWeight=\"Normal\" Margin=\"0,0,0,5\" HorizontalAlignment=\"Left\" VerticalAlignment=\"Top\" TextWrapping=\"Wrap\" Foreground=\"Blue\" TextDecorations=\"Underline\" />",
                        field.Name);
                    sb.Append("</HyperlinkButton>");
					sb.Append("</StackPanel>");
					#endregion
					break;

				case FieldType.Image:
					#region
					sb = new StringBuilder();
					sb.AppendFormat(@"<StackPanel Grid.Column=""2"" Grid.Row=""{0}"" Orientation=""Vertical"">",
						rowNumber);
                    sb.AppendFormat("<Image Margin=\"8, -2, 5, 5\" Source=\"{{Binding PopupItem, Converter={{StaticResource UrlLocationAttributeConverter}}, ConverterParameter={0}}}\" ToolTipService.ToolTip=\"{{Binding PopupItem, Converter={{StaticResource UrlDescriptionAttributeConverter}}, ConverterParameter={1}}}\" />", field.Name, field.Name);
                    sb.Append("</StackPanel>");
					#endregion
					break;

				default:
                    #region
                    sb = new StringBuilder();
                    if (!string.IsNullOrEmpty(field.Name))
                    {
                        sb.AppendFormat(@"<mapping:HyperlinkOrTextBlock Grid.Column=""2"" Grid.Row=""{0}"" Margin=""5,2""
FieldValue=""{{Binding PopupItem, Converter={{StaticResource LabelAttributeConverter}}, ConverterParameter={1} }}"" />",
                            rowNumber, field.Name);
                    }
                    #endregion
                    break;
			}

			return sb;
		}

        public static string ExtractHyperlinkValue(object value, bool returnUrl)
        {
            HyperlinkImageFieldValue hyperlinkImageFieldValue = value as HyperlinkImageFieldValue;
            if (hyperlinkImageFieldValue != null)
            {
                return returnUrl ? hyperlinkImageFieldValue.ImageUrl : hyperlinkImageFieldValue.ImageTooltip;
            }
            else
            {
                EntityFieldValue entityFieldValue = value as EntityFieldValue;
                if (entityFieldValue != null)
                {
                    return returnUrl ? entityFieldValue.LinkUrl : entityFieldValue.DisplayText;
                }
                else
                {
                    LookupFieldValue lookupFieldValue = value as LookupFieldValue;
                    if (lookupFieldValue != null)
                    {
                        return returnUrl ? lookupFieldValue.LinkUrl : lookupFieldValue.DisplayText;
                    }
                    else
                    {
                        HyperlinkFieldValue hyperlinkFieldValue = value as HyperlinkFieldValue;
                        if (hyperlinkFieldValue != null)
                        {
                            return returnUrl ? hyperlinkFieldValue.LinkUrl : hyperlinkFieldValue.DisplayText;
                        }
                        else
                        {
                            return value as string;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Custom label/attribute value converter for the popup content of web map documents.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class LabelAttributeConverter : IValueConverter
    {
        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IDictionary<string, object> dict = null;
            PopupItem popupitem = null;

            if (value is PopupItem)
            {
                popupitem = value as PopupItem;
                if (popupitem != null && popupitem.Graphic != null)
                    dict = popupitem.Graphic.Attributes;
            }

            if (dict != null)
            {
                string fieldName = parameter as string;
                if (!string.IsNullOrEmpty(fieldName))
                {
                    object o = MapTipsHelper.GetFieldValue(popupitem, fieldName);
                    if (o != null)
                        return o.ToString();
                }
            }

            return null;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Custom Url location value converter for the popup content of web map documents.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class UrlLocationAttributeConverter : IValueConverter
    {
        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IDictionary<string, object> dict = null;
            PopupItem popupitem = null;

            if (value is PopupItem)
            {
                popupitem = value as PopupItem;
                if (popupitem != null && popupitem.Graphic != null)
                    dict = popupitem.Graphic.Attributes;
            }

            if (dict != null)
            {
                object fieldValue = null;
                string fieldName = parameter as string;
                if (!string.IsNullOrEmpty(fieldName))
                    fieldValue = MapTipsHelper.GetFieldValue(popupitem, fieldName);

                if (fieldValue != null)
                {
                    string fieldUrl = MapTipsHelper.ExtractHyperlinkValue(fieldValue, true);

                    if (String.IsNullOrEmpty(fieldUrl))
                        return null;

                    Uri targetUri = null;
                    if (!Uri.TryCreate(fieldUrl, UriKind.Absolute, out targetUri))
                        return null;

                    return fieldUrl;
                }
            }

            return null;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Custom Url description value converter for the popup content of web map documents.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class UrlDescriptionAttributeConverter : IValueConverter
    {
        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IDictionary<string, object> dict = null;
            PopupItem popupitem = null;

            if (value is PopupItem)
            {
                popupitem = value as PopupItem;
                if (popupitem != null && popupitem.Graphic != null)
                    dict = popupitem.Graphic.Attributes;
            }

            if (dict != null)
            {
                object fieldValue = null;
                string fieldName = parameter as string;
                if (!string.IsNullOrEmpty(fieldName))
                    fieldValue = MapTipsHelper.GetFieldValue(popupitem, fieldName);

                if (fieldValue != null)
                {
                    string fieldUrl = MapTipsHelper.ExtractHyperlinkValue(fieldValue, false);

                    if (String.IsNullOrEmpty(fieldUrl))
                        return null;

                    return fieldUrl;
                }
            }

            return null;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <param name="value">The target data being passed to the source.</param>
        /// <param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="culture">The culture of the conversion.</param>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
