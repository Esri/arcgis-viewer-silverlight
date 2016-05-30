/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Interactivity;

namespace MeasureTool.Addins
{
    /// <summary>
    /// Attaches a behavior to the targeted object
    /// </summary>
    public class AttachBehaviorAction : TargetedTriggerAction<DependencyObject>
    {
        /// <summary>
        /// Raised when the action is executed.  Attempts to add the Behavior to the target object's
        /// behaviors collection.
        /// </summary>
        /// <param name="parameter"></param>
        protected override void Invoke(object parameter)
        {
            // Make sure that both the target and behavior are non-null, and that the target matches the
            // behavior's target type
            if ((Target != null) && (Behavior != null) && isTargetTypeValid(Target, Behavior.GetType()))
            {
                // Add the behavior to the target object's behaviors collection
                BehaviorCollection behaviors = Interaction.GetBehaviors(Target);
                if (!behaviors.Contains(Behavior))
                    behaviors.Add(Behavior);
            }
        }

        /// <summary>
        /// Raised when the action's target object changes.  Removes the behavior from the previous target
        /// and adds it to the new one.
        /// </summary>
        protected override void OnTargetChanged(DependencyObject oldTarget, DependencyObject newTarget)
        {
            base.OnTargetChanged(oldTarget, newTarget);

            // Detach behavior from previous target
            if (oldTarget != null)
            {
                BehaviorCollection behaviors = Interaction.GetBehaviors(oldTarget);
                if (behaviors.Contains(Behavior))
                    behaviors.Remove(Behavior);
            }

            // Attach behavior to the new target, making sure that the new target is of the behavior's
            // target type.
            if (newTarget != null && isTargetTypeValid(newTarget, Behavior.GetType()))
            {
                BehaviorCollection behaviors = Interaction.GetBehaviors(newTarget);
                if (!behaviors.Contains(Behavior))
                    behaviors.Add(Behavior);
            }
        }

        #region Dependency Properties

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Behavior"/> property
        /// </summary>
        public static DependencyProperty BehaviorProperty = DependencyProperty.Register(
            "Behavior", typeof(Behavior), typeof(AttachBehaviorAction), null);

        /// <summary>
        /// The <see cref="System.Windows.Interactivity.Behavior" /> to attach
        /// </summary>
        public Behavior Behavior
        {
            get { return GetValue(BehaviorProperty) as Behavior; }
            set { SetValue(BehaviorProperty, value);}
        }

        #endregion Dependency Properties

        /// <summary>
        /// Checks whether the specified target fits the type constraint of the passed-in type
        /// </summary>
        private bool isTargetTypeValid(DependencyObject target, Type type)
        {
            // Check whether the passed-in type has a type constraint (i.e. is generic)
            if (type.IsGenericType)
            {
                // Get the type constraints
                Type[] typeConstraints = type.GetGenericArguments();

                // Check the target's type against the type constraints
                bool isValidTargetType = false;
                foreach (Type t in typeConstraints)
                {
                    if (t.IsAssignableFrom(target.GetType()))
                    {
                        // The target is within the type constraint (i.e. is the same type or
                        // a subclass)
                        isValidTargetType = true;
                        break;
                    }
                }

                return isValidTargetType;
            }
            else if (type.BaseType != null) // Check the base type
            {
                return isTargetTypeValid(target, type.BaseType);
            }
            else // No type constraint found
            {
                return true;
            }
        }
    }
}
