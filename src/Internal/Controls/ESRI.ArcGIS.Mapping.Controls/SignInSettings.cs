/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System;
using System.Windows;
using ESRI.ArcGIS.Mapping.Core;
using System.Xml.Linq;
using System.Linq;
using ESRI.ArcGIS.Mapping.Controls.ArcGISOnline;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace ESRI.ArcGIS.Mapping.Controls
{
  public class SignInSettings : INotifyPropertyChanged
  {
    public SignInSettings()
    {
      current = this;
      signInButtonSource = new BitmapImage()
            {
              UriSource = new Uri("/ESRI.ArcGIS.Mapping.Controls;component/Images/toolbar/SignIn32.png", UriKind.Relative)
            };
    }
    static SignInSettings current;
    public static SignInSettings Current
    {
      get { if (current == null) current = new SignInSettings(); return current; }
    }
    string status = "Sign In";
    public string SignInStatus
    {
      get { return status; }
      set
      {
        status = value;
        raisePropChanged("SignInStatus");
      }
    }

    string user;
    public string SignedInUser
    {
      get { return user; }
      set
      {
        user = value;
        raisePropChanged("SignedInUser");
      }
    }

    BitmapImage signInButtonSource;
    public BitmapImage SignInButtonSource
    {
      get { return signInButtonSource; }
      set
      {
        signInButtonSource = value;
        raisePropChanged("SignInButtonSource");
      }
    }

    Visibility userVisibility;
    public Visibility UserNameVisibility
    {
      get { return userVisibility; }
      set
      {
        userVisibility = value;
        raisePropChanged("UserNameVisibility");
      }
    }

    double buttonWidth = 60;
    public double ButtonWidth
    {
      get { return buttonWidth; }
      set
      {
        buttonWidth = value;
        raisePropChanged("ButtonWidth");
      }
    }

    void raisePropChanged(string property)
    {
      if (PropertyChanged != null)
        PropertyChanged(null, new PropertyChangedEventArgs(property));
    }

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion
  }
}
