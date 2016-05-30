/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Controls;
using ESRI.ArcGIS.Client.FeatureService;
using ESRI.ArcGIS.Client.WebMap.Charting;
using ESRI.ArcGIS.Mapping.Core.Resources;
using ESRI.ArcGIS.Client.Utils;

namespace ESRI.ArcGIS.Mapping.Core
{
    /// <summary>
    /// Contains information about the popup information for a <see cref="FeatureLayer"/>.
    /// </summary>
    public sealed class PopupInfo
    {
        #region Constructor
        public PopupInfo()
        {
            this.popupInfoTemplate = null;
        }
        #endregion

        /// <summary>
        /// Describes the media type associated with the <see cref="PopupInfo"/>.
        /// </summary>
		internal enum MediaType
        {
            /// <summary>Image</summary>
            Image,
            /// <summary>Column chart</summary>
            ColumnChart,
            /// <summary>Pie chart</summary>
            PieChart,
            /// <summary>Line chart</summary>
            LineChart,
            /// <summary>Bar chart</summary>
            BarChart
        };

        #region PopupInfo Members
        /// <summary>
        /// The title of the <see cref="PopupInfo"/>.
        /// </summary>
		public string Title { get; set; }

        /// <summary>
        /// The description of the <see cref="PopupInfo"/>.
        /// </summary>
        internal string Description { get; set; }

        /// <summary>
        /// Indicates whether attachments should be shown in the <see cref="PopupInfo"/>.
        /// </summary>
        internal bool ShowAttachments { get; set; }

        /// <summary>
        /// The collection of <see cref="FieldInfo"/> associated with the <see cref="PopupInfo"/>.
        /// </summary>
        internal IEnumerable<FieldInfo> FieldInfos { get; set; }

        /// <summary>
        /// The collection of <see cref="MediaInfo"/> associated with the <see cref="PopupInfo"/>.
        /// </summary>
        internal IEnumerable<MediaInfo> MediaInfos { get; set; }

        /// <summary>
        /// FeatureLayer info used to find CodedValue domains for display text.
        /// </summary>
        internal FeatureLayerInfo LayerInfo { get; set; }

        /// <summary>
        /// Indicates whether the attributes section of the popup should be rendered or not. This is on by
        /// default and only turned off when the popup configuration specifies NONE. This selection is
        /// represented in JSON by the existence of the "description" node but with no value assigned.
        /// </summary>
        internal bool hideAttributes { get; set; }

        #endregion

        static public PopupInfo FromDictionary(IDictionary<string, object> dictionary)
        {
            var popupInfo = new PopupInfo();

            if (dictionary.ContainsKey("title"))
                popupInfo.Title = dictionary["title"] as string;
            if (dictionary.ContainsKey("description"))
            {
                popupInfo.Description = dictionary["description"] as string;
                //If there is a description, then no attributes should be displayed.
                //User chose CUSTOM or SINGLE if the string is not empty
                if (!string.IsNullOrEmpty(popupInfo.Description))
                    popupInfo.hideAttributes = true;
                else
                    //User chose NONE in XO if the string is empty - however disregard as this is not true for JS Viewer
                    //The workaround is for the user to uncheck all attribute fields before selecting NONE in XO.
                    popupInfo.hideAttributes = false;
            }
            else
                popupInfo.hideAttributes = false;// If no description in Json, attributes should be displayed using field infos
            if (dictionary.ContainsKey("showAttachments"))
                popupInfo.ShowAttachments = Convert.ToBoolean(dictionary["showAttachments"]);
            if (dictionary.ContainsKey("fieldInfos"))
            {
                IEnumerable enumFieldInfos = dictionary["fieldInfos"] as IEnumerable;
                if (enumFieldInfos != null)
                {
                    List<FieldInfo> fieldInfos = new List<FieldInfo>();
                    foreach (IDictionary<string, object> item in enumFieldInfos)
                    {
                        FieldInfo fi = FieldInfo.FromDictionary(item);
                        if (fi != null)
                            fieldInfos.Add(fi);
                    }
                    popupInfo.FieldInfos = fieldInfos;
                }
            }
            if (dictionary.ContainsKey("mediaInfos"))
            {
                IEnumerable enumMediaInfos = dictionary["mediaInfos"] as IEnumerable;
                if (enumMediaInfos != null)
                {
                    List<MediaInfo> mediaInfos = new List<MediaInfo>();
                    foreach (IDictionary<string, object> item in enumMediaInfos)
                    {
                        MediaInfo mi = MediaInfo.FromDictionary(item);
                        if (mi != null)
                            mediaInfos.Add(mi);
                    }
                    popupInfo.MediaInfos = mediaInfos;
                }
            }

            return popupInfo;
        }

        public static PopupInfo FromJson(String json)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            IDictionary<string, object> dict = jss.DeserializeObject(json) as IDictionary<string, object>;

            if (dict != null && dict.Count > 0)
            {
                var popupInfo = PopupInfo.FromDictionary(dict);
                return popupInfo;
            }
            return null;
        }

        public static Dictionary<string, PopupInfo> DictionaryFromJson(string json)
        {
            JavaScriptSerializer jss = new JavaScriptSerializer();
            IDictionary<string, object> dictionary = jss.DeserializeObject(json) as IDictionary<string, object>;
            Dictionary<string, PopupInfo> dict = new Dictionary<string, PopupInfo>();
            foreach (var item in dictionary)
            {
                IDictionary<string, object> popupDict = item.Value as IDictionary<string, object>;
                if (popupDict != null)
                {
                    PopupInfo info = PopupInfo.FromDictionary(popupDict);
                    if (info != null)
                        dict.Add(item.Key, info);
                }
            }
            return dict;
        }
        
        #region PopupInfo DataTemplate
        /// <summary>
        /// The layout template of the <see cref="PopupInfo"/>.
        /// </summary>
        public DataTemplate PopupInfoTemplate
        {
            get
            {
                if (popupInfoTemplate == null)
                    GeneratePopupInfoTemplate();
                return popupInfoTemplate;
            }
        }
        private DataTemplate popupInfoTemplate;

        /// <summary>
        /// The layout template of the <see cref="PopupInfo"/> as a XAML string.
        /// </summary>
        public string PopupInfoTemplateXaml
        {
            get
            {
                if (popupInfoTemplateXaml == null)
                    GeneratePopupInfoTemplate();
                return popupInfoTemplateXaml;
            }
        }
        private string popupInfoTemplateXaml;

        private void GeneratePopupInfoTemplate()
        {
            popupInfoTemplateXaml = GeneratePopupInfoTemplateString();
            try
            {
                this.popupInfoTemplate = (DataTemplate)System.Windows.Markup.XamlReader.Load(popupInfoTemplateXaml);
            }
            catch { /* No content */ }
        }

        public string GeneratePopupInfoTemplateString()
        {
            string LookupField = LayerInfo != null ? LayerInfo.TypeIdField : "";
            StringBuilder sb = new StringBuilder();
            sb.Append("<DataTemplate xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" ");
            sb.Append("xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" ");
            sb.Append("xmlns:webmappopup=\"clr-namespace:ESRI.ArcGIS.Client.WebMap.Popup;assembly=ESRI.ArcGIS.Client.Portal\" ");
            sb.Append("xmlns:mappingCore=\"clr-namespace:ESRI.ArcGIS.Mapping.Core;assembly=ESRI.ArcGIS.Mapping.Core\" ");
            sb.Append("xmlns:converters=\"clr-namespace:ESRI.ArcGIS.Client.Application.Layout.Converters;assembly=ESRI.ArcGIS.Client.Application.Layout\" ");
            sb.Append("xmlns:charting=\"clr-namespace:ESRI.ArcGIS.Client.WebMap.Charting;assembly=ESRI.ArcGIS.Client.Portal\">");
            sb.Append("<Grid HorizontalAlignment=\"Stretch\">");
            sb.Append("<Grid.Resources>");
            sb.Append("<mappingCore:WebMapLabelAttributeConverter x:Key=\"WebMapLabelAttributeConverter\"  />");
            if (FieldInfos != null)
            {
                // Create resource dictionary with the field labels. The dictionary will be passed as argument of PopupChart
                sb.Append("<ResourceDictionary x:Key=\"KeyToLabelDictionary\" xmlns:sys=\"clr-namespace:System;assembly=mscorlib\" >");
                foreach (var info in FieldInfos)
                {
                    sb.AppendFormat("<sys:String x:Key=\"{0}\">{1}</sys:String>", SafeXML(info.FieldName), SafeXML(info.Label));
                }
                sb.Append("</ResourceDictionary>");
            }
            sb.Append("<mappingCore:HtmlToPlainTextConverter x:Key=\"htmlConverter\" />");
            sb.Append("<mappingCore:UrlAttributeConverter x:Key=\"urlConverter\" />");
            sb.Append("<mappingCore:HyperlinkVisibleConverter x:Key=\"showLinkConverter\" ShowUrl=\"true\" />");
            sb.Append("<mappingCore:HyperlinkVisibleConverter x:Key=\"hideLinkConverter\" ShowUrl=\"false\" />");

            #region ItemSelectorItem Template.  Required for localization of Previous/Next buttons.

            sb.Append("<converters:LocalizationConverter x:Key=\"Localization\" />");
            sb.Append(@"
<Style x:Key=""ItemSelectorItemStyle"" TargetType=""webmappopup:ItemSelectorItem"">
			<Setter Property=""Template"">
				<Setter.Value>
					<ControlTemplate TargetType=""webmappopup:ItemSelectorItem"">
						<StackPanel x:Name=""LayoutRoot"" Background=""{TemplateBinding Background}"" Visibility=""Collapsed"" Opacity=""0"">
							<VisualStateManager.VisualStateGroups>
								<VisualStateGroup x:Name=""CommonStates"">
									<VisualState x:Name=""Normal""/>
									<VisualState x:Name=""MouseOver""/>
									<VisualState x:Name=""Disabled"" />
								</VisualStateGroup>
								<VisualStateGroup x:Name=""SelectionStates"">
									<VisualState x:Name=""Unselected"">
										<Storyboard>
											<ObjectAnimationUsingKeyFrames BeginTime=""00:00:00"" Storyboard.TargetName=""LayoutRoot"" Storyboard.TargetProperty=""(UIElement.Visibility)"">
												<DiscreteObjectKeyFrame KeyTime=""00:00:0.3"">
													<DiscreteObjectKeyFrame.Value>
														<Visibility>Collapsed</Visibility>
													</DiscreteObjectKeyFrame.Value>
												</DiscreteObjectKeyFrame>
											</ObjectAnimationUsingKeyFrames>
											<DoubleAnimation Duration=""0:0:0.3"" To=""0.0"" Storyboard.TargetProperty=""(UIElement.Opacity)"" Storyboard.TargetName=""LayoutRoot""/>
										</Storyboard>
									</VisualState>
									<VisualState x:Name=""Selected"">
										<Storyboard>
											<ObjectAnimationUsingKeyFrames BeginTime=""00:00:00"" Storyboard.TargetName=""LayoutRoot"" Storyboard.TargetProperty=""(UIElement.Visibility)"">
												<DiscreteObjectKeyFrame KeyTime=""00:00:00"">
													<DiscreteObjectKeyFrame.Value>
														<Visibility>Visible</Visibility>
													</DiscreteObjectKeyFrame.Value>
												</DiscreteObjectKeyFrame>
											</ObjectAnimationUsingKeyFrames>
											<DoubleAnimation Duration=""0:0:0.3"" To=""1.0"" Storyboard.TargetProperty=""(UIElement.Opacity)"" Storyboard.TargetName=""LayoutRoot""/>
										</Storyboard>
									</VisualState>
								</VisualStateGroup>
								<VisualStateGroup x:Name=""FocusStates"">
									<VisualState x:Name=""Focused""/>
									<VisualState x:Name=""Unfocused""/>
								</VisualStateGroup>
							</VisualStateManager.VisualStateGroups>
							<ContentPresenter
								Content=""{TemplateBinding Header}""
								Margin=""{TemplateBinding Padding}""
								HorizontalAlignment=""{TemplateBinding HorizontalContentAlignment}""
								VerticalAlignment=""{TemplateBinding VerticalContentAlignment}""/>
							<Grid HorizontalAlignment=""Center"">
								<Grid.ColumnDefinitions>
									<ColumnDefinition Width=""30"" />
									<ColumnDefinition Width=""*"" MinWidth=""80"" />
									<ColumnDefinition Width=""30"" />
								</Grid.ColumnDefinitions>
								<Button x:Name=""PrevButton"" HorizontalAlignment=""Right"" VerticalAlignment=""Center"" Grid.Column=""0"">
									<Button.Template>
										<ControlTemplate>
											<Grid Margin=""0,0,-4,0"" HorizontalAlignment=""Right"" Cursor=""Hand"" Background=""Transparent"">
												<ToolTipService.ToolTip>
													<TextBlock Text=""{Binding ConverterParameter=BtnBack, Converter={StaticResource Localization}}"" />
												</ToolTipService.ToolTip>
												<Path Opacity=""1"" Margin=""12"" Fill=""#FF262626"" VerticalAlignment=""Center"" HorizontalAlignment=""Center""  Data=""M 12,0 L 12,15 L 0,7.5 Z"" StrokeLineJoin=""Miter"" />
											</Grid>
										</ControlTemplate>
									</Button.Template>
								</Button>
								<ContentPresenter Grid.Column=""1""
									Content=""{TemplateBinding Content}""
									ContentTemplate=""{TemplateBinding ContentTemplate}""
									Margin=""{TemplateBinding Padding}""
									HorizontalAlignment=""{TemplateBinding HorizontalContentAlignment}""
									VerticalAlignment=""{TemplateBinding VerticalContentAlignment}""/>
								<Button x:Name=""NextButton"" HorizontalAlignment=""Left"" VerticalAlignment=""Center"" Grid.Column=""2"">
									<Button.Template>
										<ControlTemplate>
											<Grid Margin=""-4,0, 0, 0"" HorizontalAlignment=""Left"" Cursor=""Hand"" Background=""Transparent"" >
												<ToolTipService.ToolTip>
													<TextBlock Text=""{Binding ConverterParameter=BtnNext, Converter={StaticResource Localization}}"" />
												</ToolTipService.ToolTip>
												<Path Opacity=""1"" Margin=""12"" Fill=""#FF262626"" VerticalAlignment=""Center"" HorizontalAlignment=""Center""  Data=""M 0,0 L 0,15 L 12,7.5 Z"" StrokeLineJoin=""Miter"" />
											</Grid>
										</ControlTemplate>
									</Button.Template>
								</Button>
							</Grid>
						</StackPanel>
					</ControlTemplate>
				</Setter.Value>
			</Setter>
		</Style>");

            #endregion

            sb.Append("</Grid.Resources>");
            string gridRowDefinitions = string.Empty;
            StringBuilder content = new StringBuilder();
            int numRows = 0;

            if (!string.IsNullOrEmpty(this.Description))
            {
                AddTextBlock(content, this.Description);
                gridRowDefinitions += "<RowDefinition Height=\"Auto\" />";
                numRows++;
            }

            #region FieldInfos
            if (this.hideAttributes == false  // If hideAttributes, user configured NONE, CUSTOM or SINGLE
                && this.FieldInfos != null)// and there is field information, display attributes
            {
                // Grid with three columns:
                sb.Append("<Grid.ColumnDefinitions>");
                sb.Append("<ColumnDefinition Width=\"Auto\" />");
                sb.Append("<ColumnDefinition Width=\"Auto\" />");
                sb.Append("<ColumnDefinition Width=\"*\" />");
                sb.Append("</Grid.ColumnDefinitions>");

                foreach (PopupInfo.FieldInfo fieldInfo in this.FieldInfos)
                {
                    if (fieldInfo.Visible && !string.IsNullOrEmpty(fieldInfo.FieldName))
                    {
                        // Apply alternating row background to every other row
                        if (numRows % 2 == 1)
                        {
                            content.AppendFormat(@"<Rectangle Fill=""{{StaticResource BackgroundTextColorBrush}}""
                                                HorizontalAlignment=""Stretch"" VerticalAlignment=""Stretch""
                                                Grid.ColumnSpan=""3"" Grid.Row=""{0}"" Opacity=""0.15"" />", numRows);
                        }

                        // TODO: fieldInfo.IsEditable, fieldInfo.StringFieldOptions
                        content.AppendFormat(@"<Grid Grid.Row=""{0}""><TextBlock Opacity="".6"" 
                                            Text=""{1}"" Margin=""5,2"" HorizontalAlignment=""Left"" 
                                            VerticalAlignment=""Center"" TextWrapping=""Wrap"" /></Grid>",
                                             numRows, (string.IsNullOrEmpty(fieldInfo.Label) ? string.Format("{{{0}}}", 
                                             fieldInfo.FieldName) : SafeXML(fieldInfo.Label)));
                        content.AppendFormat(@"<HyperlinkButton Grid.Column=""2"" Content=""{0}"" Grid.Row=""{1}"" 
                            VerticalAlignment=""Center"" Margin=""5,2""
                            NavigateUri=""{{Binding PopupItem, Converter={{StaticResource htmlConverter}}, ConverterParameter={2}}}"" 
                            Visibility=""{{Binding PopupItem, Converter={{StaticResource showLinkConverter}}, ConverterParameter={2}}}"" 
                            TargetName=""_blank"" HorizontalAlignment=""Stretch"" />", 
                            Strings.MoreInfo, numRows, fieldInfo.FieldName);
                        content.AppendFormat(@"<TextBlock Grid.Column=""2"" Grid.Row=""{0}"" 
                            VerticalAlignment=""Center"" Margin=""5,2""
                            Visibility=""{{Binding PopupItem, Converter={{StaticResource hideLinkConverter}}, ConverterParameter={1}}}"" 
                            Text=""{{Binding PopupItem, Converter={{StaticResource htmlConverter}}, ConverterParameter={1}}}"" 
                            TextWrapping=""Wrap""", numRows, fieldInfo.FieldName);

                        // TODO: fieldInfo.Formatter
                        if (!string.IsNullOrEmpty(fieldInfo.Tooltip))
                            content.AppendFormat(" ToolTipService.ToolTip=\"{0}\"", fieldInfo.Tooltip);
                        content.Append(" />");

                        // Column separator
                        content.AppendFormat(@"<Rectangle Fill=""{{StaticResource BackgroundTextColorBrush}}""
                                                HorizontalAlignment=""Stretch"" VerticalAlignment=""Stretch""
                                                Grid.Column=""1"" Grid.Row=""{0}"" Opacity=""0.3"" Width=""1""/>",
                                                numRows);

                        gridRowDefinitions += "<RowDefinition Height=\"Auto\" />";
                        numRows++;
                    }
                }
            }
            #endregion

            #region MediaInfos
            if (this.MediaInfos != null && this.MediaInfos.Any())
            {
                content.AppendFormat(@"<webmappopup:ItemSelector Grid.Row=""{0}"" Grid.ColumnSpan=""3"" ItemContainerStyle=""{{StaticResource ItemSelectorItemStyle}}"">
                    <webmappopup:ItemSelector.Items>", numRows); // allow to go through medias
                gridRowDefinitions += "<RowDefinition Height=\"Auto\" />";
                foreach (PopupInfo.MediaInfo mediaInfo in this.MediaInfos)
                {
                    if (mediaInfo.Value != null)
                    {
                        content.Append("<webmappopup:ItemSelectorItem><webmappopup:ItemSelectorItem.Header><StackPanel Margin=\"0,10,0,0\">");
                        AddTextBlock(content, mediaInfo.Title, true);
                        content.Append("<Border Height=\"1\" Background=\"Black\" Margin=\"0,5\" />");
                        AddTextBlock(content, mediaInfo.Caption, false, true);
                        content.Append("</StackPanel></webmappopup:ItemSelectorItem.Header><StackPanel Margin=\"0,0,0,10\">");

                        switch (mediaInfo.Type)
                        {
                            case PopupInfo.MediaType.Image:
                                GenerateImageMedia(content, mediaInfo); break;

                            case PopupInfo.MediaType.BarChart:
                            case PopupInfo.MediaType.ColumnChart:
                            case PopupInfo.MediaType.LineChart:
                            case PopupInfo.MediaType.PieChart:
                                GenerateChartMedia(content, mediaInfo); break;
                        }
                        content.Append("</StackPanel></webmappopup:ItemSelectorItem>");
                    }
                }
                content.Append("</webmappopup:ItemSelector.Items></webmappopup:ItemSelector>");
            }
            #endregion

            if (!string.IsNullOrEmpty(gridRowDefinitions))
                sb.AppendFormat("<Grid.RowDefinitions>{0}</Grid.RowDefinitions>", gridRowDefinitions);
            sb.Append(content.ToString());
            sb.Append("</Grid></DataTemplate>");
            return sb.ToString();
        }

        private void GenerateImageMedia(StringBuilder sb, PopupInfo.MediaInfo mediaInfo)
        {
            if (mediaInfo.Value.ContainsKey("linkURL") &&
                                     !string.IsNullOrEmpty(mediaInfo.Value["linkURL"] as string))
            {
                sb.AppendFormat("<HyperlinkButton NavigateUri=\"{0}\" TargetName=\"_blank\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\">",
                                GetBinding(mediaInfo.Value["linkURL"]));
                sb.Append("<HyperlinkButton.Template>");
                sb.Append("<ControlTemplate>");
            }
            if (mediaInfo.Value.ContainsKey("sourceURL") &&
                !string.IsNullOrEmpty(mediaInfo.Value["sourceURL"] as string))
                sb.AppendFormat("<Image Source=\"{0}\" Stretch=\"Uniform\" HorizontalAlignment=\"Center\"" +
                                " VerticalAlignment=\"Center\" MaxHeight=\"145\" Margin=\"0,5,0,0\" />",
                                GetBinding(mediaInfo.Value["sourceURL"]));
            if (mediaInfo.Value.ContainsKey("linkURL") &&
                !string.IsNullOrEmpty(mediaInfo.Value["linkURL"] as string))
            {
                sb.Append("</ControlTemplate>");
                sb.Append("</HyperlinkButton.Template>");
                sb.Append("</HyperlinkButton>");
            }
        }
        internal const string NormalizeSeparator = "_::_"; // separator for the normalize field in Fields
        private static void GenerateChartMedia(StringBuilder sb, MediaInfo mediaInfo)
        {
            var fields = mediaInfo.Value.ContainsKey("fields") ? mediaInfo.Value["fields"] as IEnumerable : null;
            if (fields != null)
            {
                //Build string of fields to include in chart (used by ChartConverter as a ConverterParameter)
                string fieldString = "";
                foreach (var field in fields)
                {
                    string fieldName = field.ToString();
                    if (fieldString.Length > 0)
                        fieldString += ",";
                    fieldString += fieldName;
                }

                if (mediaInfo.Value.ContainsKey("normalizeField"))
                {
                    var normalizeField = mediaInfo.Value["normalizeField"] as string;
                    if (!string.IsNullOrEmpty(normalizeField) && !normalizeField.Equals("null"))
                        fieldString += NormalizeSeparator + normalizeField;
                }

                string chartControl = null;
                switch (mediaInfo.Type)
                {
                    case PopupInfo.MediaType.BarChart:
                    case PopupInfo.MediaType.ColumnChart:
                    case PopupInfo.MediaType.LineChart:
                    case PopupInfo.MediaType.PieChart:
                        chartControl = mediaInfo.Type.ToString(); break;
                }

                if (!string.IsNullOrEmpty(chartControl))
                    sb.AppendFormat("<charting:{0} DataContext=\"{{Binding PopupItem.Graphic.Attributes}}\" Fields=\"{1}\" KeyToLabelDictionary =\"{{StaticResource KeyToLabelDictionary}}\" MaxHeight=\"160\" MaxWidth=\"218\" Margin=\"0,5,0,0\" />",
                                        chartControl, fieldString);
            }
        }

        private void AddTextBlock(StringBuilder sb, string value, bool isBoldFont = false, bool isItalicFont = false)
        {
            if (!string.IsNullOrEmpty(value))
            {
                sb.Append("<TextBlock TextWrapping=\"Wrap\" DataContext=\"{Binding PopupItem}\"");
                if (isBoldFont)
                    sb.Append(" FontWeight=\"Bold\"");
                if (isItalicFont)
                    sb.Append(" FontStyle=\"Italic\"");
                sb.Append(">");
                sb.Append("<mappingCore:WebMapLabelAttributeConverter.InlineCollection>");
                sb.AppendFormat("<Binding Converter=\"{{StaticResource WebMapLabelAttributeConverter}}\" ConverterParameter=\"{{}}{0}\" />", SafeXML(value.ToStrippedHtmlText()));
                sb.Append("</mappingCore:WebMapLabelAttributeConverter.InlineCollection>");
                sb.Append("</TextBlock>");
            }
        }

        private string GetBinding(object value)
        {
            if (value != null)
                return string.Format("{{Binding PopupItem, Converter={{StaticResource urlConverter}}, ConverterParameter='{0}'}}", value.ToStrippedHtmlText());

            return string.Empty;
        }
        #endregion

        #region FieldInfo Class Definition
        /// <summary>
        /// Contains information about fields that are used in the <see cref="PopupInfo"/>.
        /// </summary>
        internal sealed class FieldInfo
        {
            #region Constructor
            internal FieldInfo() { }
            #endregion

            /// <summary>
            /// The name of the field.
            /// </summary>
            internal string FieldName { get; set; }

            /// <summary>
            /// The field label.
            /// </summary>
            internal string Label { get; set; }

            /// <summary>
            /// Format string to be used for the field value.
            /// </summary>
            internal string Formatter { get; set; }

            /// <summary>
            /// The value indicating whether this <see cref="FieldInfo"/> is visible.
            /// </summary>
            internal bool Visible { get; set; }

            /// <summary>
            /// The value indicating whether this instance of the <see cref="FieldInfo"/> is editable.
            /// </summary>
            internal bool IsEditable { get; set; }

            /// <summary>
            /// The tooltip text associated with the <see cref="FieldInfo"/>.
            /// </summary>
            internal string Tooltip { get; set; }

            // TODO: Support StringFieldOptions

            internal static FieldInfo FromDictionary(IDictionary<string, object> dictionary)
            {
                FieldInfo fi = new FieldInfo();

                if (dictionary.ContainsKey("fieldName"))
                    fi.FieldName = dictionary["fieldName"] as string;
                if (dictionary.ContainsKey("label"))
                    fi.Label = dictionary["label"] as string;
                if (dictionary.ContainsKey("formatter"))
                    fi.Formatter = dictionary["formatter"] as string;
                if (dictionary.ContainsKey("visible"))
                    fi.Visible = Convert.ToBoolean(dictionary["visible"]);
                if (dictionary.ContainsKey("isEditable"))
                    fi.IsEditable = Convert.ToBoolean(dictionary["isEditable"]);
                if (dictionary.ContainsKey("tooltip"))
                    fi.Tooltip = dictionary["tooltip"] as string;
                // TODO: Add support for stringFieldOptions

                return fi;
            }
        }
        #endregion

        #region MediaInfo Class Definition
        /// <summary>
        /// Contains information about media information that are used in the <see cref="PopupInfo"/>.
        /// </summary>
        internal sealed class MediaInfo
        {
            #region Constructor
            internal MediaInfo() { }
            #endregion

            /// <summary>
            /// The title of the media.
            /// </summary>
            internal string Title { get; set; }

            /// <summary>
            /// The caption of the media.
            /// </summary>
            internal string Caption { get; set; }

            /// <summary>
            /// The type of the media.
            /// </summary>
            internal MediaType Type { get; set; }

            /// <summary>
            /// The value of the media.
            /// </summary>
            internal IDictionary<string, object> Value { get; set; }

            internal static MediaInfo FromDictionary(IDictionary<string, object> dictionary)
            {
                MediaInfo mi = new MediaInfo();

                if (dictionary.ContainsKey("type"))
                {
                    string type = dictionary["type"] as string;
                    switch (type.ToLower())
                    {
                        case "image": mi.Type = MediaType.Image; break;
                        case "piechart": mi.Type = MediaType.PieChart; break;
                        case "barchart": mi.Type = MediaType.BarChart; break;
                        case "columnchart": mi.Type = MediaType.ColumnChart; break;
                        case "linechart": mi.Type = MediaType.LineChart; break;
                        default: return null;
                    }
                }
                else return null;
                if (dictionary.ContainsKey("title"))
                    mi.Title = dictionary["title"] as string;
                if (dictionary.ContainsKey("caption"))
                    mi.Caption = dictionary["caption"] as string;
                if (dictionary.ContainsKey("value"))
                    mi.Value = dictionary["value"] as IDictionary<string, object>;

                return mi;
            }
        }
        #endregion

        internal static string SafeXML(string str)
        {
            return str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;");
        }
    }

    #region Extended Methods
    internal static class PopupInfoEntention
    {
        private static string htmlLineBreakRegex = @"<br(.)*?>";	// Regular expression to strip HTML line break tag
        private static string htmlStripperRegex = @"<(.|\n)*?>";	// Regular expression to strip HTML tags

        internal static string ToStrippedHtmlText(this object input)
        {
            string retVal = string.Empty;

            if (input != null)
            {
                // Replace HTML line break tags with $LINEBREAK$:
                retVal = Regex.Replace(input as string, htmlLineBreakRegex, "$LINEBREAK$", RegexOptions.IgnoreCase);
                // Remove the rest of HTML tags:
                retVal = Regex.Replace(retVal, htmlStripperRegex, string.Empty);
            }

            return retVal;
        }
    }
    #endregion

    #region Custom String Format Converter
    public class ConverterUtil
    {
        public static IDictionary<string, object> GetAttributeDictionary(object value)
        {
            IDictionary<string, object> dict = null;
            ESRI.ArcGIS.Client.Extensibility.PopupItem item = value as ESRI.ArcGIS.Client.Extensibility.PopupItem;
            if (item != null && item.Graphic != null)
                    dict = item.Graphic.Attributes;
            else
                dict = value as IDictionary<string, object>;
            return dict;
        }

        public static object GetValue(object value, string fieldName)
        {
            ESRI.ArcGIS.Client.Extensibility.PopupItem item = value as ESRI.ArcGIS.Client.Extensibility.PopupItem;
            if (item != null)
            {
                return MapTipsHelper.GetFieldValue(item, fieldName);
            }
            return null;
        }
        public static object GetValue(ESRI.ArcGIS.Client.Extensibility.PopupItem popupItem, string fieldName)
        {
           return MapTipsHelper.GetFieldValue(popupItem, fieldName);
        }
    }

    /// <summary>
    /// FOR INTERNAL USE ONLY. Removes HTML from attribute values.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class HtmlToPlainTextConverter : IValueConverter
    {
        private static string htmlLineBreakRegex = @"<br ?/?>";  // Regular expression to strip HTML line break tag        
        private static string htmlStripperRegex = @"<(.|\n)*?>";	// Regular expression to strip HTML tags

        #region IValueConverter Members

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
        object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string FieldName = parameter as string;
            object fieldValue = ConverterUtil.GetValue(value, FieldName);
            if (fieldValue is string)
            {
                // Replace HTML line break tags.
                fieldValue = Regex.Replace(fieldValue as string, htmlLineBreakRegex, string.Empty);
                // Remove the rest of HTML tags.
                fieldValue = Regex.Replace(fieldValue as string, htmlStripperRegex, string.Empty);
                return fieldValue;
            }
            return fieldValue;
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
        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    /// <summary>
    /// FOR INTERNAL USE ONLY. Creates a string url from an attribute dictionary
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class UrlAttributeConverter : IValueConverter
    {
        private string attributeBindingRegex = @"({)([^}]*)(})";	// Regular expression to identify attribute bindings
        #region IValueConverter Members

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
            var dict = ConverterUtil.GetAttributeDictionary(value);
            if (dict != null)
            {

                StringBuilder sb = new StringBuilder();
                string formatter = parameter as string;
                if (!string.IsNullOrEmpty(formatter))
                {
                    string[] splitStringArray = Regex.Split(formatter, attributeBindingRegex);
                    bool isAttributeName = false;
                    foreach (string str in splitStringArray)
                    {
                        if (str == "{") { isAttributeName = true; continue; }
                        if (str == "}") { isAttributeName = false; continue; }
                        if (isAttributeName && dict.ContainsKey(str))
                            sb.AppendFormat("{0}", ConverterUtil.GetValue(value, str));
                        else if (!isAttributeName)
                            sb.AppendFormat("{0}", str);
                    }
                    return sb.ToString();
                }
                else
                    return value;
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

        #endregion
    }
    /// <summary>
    /// FOR INTERNAL USE ONLY. Custom label/attribute value converter for the popup content of web map documents.
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class WebMapLabelAttributeConverter : IValueConverter
    {
        private string attributeBindingRegex = @"({)([^}]*)(})";	// Regular expression to identify attribute bindings
        private static string htmlLineBreakRegex = @"<br ?/?>";  // Regular expression to strip HTML line break tag        
        private static string htmlStripperRegex = @"<(.|\n)*?>";	// Regular expression to strip HTML tags  

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
            var dict = ConverterUtil.GetAttributeDictionary(value);
            if (dict != null)
            {
                
                string formatter = parameter as string;
                ESRI.ArcGIS.Client.Extensibility.PopupItem popupItem;
                if ((popupItem = value as ESRI.ArcGIS.Client.Extensibility.PopupItem) != null)
                    value = ConvertToString(popupItem, dict, formatter);
                if (value is string)
                {
                    return CreateInlineCollection(value as string);
                }
            }
            return null;
        }

        public object ConvertToString(ESRI.ArcGIS.Client.Extensibility.PopupItem item, IDictionary<string, object> dict, string formatter)
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
                    if (isAttributeName && dict.ContainsKey(str))
                    {
                        var temp = ConverterUtil.GetValue(item, str);
                        sb.AppendFormat("{0}", temp);
                    }
                    else if (!isAttributeName)
                        sb.AppendFormat("{0}", str);
                }
                return sb.ToString().Replace("$LINEBREAK$", "<br/>").Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&apos;", "'").Replace("&quot;", "\"");
            }
            return null;
        }

        private static object CreateInlineCollection(string value)
        {
            List<Inline> InlineCollection = new List<Inline>();
            string[] lines = Regex.Split(value, htmlLineBreakRegex, RegexOptions.IgnoreCase);
            bool skip = true;
            foreach (string line in lines)
            {
                if (!skip)
                    InlineCollection.Add(new LineBreak());
                else
                    skip = false;

                // Remove the rest of HTML tags.
                string strRun = Regex.Replace(line, htmlStripperRegex, string.Empty, RegexOptions.IgnoreCase);
                if (!string.IsNullOrEmpty(strRun))
                {
                    Run run = new Run();
                    run.Text = strRun;
                    InlineCollection.Add(run);
                }
            }
            return InlineCollection;
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
        /// <summary>
        /// Gets the Inline collection from the dependency object.
        /// </summary>
        /// <param name="obj">DependencyObject that should be searched for the attached property.</param>
        /// <returns>A collection of Inline objects found attached to the DependencyObject.</returns>
        public static IEnumerable<Inline> GetInlineCollection(DependencyObject obj)
        {
            return (IEnumerable<Inline>)obj.GetValue(InlineCollectionProperty);
        }
        /// <summary>
        /// Sets a collection of Inline object to a DependencyObject
        /// </summary>
        /// <param name="obj">The DependencyObject to set the collection of Inline object to.</param>
        /// <param name="value">The Inline object collection to be set to the DependencyObject.</param>
        public static void SetInlineCollection(DependencyObject obj, IEnumerable<Inline> value)
        {
            obj.SetValue(InlineCollectionProperty, value);
        }

        /// <summary>
        /// Attached property that holds a collection of Inline objects.
        /// </summary>
        public static readonly DependencyProperty InlineCollectionProperty =
            DependencyProperty.RegisterAttached("InlineCollection", typeof(IEnumerable<Inline>), typeof(WebMapLabelAttributeConverter), new PropertyMetadata(OnInlineCollectionPropertyChanged));

        private static void OnInlineCollectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBlock textblock = d as TextBlock;
            IEnumerable<Inline> newValue = e.NewValue as IEnumerable<Inline>;
            IEnumerable<Inline> oldvalue = e.OldValue as IEnumerable<Inline>;

            if (textblock != null)
            {
                textblock.Inlines.Clear();
                if (newValue != null)
                {
                    foreach (Inline line in newValue)
                    {
                        textblock.Inlines.Add(line);
                    }
                }
            }
        }
    }

    /// <summary>
    /// FOR INTERNAL USE ONLY. Converter for WebMap popups that convert text to hyperlinks. 
    /// </summary>
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class HyperlinkVisibleConverter : IValueConverter
    {
        /// <summary>
        /// *For internal use only* : Gets or sets a value indicating whether show URL.
        /// </summary>		
        public bool ShowUrl { get; set; }
        #region IValueConverter Members

        /// <summary>
        /// *For internal use only* : Modifies the source data before passing it to the target for display in the UI.
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
            Uri url;
            bool isUrl = false;
            object fieldValue = ConverterUtil.GetValue(value, parameter as string);
            if (fieldValue is string)
            {
                if (fieldValue is string)
                    isUrl = Uri.TryCreate(fieldValue as string, UriKind.Absolute, out url);
            }
            return (isUrl == ShowUrl) ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// *For internal use only* : Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
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

        #endregion
    }
    #endregion
}
