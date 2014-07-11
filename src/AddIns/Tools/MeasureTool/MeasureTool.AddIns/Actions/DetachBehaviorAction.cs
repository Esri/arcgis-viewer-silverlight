/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
All other rights reserved.
*/

using System.Windows;
using System.Windows.Interactivity;
using ESRI.ArcGIS.Client;
using ESRI.ArcGIS.Client.Geometry;
using System.Collections.Generic;
using System;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Detaches all behaviors of the specified type from the target object
    /// </summary>
    public class DetachBehaviorAction : TargetedTriggerAction<DependencyObject>
    {
        protected override void Invoke(object parameter)
        {
            if ((Target != null) && (BehaviorType != null))
            {
                BehaviorCollection behaviors = Interaction.GetBehaviors(Target);
                if (BehaviorType != null)
                {
                    int i = 0;
                    while (i < behaviors.Count)
                    {
                        Behavior b = behaviors[i];
                        if (b.GetType() == BehaviorType)
                            behaviors.Remove(b);
                        else
                            i++;
                    }
                }
            }
        }

        #region Dependency Properties

        /// <summary>
        /// Backing DependencyProperty for the <see cref="BehaviorType"/> property
        /// </summary>
        public static DependencyProperty BehaviorTypeProperty = DependencyProperty.Register(
            "BehaviorType", typeof(Type), typeof(DetachBehaviorAction), null);

        /// <summary>
        /// The <see cref="System.Type"/> of the behavior to detach
        /// </summary>
        public Type BehaviorType
        {
            get { return this.GetValue(BehaviorTypeProperty) as Type; }
            set { this.SetValue(BehaviorTypeProperty, value); }
        }


        #endregion Dependency Properties
    }
}
