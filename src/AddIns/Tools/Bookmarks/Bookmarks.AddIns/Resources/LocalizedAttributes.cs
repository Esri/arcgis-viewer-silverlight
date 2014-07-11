/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client.Extensibility;
namespace Bookmarks.AddIns
{
    /// <summary>
    /// Defines a description by looking up a string resource with the specified name
    /// </summary>
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        public LocalizedDescriptionAttribute(string desc) : base(desc) 
        {
            Description = StringResourcesManager.GetResource(desc); 
        }
    }

    /// <summary>
    /// Defines a display name by looking up a string resource with the specified name
    /// </summary>
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        public LocalizedDisplayNameAttribute(string displayName) : base(displayName)
        {
            Name = StringResourcesManager.GetResource(displayName);
        }
    }

    /// <summary>
    /// Defines a category by looking up a string resource with the specified name
    /// </summary>
    public class LocalizedCategoryAttribute : CategoryAttribute
    {
        public LocalizedCategoryAttribute(string category) : base(category)
        {
            Category = StringResourcesManager.GetResource(category);
        }
    }
}
