/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows.Controls;
using System.Windows.Interactivity;

namespace SearchTool
{
    /// <summary>
    /// Puts focus on the associated control
    /// </summary>
    public class FocusAction : TargetedTriggerAction<Control>
    {
        protected override void Invoke(object parameter)
        {
            if (Target != null)
                Target.Focus();
        }
    }
}
