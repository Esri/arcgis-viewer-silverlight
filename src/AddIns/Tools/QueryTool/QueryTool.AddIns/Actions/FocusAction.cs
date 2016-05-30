/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.Windows.Controls;
using System.Windows.Interactivity;

namespace QueryTool.AddIns
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
