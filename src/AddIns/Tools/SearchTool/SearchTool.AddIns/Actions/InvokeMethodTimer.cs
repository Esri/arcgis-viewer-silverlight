/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Microsoft.Expression.Interactivity.Core;
using Interactivity = System.Windows.Interactivity;

namespace SearchTool
{
    /// <summary>
    /// Object that allows Invoking a method on an object with a specified delay.  Useful for invoking and
    /// cancelling methods via XAML.
    /// </summary>
    public class InvokeMethodTimer : DependencyObject
    {
        private DispatcherTimer _timer; // timer for managing the interval at which to the method
        private InvokableCallMethodAction _callMethodAction; // action used to invoke the method

        public InvokeMethodTimer()
        {
            // Create a CallMethodAction to handle invoking the method.  Use the InvokableCallMethodAction
            // class so that the Invoke method can be invoked programmatically.
            _callMethodAction = new InvokableCallMethodAction();

            // Bind the action's MethodName and TargetObject properties to the corresponding properties
            // on the InvokeMethodTimer object
            Binding b = new Binding("MethodName") { Source = this };
            BindingOperations.SetBinding(_callMethodAction, CallMethodAction.MethodNameProperty, b);

            b = new Binding("TargetObject") { Source = this };
            BindingOperations.SetBinding(_callMethodAction, CallMethodAction.TargetObjectProperty, b);

            // Add the CallMethodAction to the InvokeMethodTimer's triggers collection.  Without this,
            // invoking the action will fail silently.
            Interactivity.EventTrigger trigger = new Interactivity.EventTrigger();
            trigger.Actions.Add(_callMethodAction);
            Interactivity.TriggerCollection triggers = Interactivity.Interaction.GetTriggers(this);
            triggers.Add(trigger);

            // Initialize the timer
            _timer = new DispatcherTimer() { Interval = Delay };
            _timer.Tick += (o, e) => 
            {
                _timer.Stop();
                _callMethodAction.Invoke(null); 
            };
        }

        /// <summary>
        /// Starts the timer for invoking the method
        /// </summary>
        public void Invoke() { IsTimerRunning = true; }

        /// <summary>
        /// Cancels method invocation if the timer was running
        /// </summary>
        public void Cancel() { IsTimerRunning = false; }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="TargetObject"/> property
        /// </summary>
        public static DependencyProperty TargetObjectProperty = DependencyProperty.Register(
            "TargetObject", typeof(object), typeof(InvokeMethodTimer), null);

        /// <summary>
        /// Gets or sets the object to invoke the method on
        /// </summary>
        public object TargetObject
        {
            get { return GetValue(TargetObjectProperty) as object; }
            set { SetValue(TargetObjectProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="MethodName"/> property
        /// </summary>
        public static DependencyProperty MethodNameProperty = DependencyProperty.Register(
            "MethodName", typeof(string), typeof(InvokeMethodTimer), null);

        /// <summary>
        /// Gets or sets the name of the method to invoke
        /// </summary>
        public string MethodName
        {
            get { return GetValue(MethodNameProperty) as string; }
            set { SetValue(MethodNameProperty, value); }
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="Delay"/> property
        /// </summary>
        public static DependencyProperty DelayProperty = DependencyProperty.Register(
            "Delay", typeof(TimeSpan), typeof(InvokeMethodTimer), new PropertyMetadata(TimeSpan.FromMilliseconds(500)));

        /// <summary>
        /// Gets or sets the delay to use when invoking the method
        /// </summary>
        public TimeSpan Delay
        {
            get { return (TimeSpan)GetValue(DelayProperty); }
            set { SetValue(DelayProperty, value); }
        }

        // Fires when the Delay property is changed.  Updates the interval on the method timer.
        private static void OnDelayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Update the timer with the new interval
            DispatcherTimer methodTimer = ((InvokeMethodTimer)d)._timer;
            if (e.NewValue != null)
                methodTimer.Interval = (TimeSpan)e.NewValue;            
        }

        /// <summary>
        /// Backing DependencyProperty for the <see cref="IsTimerRunning"/> property
        /// </summary>
        public static DependencyProperty IsTimerRunningProperty = DependencyProperty.Register(
            "IsTimerRunning", typeof(bool), typeof(InvokeMethodTimer), new PropertyMetadata(OnIsTimerRunningPropertyChanged));

        /// <summary>
        /// Gets or sets whether the timer to invoke the method is running.  The timer can be started or stopped by 
        /// manipulating this property.
        /// </summary>
        public bool IsTimerRunning
        {
            get { return (bool)GetValue(IsTimerRunningProperty); }
            set { SetValue(IsTimerRunningProperty, value); }
        }

        // Fires when the IsTimerRunning property is changed.  Updates the state of the timer.
        private static void OnIsTimerRunningPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Start or stop the timer based on the new property value
            DispatcherTimer methodTimer = ((InvokeMethodTimer)d)._timer;
            if ((bool)e.NewValue && !methodTimer.IsEnabled)
                methodTimer.Start();
            else if (!(bool)e.NewValue && methodTimer.IsEnabled)
                methodTimer.Stop();
        }
    }
}
