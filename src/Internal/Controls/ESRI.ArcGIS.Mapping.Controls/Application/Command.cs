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
using System.Windows.Threading;

namespace ESRI.ArcGIS.Mapping.Controls
{
    public abstract class Command : ICommand
    {
        public Command() 
        { 
        }

        public Command(Dispatcher uiThreadDispatcher)
        {
            UIThreadDispatcher = uiThreadDispatcher;
        }
        
        #region ICommand Members

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

#pragma warning disable 0067
        public virtual event EventHandler CanExecuteChanged;
#pragma warning restore 0067

        public abstract void Execute(object parameter);

        #endregion

        public Dispatcher UIThreadDispatcher { get; set; }
        public View View { get; set; }
    }
}
