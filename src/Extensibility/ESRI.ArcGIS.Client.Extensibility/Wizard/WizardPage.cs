/*
(c) Copyright ESRI.
This source is subject to the Microsoft Public License (Ms-PL).
Please see https://opensource.org/licenses/ms-pl for details.
All other rights reserved.
*/

using System.ComponentModel;
using System.Windows;

namespace ESRI.ArcGIS.Client.Extensibility
{
    /// <summary>
    /// Represents a page in a wizard
    /// </summary>
    public class WizardPage : DependencyObject
    {
        /// <summary>
        /// Identifies the <see cref="Heading"/> DependencyProperty
        /// </summary>
        public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(
            "Heading", typeof(object), typeof(WizardPage), null);

        /// <summary>
        /// Gets or sets the main descriptive text for the wizard page
        /// </summary>
        public object Heading
        {
            get { return GetValue(HeadingProperty); }
            set { SetValue(HeadingProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Description"/> DependencyProperty
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description", typeof(object), typeof(WizardPage), null);

        /// <summary>
        /// Gets or sets supplementary descriptive text for the wizard page
        /// </summary>
        public object Description
        {
            get { return GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Content"/> DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(object), typeof(WizardPage), null);

        /// <summary>
        /// Gets or sets content of the wizard page
        /// </summary>
        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }


        /// <summary>
        /// Identifies the <see cref="InputValid"/> DependencyProperty
        /// </summary>
        public static readonly DependencyProperty InputValidProperty = DependencyProperty.Register(
            "InputValid", typeof(bool), typeof(WizardPage), null);

        /// <summary>
        /// Gets or sets whether the input on the page is valid.  The wizard host can use 
        /// this to determine whether the wizard can proceed to the next page or be completed.
        /// </summary>
        public bool InputValid
        {
            get { return (bool)GetValue(InputValidProperty); }
            set { SetValue(InputValidProperty, value); }
        }
    }
}
