/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Collections.Generic;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Provides members to manipulate application-specific layer properties
    /// </summary>
    public class LayerProperties
    {
        #region IsVisibleInMapContents
        /// <summary>
        /// Gets whether the layer is visible in the application's list of map contents
        /// </summary>
        /// <param name="layer">The layer to get visibility for</param>
        /// <returns>True if the layer is visible, false if not</returns>
        public static bool GetIsVisibleInMapContents(Layer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            return (bool)layer.GetValue(IsVisibleInMapContentsProperty);
        }

        /// <summary>
        /// Sets whether the layer is visible in the application's list of map contents
        /// </summary>
        /// <param name="layer">The layer to set visibility for</param>
        /// <param name="value">Whether or not the layer should be visible.  True if so, false if not.</param>
        public static void SetIsVisibleInMapContents(Layer layer, bool value)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            layer.SetValue(IsVisibleInMapContentsProperty, value);
        }

        /// <summary>
        /// Identifies the IsVisibleInMapContents attached DependencyProperty
        /// </summary>
        public static readonly DependencyProperty IsVisibleInMapContentsProperty =
            DependencyProperty.RegisterAttached("IsVisibleInMapContents", typeof(bool), typeof(Layer), new PropertyMetadata(true));
        #endregion

        #region IsEditable

        /// <summary>
        /// Identifies the IsEditable attached DependencyProperty
        /// </summary>
        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.RegisterAttached(
            "IsEditable", typeof (bool), typeof (Layer), new PropertyMetadata(false));

        /// <summary>
        /// Sets whether editing has been enabled on a layer at the application level
        /// </summary>
        /// <param name="layer">The layer to enable or disable editing for</param>
        /// <param name="value">Whether or not editing should be enabled</param>
        internal static void SetIsEditable(Layer layer, bool value)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            layer.SetValue(IsEditableProperty, value);
        }

        /// <summary>
        /// Gets whether editing has been enabled on a layer at the application level
        /// </summary>
        /// <param name="layer">The layer to get the editability of</param>
        /// <returns>Whether or not editing is enabled</returns>
        public static bool GetIsEditable(Layer layer)
        {
            if (layer == null)
            {
                throw new ArgumentNullException("layer");
            }
            bool rtn = (bool)layer.GetValue(IsEditableProperty);
            return rtn;
        }

        #endregion

		#region IsPopupEnabled
        /// <summary>
        /// Identifies the IsPopupEnabled attached DependencyProperty
        /// </summary>
		public static readonly DependencyProperty IsPopupEnabledProperty =
			DependencyProperty.RegisterAttached("IsPopupEnabled", typeof(bool), typeof(Layer), new PropertyMetadata(true));

        /// <summary>
        /// Sets whether popups are enabled on a layer
        /// </summary>
        /// <param name="layer">The layer to enable or disable popups for</param>
        /// <param name="value">Whether to enable or disable popups</param>
		public static void SetIsPopupEnabled(Layer layer, bool value)
		{
			if (layer == null)
			{
				throw new ArgumentNullException("layer");
			}
		layer.SetValue(IsPopupEnabledProperty, value);
		}

        /// <summary>
        /// Gets whether popups are enabled on a layer
        /// </summary>
        /// <param name="layer">The layer to check</param>
        /// <returns>The popup's enabled state</returns>
		public static bool GetIsPopupEnabled(Layer layer)
		{
			if (layer == null)
			{
				throw new ArgumentNullException("layer");
			}
			return (bool)layer.GetValue(IsPopupEnabledProperty);
		}
		#endregion

		#region PopupDataTemplates
        /// <summary>
        /// Gets the XAML data templates used to display the contents of a layer's popups.
        /// </summary>
        /// <param name="obj">The layer to retrieve templates for</param>
        /// <returns>Dictionary with integer keys and string values.  Integer keys are layer ids: -1 for a GraphicLayer, otherwise Layer ID.
        /// String values are XAML data template strings on which XamlReader.Load can be called to create a DataTemplate.</returns>
        public static IDictionary<int, string> GetPopupDataTemplates(Layer obj)
		{
			if (obj == null) return null;// -1 is Graphic Layer, zero is a valid layer id.
			return (Dictionary<int, string>)obj.GetValue(PopupDataTemplatesProperty);
		}

        /// <summary>
        /// Sets the XAML data templates used to display the contents of a layer's popups.
        /// </summary>
        /// <param name="obj">The layer to specify templates for</param>
        /// <param name="value">Dictionary with integer keys and string values.  Integer keys are layer ids: -1 for a GraphicLayer, otherwise Layer ID.
        /// String values are XAML data template strings on which XamlReader.Load can be called to create a DataTemplate.</param>
        public static void SetPopupDataTemplates(Layer obj, IDictionary<int, string> value)
		{
			if (obj != null)
				obj.SetValue(PopupDataTemplatesProperty, value);
		}

        /// <summary>
        /// Identifies the PopupDataTemplates attached DependencyProperty
        /// </summary>
        public static readonly DependencyProperty PopupDataTemplatesProperty =
            DependencyProperty.RegisterAttached("PopupDataTemplates", typeof(IDictionary<int, string>), typeof(Layer), new PropertyMetadata(null));
		#endregion

		#region PopupTitleExpressions
		/// <summary>
		/// Gets the title expressions used to display the titles on a layer's popups.
		/// </summary>
		/// <param name="obj">The layer to set title expressions for</param>
		/// <returns>Dictionary with integer keys and string values.  Integer keys are layer ids: -1 for a GraphicLayer, otherwise Layer ID.
        /// String values are format strings like this: "{NAME}: {POPULATION}"</returns>
        public static IDictionary<int, string> GetPopupTitleExpressions(Layer obj)
		{
			if (obj == null) return null;// -1 is Graphic Layer, zero is a valid layer id.
			return (Dictionary<int, string>)obj.GetValue(PopupTitleExpressionsProperty);
		}

		/// <summary>
        /// Sets the title expressions used to display the titles on a layer's popups.
        /// </summary>
        /// <param name="obj">The layer to get title expressions for</param>
        /// <param name="value">Dictionary with integer keys and string values.  Integer keys are layer ids: -1 for a GraphicLayer, otherwise Layer ID.
        /// String values are format strings like this: "{NAME}: {POPULATION}"</param>
        public static void SetPopupTitleExpressions(Layer obj, IDictionary<int, string> value)
		{
			if (obj != null)
				obj.SetValue(PopupTitleExpressionsProperty, value);
		}

        /// <summary>
        /// Identifies the PopupTitleExpressions attached DependencyProperty
        /// </summary>
        public static readonly DependencyProperty PopupTitleExpressionsProperty =
			DependencyProperty.RegisterAttached("PopupTitleExpressions", typeof(IDictionary<int, string>), typeof(Layer), new PropertyMetadata(null));
		#endregion
    }
}
