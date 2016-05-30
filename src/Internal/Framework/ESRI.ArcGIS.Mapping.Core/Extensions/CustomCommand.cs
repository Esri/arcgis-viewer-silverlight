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
using System.Collections.Generic;

namespace ESRI.ArcGIS.Mapping.Core
{
    public class CustomCommand
    {
        public string ID { get; set; }
        public string AssemblyName { get; set; }
        public string TypeName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string ConfigString { get; set; }
        public string SmallThumbnailImageUrl { get; set; }
        public string LargeThumbnailImageUrl { get; set; }
        public ConfigurationType ConfigurationType { get; set; }
        public ButtonType ButtonType { get; set; }
    }

    public class CustomCommandGroup
    {
        public string ID { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string VisiblityContext { get; set; }
        public IEnumerable<CustomCommand> Commands { get; set; }
    }

    public class CustomCommandTab
    {
        public string ID { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string VisiblityContext { get; set; }
        public IEnumerable<CustomCommandGroup> Groups { get; set; }
    }

    public enum ButtonType
    {
        Button,
        ToggleButton,
        CheckBox
    }
}
