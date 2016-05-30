/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Collections;
using System.Windows;
using System.Windows.Interactivity;

namespace SearchTool
{
    /// <summary>
    /// Selects all the items in a collection.  Uses each item's IsSelected or Selected property if available, 
    /// or otherwise sets the <see cref="Properties.IsSelected"/> attached property.
    /// </summary>
    public class SelectAllAction : TargetedTriggerAction<IEnumerable>
    {
        protected override void Invoke(object parameter)
        {
            if (Target != null)
            {
                foreach (object o in Target)
                {
                    if (o.PropertyExists("IsSelected")) // check if the object has an IsSelected property
                        ((dynamic)o).IsSelected = true;
                    else if (o.PropertyExists("Selected")) // check if the object has a Selected property
                        ((dynamic)o).Selected = true;
                    else if (o is DependencyObject) // use an attached property
                        Properties.SetIsSelected((DependencyObject)o, true);
                }
            }
        }
    }
}
