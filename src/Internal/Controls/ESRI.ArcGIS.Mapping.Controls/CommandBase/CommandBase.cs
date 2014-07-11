/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
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
using System.ComponentModel;

namespace ESRI.ArcGIS.Mapping.Controls
{
    // NOTE:
    //
    // This class "CommandBase" is referenced via reflection when the Builder tool determines the list of ICommand classes
    // contained in this assembly. Changing the name, namespace, or location of this class will cause this code in the
    // builder application to no longer be able to detect these tools (i.e. don't do that!).
    public abstract class CommandBase : ICommand, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public abstract void Execute(object parameter);

        protected virtual void OnCanExecuteChanged(EventArgs args)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, args);
        }

        protected void RaiseCanExecuteChangedEvent(object sender, EventArgs args)
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(sender, args);
        }
    }
}
