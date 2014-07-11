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
using ESRI.ArcGIS.Client.Application.Controls;

namespace ESRI.ArcGIS.Mapping.Builder.Controls
{
    public class AddToolbarItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string IconUrl { get; set; }
        public Type ToolbarItemType { get; set; }
    }

    public static class ToolbarManagement
    {
        internal static ButtonDisplayInfo CreateButtonDisplayInfoForTreeViewItem(TreeViewItem item)
        {
            return CreateButtonDisplayInfoForToolbarItem(GetAssociatedToolbarItem(item));
        }

        internal static ButtonDisplayInfo CreateButtonDisplayInfoForToolbarItem(AddToolbarItem item)
        {
            ButtonDisplayInfo bdi = new ButtonDisplayInfo();
            bdi.Description = item.Description;

            // If no icon location is available, default to a "screwdriver and wrench" image by default
            if (!String.IsNullOrEmpty(item.IconUrl))
            {
                bdi.Icon = item.IconUrl;
            }
            else
            {
                bdi.Icon = GetDefaultIconUrl(item.ToolbarItemType);
            }

            bdi.Label = item.Name;
            return bdi;
        }

        internal static string GetDefaultIconUrl(Type commandType)
        {
            // If the tool comes from an external source (user created) then default to "screwdriver and wrench"
            // icon. If it comes from an internal source (ESRI created) then there better be an image that has
            // the same name as the tool itself.
            if (commandType != null && (commandType.FullName.StartsWith("ESRI.ArcGIS.Mapping.", StringComparison.Ordinal) == true ||
                commandType.FullName.StartsWith("ESRI.ArcGIS.Client.Application.Controls.Editor.", StringComparison.Ordinal) == true))
                return String.Format("Images/toolbar/{0}16.png", commandType.Name);
            else
                return "/ESRI.ArcGIS.Mapping.Controls;component/images/icon_tools16.png";
        }

        internal static string GetDefaultGroupIconUrl()
        {
            return "/ESRI.ArcGIS.Mapping.Controls;component/images/icon_tools16.png";
        }

        internal static AddToolbarItem CreateToolbarItemForType(Type t)
        {
            // Get all custom attributes associated with this type but do not gather those that might be obtained
            // via inheritance.
            object[] attrs = t.GetCustomAttributes(false);

            // Create object to store information
            AddToolbarItem cmd = new AddToolbarItem();

            // Process each attribute, looking for the ones we care about and if found, extract information and continue
            // to the next attribute.
            foreach (object att in attrs)
            {
                ESRI.ArcGIS.Client.Extensibility.CategoryAttribute catAttribute = att as ESRI.ArcGIS.Client.Extensibility.CategoryAttribute;
                if (catAttribute != null)
                {
                    cmd.Category = catAttribute.Category;
                    continue;
                }

                ESRI.ArcGIS.Client.Extensibility.DisplayNameAttribute nameAttribute = att as ESRI.ArcGIS.Client.Extensibility.DisplayNameAttribute;
                if (nameAttribute != null)
                {
                    cmd.Name = nameAttribute.Name;
                    continue;
                }

                ESRI.ArcGIS.Client.Extensibility.DescriptionAttribute descAttribute = att as ESRI.ArcGIS.Client.Extensibility.DescriptionAttribute;
                if (descAttribute != null)
                {
                    cmd.Description = descAttribute.Description;
                    continue;
                }

                ESRI.ArcGIS.Client.Extensibility.DefaultIconAttribute iconAttribute = att as ESRI.ArcGIS.Client.Extensibility.DefaultIconAttribute;
                if (iconAttribute != null)
                {
                    cmd.IconUrl = iconAttribute.DefaultIcon;
                    continue;
                }
            }
            return cmd;
        }

        #region AssociatedToolbarItem
        /// <summary>
        /// Gets the value of the AssociatedToolbarItem attached property for a specified TreeViewItem.
        /// </summary>
        /// <param name="element">The TreeViewItem from which the property value is read.</param>
        /// <returns>The AssociatedToolbarItem property value for the TreeViewItem.</returns>
        internal static AddToolbarItem GetAssociatedToolbarItem(TreeViewItem element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return element.GetValue(AssociatedToolbarItemProperty) as AddToolbarItem;
        }

        /// <summary>
        /// Sets the value of the AssociatedToolbarItem attached property to a specified TreeViewItem.
        /// </summary>
        /// <param name="element">The TreeViewItem to which the attached property is written.</param>
        /// <param name="value">The needed AssociatedToolbarItem value.</param>
        internal static void SetAssociatedToolbarItem(TreeViewItem element, AddToolbarItem value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(AssociatedToolbarItemProperty, value);
        }

        /// <summary>
        /// Identifies the AssociatedToolbarItem dependency property.
        /// </summary>
        public static readonly DependencyProperty AssociatedToolbarItemProperty =
            DependencyProperty.RegisterAttached(
                "AssociatedToolbarItem",
                typeof(AddToolbarItem),
                typeof(ToolbarManagement),
                new PropertyMetadata(null));
        #endregion 
    }
}
