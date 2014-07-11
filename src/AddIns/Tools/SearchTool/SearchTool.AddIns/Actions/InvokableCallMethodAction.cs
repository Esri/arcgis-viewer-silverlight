/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using Microsoft.Expression.Interactivity.Core;

namespace SearchTool
{
    /// <summary>
    /// Action that exposes CallMethodAction.Invoke to allow calling it programmatically
    /// </summary>
    public class InvokableCallMethodAction : CallMethodAction
    {
        /// <summary>
        /// Executes the method specified by the MethodName property on the targeted object
        /// </summary>
        public new void Invoke(object parameter)
        {
            base.Invoke(parameter);
        }
    }
}
