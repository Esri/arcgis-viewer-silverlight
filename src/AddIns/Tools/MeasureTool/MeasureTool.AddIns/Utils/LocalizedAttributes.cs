/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using ESRI.ArcGIS.Client.Extensibility;
namespace MeasureTool.Addins
{
    /// <summary>
    /// Defines a description by looking up a string resource with the specified name
    /// </summary>
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        public LocalizedDescriptionAttribute(string desc) : base(desc) 
        {
            this.Description = ResourceHelper.GetStringResource(desc); 
        }
    }

    /// <summary>
    /// Defines a description by looking up a string resource with the specified name
    /// </summary>
    public class LocalizedPropertyDescriptionAttribute : System.ComponentModel.DescriptionAttribute
    {
        public LocalizedPropertyDescriptionAttribute(string desc)
            : base(desc)
        {
            _description = ResourceHelper.GetStringResource(desc);
        }

        private string _description;
        public override string Description
        {
            get { return _description; }
        }
    }

    /// <summary>
    /// Defines a display name by looking up a string resource with the specified name
    /// </summary>
    public class LocalizedDisplayNameAttribute : DisplayNameAttribute
    {
        public LocalizedDisplayNameAttribute(string displayName) : base(displayName)
        {
            this.Name = ResourceHelper.GetStringResource(displayName);
        }
    }

    /// <summary>
    /// Defines a category by looking up a string resource with the specified name
    /// </summary>
    public class LocalizedCategoryAttribute : CategoryAttribute
    {
        public LocalizedCategoryAttribute(string category) : base(category)
        {
            this.Category = ResourceHelper.GetStringResource(category);
        }
    }
}
