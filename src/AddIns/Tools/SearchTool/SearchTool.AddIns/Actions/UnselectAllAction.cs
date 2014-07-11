/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Collections;
using System.Windows;
using System.Windows.Interactivity;

namespace SearchTool
{
    /// <summary>
    /// Unselects all the items in a collection.  Uses each item's IsSelected or Selected property if available, 
    /// or otherwise sets the <see cref="Properties.IsSelected"/> attached property.
    /// </summary>
    public class UnselectAllAction : TargetedTriggerAction<IList>
    {
        protected override void Invoke(object parameter)
        {
            if (Target != null)
            {
                foreach (object o in Target)
                {
                    if (o.PropertyExists("IsSelected")) // check whether the object has an IsSelected property
                        ((dynamic)o).IsSelected = false;
                    else if (o.PropertyExists("Selected")) // check whether the object has a Selected property
                        ((dynamic)o).Selected = false;
                    else if (o is DependencyObject) // use the IsSelected attached property
                        Properties.SetIsSelected((DependencyObject)o, false);
                }
            }
        }
    }
}
