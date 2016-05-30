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

namespace ESRI.ArcGIS.Mapping.Core
{
    public class ThemeColorSet
    {
        private Color textBackgroundDark1 = Color.FromArgb(255, 0, 0, 0);
        public Color TextBackgroundDark1
        {
            get { return textBackgroundDark1; }
            set { textBackgroundDark1 = value; }
        }

        private Color textBackgroundLight1 = Color.FromArgb(255, 255, 255, 255);
        public Color TextBackgroundLight1
        {
            get { return textBackgroundLight1; }
            set { textBackgroundLight1 = value; }
        }

        private Color textBackgroundDark2 = Color.FromArgb(255, 0x1F, 0x49, 0x7D);
        public Color TextBackgroundDark2
        {
            get { return textBackgroundDark2; }
            set { textBackgroundDark2 = value; }
        }

        private Color textBackgroundLight2  = Color.FromArgb(255, 0xEE,0xEC, 0xE1);
        public Color TextBackgroundLight2
        {
            get { return textBackgroundLight2; }
            set { textBackgroundLight2 = value; }
        }

        private Color accent1 = Color.FromArgb(255, 0x4F, 0x81, 0xBD);
        public Color Accent1
        {
            get { return accent1; }
            set { accent1 = value; }
        }

        private Color accent2 = Color.FromArgb(255, 0xC0, 0x50, 0x4D);
        public Color Accent2
        {
            get { return accent2; }
            set { accent2 = value; }
        }

        private Color accent3 = Color.FromArgb(255, 0x9B, 0xBB, 0x59);
        public Color Accent3
        {
            get { return accent3; }
            set { accent3 = value; }
        }

        private Color accent4 = Color.FromArgb(255, 0x80, 0x64, 0xA2);
        public Color Accent4
        {
            get { return accent4; }
            set { accent4 = value; }
        }

        private Color accent5 = Color.FromArgb(255, 0x4B, 0xAC, 0xC6);
        public Color Accent5
        {
            get { return accent5; }
            set { accent5 = value; }
        }

        private Color accent6 = Color.FromArgb(255, 0xF7, 0x96, 0x46);
        public Color Accent6
        {
            get { return accent6; }
            set { accent6 = value; }
        }

        private Color hyperlink = Color.FromArgb(255, 0, 0, 0xFF);
        public Color Hyperlink
        {
            get { return hyperlink; }
            set { hyperlink = value; }
        }

        private Color followedHyperlink = Color.FromArgb(255, 0x80, 0, 0x80);
        public Color FollowedHyperlink
        {
            get { return followedHyperlink; }
            set { followedHyperlink = value; }
        }
    }
}
