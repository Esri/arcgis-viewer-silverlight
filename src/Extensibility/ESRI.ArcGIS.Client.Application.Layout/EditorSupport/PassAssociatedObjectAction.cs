/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client.Application.Layout.EditorSupport;

namespace ESRI.ArcGIS.Client.Application.Layout
{
    // Passes the action's AssociatedObject to the TargetObject's ObjectReceived method.  Requires that the 
    // TargetObject implement IReceiveObject. 
    /// <summary>
    /// Internal use only.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class PassAssociatedObjectAction : TargetedTriggerAction<DependencyObject>
    {
        /// <summary>
        /// Invokes the action
        /// </summary>
        /// <param name="parameter">Not used</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected override void Invoke(object parameter)
        {
            if (TargetObject is IReceiveObject)
                ((IReceiveObject)TargetObject).ReceiveObject(AssociatedObject);
        }

        /// <summary>
        /// Called when the Target property chnages
        /// </summary>
        /// <param name="oldTarget"></param>
        /// <param name="newTarget"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]      
        protected override void OnTargetChanged(DependencyObject oldTarget, DependencyObject newTarget)
        {
            if (newTarget is IReceiveObject || AssociatedObject.Equals(newTarget) || newTarget == null)
                base.OnTargetChanged(oldTarget, newTarget);
            else
                throw new Exception("Action target must implement IReceiveObject");
        }
    }
}
