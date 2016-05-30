/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Extensibility;
using SearchTool.Resources;
using System.Linq;

namespace SearchTool
{
    /// <summary>
    /// Initializes the title for the targeted layer's popups.  Only supports GraphicsLayers.
    /// </summary>
    public class SetPopupTitleAction : TargetedTriggerAction<Layer>
    {
        protected override void Invoke(object parameter)
        {
            // Make sure the target is a GraphicsLayer
            if (Target != null && Target is GraphicsLayer)
            {
                // Check whether a title expression has been defined
                if (string.IsNullOrEmpty(TitleExpression))
                {
                    // No title expression, so auto-initialize popups

                    // If the target layer is a feature layer, check whether the service metadata specifies a display field
                    if (Target is FeatureLayer)
                    {
                        FeatureLayer fLayer = (FeatureLayer)Target;

                        // Check if the layer's metadata defines a display field
                        if (fLayer.LayerInfo != null && !string.IsNullOrEmpty(fLayer.LayerInfo.DisplayField))
                        {
                            // use the display field
                            setPopupTitle(fLayer.LayerInfo.DisplayField, fLayer);
                            return;
                        }
                        else if (!fLayer.IsInitialized)
                        {
                            // Wait for the layer to initialize and check again
                            fLayer.Initialized += Layer_Initialized;
                            return;
                        }
                    }

                    // The layer is not a feature layer (i.e. does not have metadata), so auto-determine the display field
                    // based on the graphics in the layer
                    initPopupTitleFromGraphics((GraphicsLayer)Target);
                }
                else
                {
                    // A title expression has been explicitly defined, so use that
                    Dictionary<int, string> expression = new Dictionary<int, string>();
                    expression.Add(-1, TitleExpression);
                    LayerProperties.SetPopupTitleExpressions(Target, expression);
                }

            }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="TitleExpression"/> property
        /// </summary>
        public static DependencyProperty TitleExpressionProperty = DependencyProperty.Register(
            "TitleExpression", typeof(string), typeof(SetPopupTitleAction), null);

        /// <summary>
        /// Gets or sets the expression to use for the popup title.  Field values can be included by specifying
        /// field names in curly braces.
        /// </summary>
        public string TitleExpression
        {
            get { return GetValue(TitleExpressionProperty) as string; }
            set { SetValue(TitleExpressionProperty, value); }
        }

        // Fires when the layer initializes, if the target is a feature layer
        private void Layer_Initialized(object sender, System.EventArgs e)
        {
            // Unhook the initialized event
            FeatureLayer fLayer = (FeatureLayer)Target;
            fLayer.Initialized -= Layer_Initialized;

            // Check whether the feature layer metadata specifies a display field
            if (fLayer.LayerInfo != null && !string.IsNullOrEmpty(fLayer.LayerInfo.DisplayField))
                // Use the display field
                setPopupTitle(fLayer.LayerInfo.DisplayField, fLayer);
            else // No display field defined on the service, so auto-determine it based on the layer's graphics
                initPopupTitleFromGraphics(fLayer);
        }

        // Fires when the first graphic or set of graphics is added to the targeted layer
        private void Graphics_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Unhook the event handler
            ((GraphicCollection)sender).CollectionChanged -= Graphics_CollectionChanged;

            // The layer now has at least one graphic, so auto-initialize the popup title
            autoInitPopupTitle(Target as GraphicsLayer);
        }

        // Initializes the popup title of a graphics layer based on the layer's graphics, i.e. not from layer metadata
        private void initPopupTitleFromGraphics(GraphicsLayer gLayer)
        {
            if (gLayer.Graphics.Count == 0) // If no graphics, wait until the first graphic is added
                gLayer.Graphics.CollectionChanged += Graphics_CollectionChanged;
            else // There are graphics in the layer, so initialize the title
                autoInitPopupTitle(gLayer);
        }

        // Initializes the popup title on a graphics layer, given the title field name
        private void setPopupTitle(string titleField, GraphicsLayer layer)
        {
            Dictionary<int, string> expression = new Dictionary<int, string>();
            expression.Add(-1, string.Format("{{{0}}}", titleField));
            LayerProperties.SetPopupTitleExpressions(Target, expression);
        }

        // Auto-initializes the popup title for the targeted layer
        private void autoInitPopupTitle(GraphicsLayer layer)
        {
            if (layer == null)
                return;

            // Auto-detect the display field for the layer
            string fieldName = findDisplayField(Target as GraphicsLayer);

            // Use the display field for the popup title
            if (!string.IsNullOrEmpty(fieldName))
                setPopupTitle(fieldName, layer);
        }

        // Auto-detects the display field for a GraphicsLayer
        private string findDisplayField(GraphicsLayer layer)
        {
            string displayField = null;

            if (layer.Graphics.Count > 0) // Make sure the layer has at least one graphic
            {
                Graphic g = layer.Graphics[0];

                // First check if there is a field named "Name" (localized)
                if (g.Attributes.Any(att => att.Key.ToLower() == Strings.Name.ToLower()))
                {
                    displayField = g.Attributes.FirstOrDefault(att => att.Key.ToLower() == Strings.Name.ToLower()).Key as string;
                }
                // Check if there is a field named "Name" (unlocalized)
                else if (Strings.Name.ToLower() != "name" && g.Attributes.Any(att => att.Key.ToLower() == "name"))
                {
                    displayField = g.Attributes.FirstOrDefault(att => att.Key.ToLower() == "name").Key;
                }
                else // Check if there is a field name that contains "Name" (localized)
                {                    
                    foreach (KeyValuePair<string, object> field in g.Attributes)
                    {
                        if (field.Key.ToLower().Contains(Strings.Name.ToLower()))
                        {
                            displayField = field.Key;
                            break;
                        }
                    }

                    if (Strings.Name != "Name" && displayField == null)
                    {
                        // Check if there is a field name that contains "Name" (un-localized)
                        foreach (KeyValuePair<string, object> field in g.Attributes)
                        {
                            if (field.Key.ToLower().Contains("name"))
                            {
                                displayField = field.Key;
                                break;
                            }
                        }
                    }

                    if (displayField == null)
                    {
                        // No field with "Name" (localized or unlocalized) in the field name exists, so just use the first string field

                        // Loop through the fields
                        foreach (KeyValuePair<string, object> field in g.Attributes)
                        {
                            // Look for the first graphic with a non-null value for the field
                            foreach (Graphic graphic in layer.Graphics)
                            {
                                // Make sure the current graphic contains the field - graphics in a layer may have different fields
                                if (!graphic.Attributes.ContainsKey(field.Key))
                                    break;

                                // Get the value
                                object val = graphic.Attributes[field.Key];
                                if (val != null) // if the value is non-null, check whether it is a string
                                {
                                    if (val is string) // it's a string, so use it as the display field
                                        displayField = field.Key;

                                    break;
                                }

                                // If the display field has been found, don't iterate any more graphics
                                if (displayField != null)
                                    break;
                            }
                        }
                    }

                    if (displayField == null)
                    {
                        // No string fields exist, so just use the first field
                        displayField = g.Attributes.First().Key;
                    }
                }
            }

            return displayField;
        }
    }
}
