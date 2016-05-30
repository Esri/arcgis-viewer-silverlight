/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace QueryRelatedRecords.AddIns
{

    /// <summary>
    /// The throttle timer is useful for limiting the number of requests to a method if
    /// the method is repeatly called many times but you only want the method raised once.
    /// It delays raising the method until a set interval, and any previous calls to the
    /// actions in that interval will be cancelled.
    /// </summary>
    internal class ThrottleTimer
    {
        System.Windows.Threading.DispatcherTimer throttleTimer;

        internal ThrottleTimer(int milliseconds) : this(milliseconds, null) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottleTimer"/> class.
        /// </summary>
        /// <param name="milliseconds">Milliseconds to throttle.</param>
        /// <param name="handler">The delegate to invoke.</param>
        internal ThrottleTimer(int milliseconds, Action handler)
        {
            Action = handler;
            throttleTimer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(milliseconds) };
            throttleTimer.Tick += (s, e) =>
            {
                if (Action != null)
                    Action.Invoke();
                throttleTimer.Stop();
            };
        }

        /// <summary>
        /// Delegate to Invoke.
        /// </summary>
        /// <value>The action.</value>
        public Action Action { get; set; }

        /// <summary>
        /// Invokes this instance (note that this will happen asynchronously and delayed).
        /// </summary>
        public void Invoke()
        {
            throttleTimer.Stop();
            throttleTimer.Start();
        }

        /// <summary>
        /// Cancels this timer if running.
        /// </summary>
        internal void Cancel()
        {
            throttleTimer.Stop();
        }
    }

}
